using AutoMapper;
using AutoMapper.QueryableExtensions;
using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Medication_Reminder_API.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly IMedicationRepository _repo;
        private readonly IMapper _mapper;
        private readonly IDoseLogService _logService;
        private readonly IMemoryCache _cache;

        public MedicationService(IMedicationRepository repo, IMapper mapper, IDoseLogService logService, IMemoryCache memoryCache)
        {
            _repo = repo;
            _mapper = mapper;
            _logService = logService;
            _cache = memoryCache;
        }

        public async Task<PagedResult<MedicationDTO>> GetAllMedicationsAsync(int page = 1, int pageSize = 5)
        {
            const string cacheKey = "all_medications";

            if (!_cache.TryGetValue(cacheKey, out List<MedicationDTO> cachedMeds))
            {
                var meds = await _repo.GetAll().ToListAsync();
                cachedMeds = _mapper.Map<List<MedicationDTO>>(meds);
                _cache.Set(cacheKey, cachedMeds, TimeSpan.FromMinutes(10));
            }

            var totalcount = cachedMeds.Count;
            var totalpages = (int)Math.Ceiling(totalcount / (double)pageSize);
            var pagedMeds = cachedMeds.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<MedicationDTO>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalcount,
                TotalPages = totalpages,
                Data = pagedMeds
            };
        }

        public async Task<List<MedicationDTO>> GetByNameAsync(string name)
        {
            var meds = await _repo.GetByNameAsync(name);
            return _mapper.Map<List<MedicationDTO>>(meds);
        }

        public async Task<List<MedicationDTO>> GetAllMedicationsForPatientAsync(int patientId)
        {
            var meds = await _repo.GetAllForPatientAsync(patientId);
            return _mapper.Map<List<MedicationDTO>>(meds);
        }

        public async Task<MedicationDTO> AddAsync(MedicationDTO dto)
        {
            if (await _repo.ExistsByNameAsync(dto.Name))
                throw new InvalidOperationException($"Medication with name '{dto.Name}' already exists.");

            var med = _mapper.Map<Medication>(dto);
            med.Status = MedicationStatus.Ongoing;

            await _repo.AddAsync(med);
            await _repo.SaveChangesAsync();
            _cache.Remove("all_medications");

            return _mapper.Map<MedicationDTO>(med);
        }

        public async Task<MedicationDTO?> EditMedicationAsync(int id, MedicationDTO dto)
        {
            var med = await _repo.GetByIdAsync(id);
            if (med == null) return null;

            _mapper.Map(dto, med);
            await _repo.SaveChangesAsync();
            _cache.Remove("all_medications");

            return _mapper.Map<MedicationDTO>(med);
        }

        public async Task<MedicationDTO?> DeleteMedicationAsync(int id)
        {
            var med = await _repo.GetByIdAsync(id);
            if (med == null) return null;

            await _repo.DeleteAsync(med);
            await _repo.SaveChangesAsync();
            _cache.Remove("all_medications");

            return _mapper.Map<MedicationDTO>(med);
        }

        public async Task<MedicationDTO?> UpdateStatusAsync(int medicationId)
        {
            var med = await _repo.GetByIdAsync(medicationId);
            if (med == null) return null;

            var totalScheduled = await _repo.CountDoseLogsAsync(med.MedicationID);
            var takenCount = await _repo.CountTakenDoseLogsAsync(med.MedicationID);

            med.Status = takenCount > totalScheduled
                ? MedicationStatus.Overtaken
                : (takenCount == totalScheduled ? MedicationStatus.Completed : MedicationStatus.Ongoing);

            await _repo.SaveChangesAsync();
            _cache.Remove("all_medications");

            return _mapper.Map<MedicationDTO>(med);
        }
    }
}
