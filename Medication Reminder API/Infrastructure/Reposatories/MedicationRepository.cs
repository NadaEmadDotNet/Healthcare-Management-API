using Medication_Reminder_API.Application.Interfaces;

namespace Medication_Reminder_API.Infrastructure.Repositories
{
    public class MedicationRepository : IMedicationRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Medication> GetAll() => _context.Medications.AsNoTracking();

        public async Task<Medication?> GetByIdAsync(int id) => await _context.Medications.FindAsync(id);

        public async Task<List<Medication>> GetByNameAsync(string name) =>
            await _context.Medications
                .Where(m => m.Name.Contains(name))
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<Medication>> GetAllForPatientAsync(int patientId) =>
            await _context.PatientMedications
                .Where(pm => pm.PatientID == patientId)
                .Select(pm => pm.Medication)
                .AsNoTracking()
                .ToListAsync();

        public async Task AddAsync(Medication med) => await _context.Medications.AddAsync(med);

        public async Task UpdateAsync(Medication med) => _context.Medications.Update(med);

        public async Task DeleteAsync(Medication med) => _context.Medications.Remove(med);

        public async Task<int> CountDoseLogsAsync(int medicationId) =>
            await _context.DoseLogs.CountAsync(d => d.MedicationID == medicationId);

        public async Task<int> CountTakenDoseLogsAsync(int medicationId) =>
            await _context.DoseLogs.CountAsync(d => d.MedicationID == medicationId && d.Status == DoseStatus.Taken);

        public async Task<bool> ExistsByNameAsync(string name) =>
            await _context.Medications.AnyAsync(m => m.Name.ToLower() == name.ToLower());

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
