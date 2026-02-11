using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Domain.Enums;

public interface IDoseLogService
{
    Task<List<DoseLogDTO>> GetDoseLogsAsync(int patientId, int medicationId); 
    PagedResult<DoseLogDTO> GetDosesForPatient(int patientId, DoseStatus? status = null, int page = 1, int pageSize = 10);
    List<DoseLogDTO> GetDosesByMedicationName(int patientId, string name);
    DoseLogDTO? GetDoseById(int id);
    DoseLogDTO AddDose(AddDoseDTO dto);
    DoseLogDTO? UpdateTakenTime(int doseLogId, DateTime takenTime, IMedicationService medService);
    int GetTakenDoseCount(int medicationId);
}
