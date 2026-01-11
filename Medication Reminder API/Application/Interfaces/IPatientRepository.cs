namespace Medication_Reminder_API.Application.Interfaces
{
    
        public interface IPatientRepository
        {
            IQueryable<Patient> GetAll();
            Task<Patient?> GetByIdAsync(int id);
            Task<List<Patient>> GetByIdsAsync(IEnumerable<int> ids);
            Task<List<Patient>> GetByNameAsync(string name);
            Task AddAsync(Patient patient);
            Task UpdateAsync(Patient patient);
            Task DeleteAsync(Patient patient);
            Task<bool> IsMedicationAssignedAsync(int patientId, int medicationId);
            Task AssignMedicationAsync(PatientMedication link);
            Task<PatientMedication?> GetPatientMedicationAsync(int patientId, int medicationId);
            Task SaveChangesAsync();

         }
}
