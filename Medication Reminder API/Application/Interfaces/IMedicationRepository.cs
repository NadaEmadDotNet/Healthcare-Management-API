namespace Medication_Reminder_API.Application.Interfaces
{
    public interface IMedicationRepository
    {
        IQueryable<Medication> GetAll();
        Task<Medication?> GetByIdAsync(int id);
        Task<List<Medication>> GetByNameAsync(string name);
        Task<List<Medication>> GetAllForPatientAsync(int patientId);
        Task AddAsync(Medication med);
        Task UpdateAsync(Medication med);
        Task DeleteAsync(Medication med);
        Task<int> CountDoseLogsAsync(int medicationId);
        Task<int> CountTakenDoseLogsAsync(int medicationId);
        Task<bool> ExistsByNameAsync(string name);
        Task SaveChangesAsync();
    }
}
