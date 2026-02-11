namespace Medication_Reminder_API.Application.Interfaces
{
    public interface IDoseLogRepository
    {
        IQueryable<DoseLog> GetAll();
        Task<DoseLog?> GetByIdAsync(int id);
        Task<List<DoseLogDTO>> GetDoseLogsAsync(int patientId, int medicationId);
        Task<List<DoseLog>> GetByPatientIdAsync(int patientId);
        Task AddAsync(DoseLog dose);
        Task SaveChangesAsync();
    }
}
