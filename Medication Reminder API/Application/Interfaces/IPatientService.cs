using Medication_Reminder_API.Application.DTOS;

namespace Medication_Reminder_API.Application.Interfaces
{
    public interface IPatientService
    {
           Task<PagedResult<PatientDto>> GetAllPatientsAsync(
            string? doctorId, string? caregiverId, string? patientId,
            int page, int pageSize);
            Task<List<PatientDto>> GetByNameAsync(string name);
            Task<List<PatientDto>> GetByIdsAsync(IEnumerable<int> ids);
            Task<PatientDto> AddAsync(PatientDto dto);
            Task<PatientDto?> EditPatientAsync(int id, PatientDto dto);
            Task<PatientDto?> DeletePatientAsync(int id);
           Task<ServiceResult> AssignMedicationToPatientAsync(int patientId, int medicationId);
            Task GenerateDosesForNewAssignmentAsync(int patientId, int medicationId);
    }

}
