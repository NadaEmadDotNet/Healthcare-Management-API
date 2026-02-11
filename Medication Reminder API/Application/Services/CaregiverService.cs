using AutoMapper.QueryableExtensions;
using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Domain.Models;

namespace Medication_Reminder_API.Services
{
    public class CaregiverService : ICaregiverService
    {
        private readonly ICaregiverRepository _repo;
        private readonly IMedicationService _medService;
        private readonly IDoseLogService _logService;
        private readonly IMapper _mapper;

        public CaregiverService(
            ICaregiverRepository repo,
            IMedicationService medService,
            IDoseLogService logService,
            IMapper mapper)
        {
            _repo = repo;
            _medService = medService;
            _logService = logService;
            _mapper = mapper;
        }

        public async Task<List<Caregiver>> GetAllCaregiversAsync() =>
            await _repo.GetAll().ToListAsync();

        public async Task<Caregiver?> GetCaregiverByNameAsync(string name) =>
            (await _repo.GetAll().ToListAsync()).FirstOrDefault(c => c.Name == name);

        public async Task<string> AssignPatientToCaregiverAsync(CaregiverAssignDTO dto)
        {
            if (dto == null)
                throw new ArgumentException("Data is empty.");

            var caregiver = await _repo.GetByUserIdAsync(dto.UserId);

            if (caregiver == null)
            {
                caregiver = new Caregiver
                {
                    UserId = dto.UserId,
                    Name = dto.Name,
                    RelationToPatient = dto.RelationToPatient
                };
                await _repo.AddAsync(caregiver);
                await _repo.SaveChangesAsync();
            }

            var isAlreadyLinked = await _repo.IsPatientAssignedAsync(caregiver.CaregiverID, dto.PatientID);

            if (isAlreadyLinked)
                return "This caregiver is already assigned to this patient.";

            var patientCaregiver = new PatientCaregiver
            {
                CaregiverID = caregiver.CaregiverID,
                PatientID = dto.PatientID
            };

            await _repo.AssignPatientAsync(patientCaregiver);
            await _repo.SaveChangesAsync();

            return "Caregiver linked to patient successfully";
        }

        public async Task<Caregiver?> EditCaregiverAsync(int id, Caregiver caregiver)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            existing.RelationToPatient = caregiver.RelationToPatient;
            await _repo.SaveChangesAsync();
            return existing;
        }

        public async Task<Caregiver?> DeleteCaregiverAsync(int id)
        {
            var caregiver = await _repo.GetByIdAsync(id);
            if (caregiver == null) return null;

            await _repo.DeleteAsync(caregiver);
            await _repo.SaveChangesAsync();
            return caregiver;
        }

        public async Task<List<CaregiverPatientDTO>> GetPatientsWithMedicationsAsync(string caregiverUserId)
        {
            var caregiver = await _repo.GetByUserIdAsync(caregiverUserId);

            if (caregiver == null)
                return new List<CaregiverPatientDTO>();

            int caregiverId = caregiver.CaregiverID;

            var patientsEntities = await _repo.GetPatientsByCaregiverIdAsync(caregiverId);

            var result = new List<CaregiverPatientDTO>();

            foreach (var patient in patientsEntities)
            {
                var patientMeds = await _medService.GetAllMedicationsForPatientAsync(patient.PatientID);

                var medicationsDto = new List<CaregiverPatientMedicationDTO>();

                foreach (var med in patientMeds)
                {
                    var doseLogs = await _logService.GetDoseLogsAsync(patient.PatientID, med.MedicationID);

                    medicationsDto.Add(new CaregiverPatientMedicationDTO
                    {
                        MedicationID = med.MedicationID,
                        Name = med.Name,
                        Frequency = med.Frequency,
                        DurationInDays = med.DurationInDays,
                        Notes = med.Notes,
                        DoseLogs = doseLogs
                    });
                }

                result.Add(new CaregiverPatientDTO
                {
                    PatientID = patient.PatientID,
                    Name = patient.Name,
                    Age = patient.Age,
                    Gender = patient.Gender,
                    ChronicConditions = patient.ChronicConditions,
                    Medications = medicationsDto
                });
            }

            return result;
        }
    }
}
