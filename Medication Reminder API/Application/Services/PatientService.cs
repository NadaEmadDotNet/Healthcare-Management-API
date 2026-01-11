using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using AutoMapper.QueryableExtensions;


public class PatientService : IPatientService
{
    private readonly IPatientRepository _repo;
    private readonly IMapper _mapper;

    public PatientService(IPatientRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PagedResult<PatientDto>> GetAllPatientsAsync(
        string? doctorId, string? caregiverId, string? patientId,
        int page = 1, int pageSize = 10)
    {
        var patients = _repo.GetAll();

        // نفس logic الفيلتر
        if (!string.IsNullOrEmpty(patientId))
            patients = patients.Where(p => p.UserId == patientId);

        var totalCount = await patients.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await patients
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<PatientDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<PatientDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Data = data
        };
    }

    public async Task<List<PatientDto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var patients = await _repo.GetByIdsAsync(ids);
        return _mapper.Map<List<PatientDto>>(patients);
    }

    public async Task<List<PatientDto>> GetByNameAsync(string name)
    {
        var patients = await _repo.GetByNameAsync(name);
        return _mapper.Map<List<PatientDto>>(patients);
    }

    public async Task<PatientDto> AddAsync(PatientDto dto)
    {
        var patient = _mapper.Map<Patient>(dto);
        await _repo.AddAsync(patient);
        await _repo.SaveChangesAsync();
        return _mapper.Map<PatientDto>(patient);
    }

    public async Task<PatientDto?> EditPatientAsync(int id, PatientDto dto)
    {
        var patient = await _repo.GetByIdAsync(id);
        if (patient == null) return null;

        _mapper.Map(dto, patient);
        await _repo.SaveChangesAsync();

        return _mapper.Map<PatientDto>(patient);
    }

    public async Task<PatientDto?> DeletePatientAsync(int id)
    {
        var patient = await _repo.GetByIdAsync(id);
        if (patient == null) return null;

        await _repo.DeleteAsync(patient);
        await _repo.SaveChangesAsync();

        return _mapper.Map<PatientDto>(patient);
    }

    public async Task<ServiceResult> AssignMedicationToPatientAsync(int patientId, int medicationId)
    {
        if (await _repo.IsMedicationAssignedAsync(patientId, medicationId))
            return new ServiceResult { Success = false, Message = "This medication has already been assigned to the patient." };

        await _repo.AssignMedicationAsync(new PatientMedication
        {
            PatientID = patientId,
            MedicationID = medicationId
        });
        await _repo.SaveChangesAsync();

        return new ServiceResult { Success = true, Message = "Medication assigned successfully." };
    }

    public async Task GenerateDosesForNewAssignmentAsync(int patientId, int medicationId)
    {
        var patientMedication = await _repo.GetPatientMedicationAsync(patientId, medicationId);
        if (patientMedication == null) return;

        var med = patientMedication.Medication;
        var patient = patientMedication.Patient;

        var now = DateTime.Now;
        double interval = 24.0 / med.Frequency;

        for (int i = 0; i < med.Frequency; i++)
        {
            var scheduledTime = now.AddHours(i * interval);
            if (scheduledTime.Date != now.Date) break;

            patientMedication.Patient.DoseLogs.Add(new DoseLog
            {
                MedicationID = med.MedicationID,
                PatientID = patient.PatientID,
                Status = DoseStatus.Scheduled,
                ScheduledTime = scheduledTime,
                PatientName = patient.Name
            });
        }

        await _repo.SaveChangesAsync();
    }
}
