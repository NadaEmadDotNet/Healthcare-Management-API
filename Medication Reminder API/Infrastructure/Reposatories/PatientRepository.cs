using Medication_Reminder_API.Application.Interfaces;

namespace Medication_Reminder_API.Infrastructure.Reposatories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Patient> GetAll() => _context.Patients.AsNoTracking();

        public async Task<Patient?> GetByIdAsync(int id) => await _context.Patients.FindAsync(id);

        public async Task<List<Patient>> GetByIdsAsync(IEnumerable<int> ids) =>
            await _context.Patients.Where(p => ids.Contains(p.PatientID)).AsNoTracking().ToListAsync();

        public async Task<List<Patient>> GetByNameAsync(string name) =>
            await _context.Patients
                .Where(p => EF.Functions.Like(p.Name, $"%{name}%"))
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();

        public async Task AddAsync(Patient patient) => await _context.Patients.AddAsync(patient);

        public async Task UpdateAsync(Patient patient) => _context.Patients.Update(patient);

        public async Task DeleteAsync(Patient patient) => _context.Patients.Remove(patient);

        public async Task<bool> IsMedicationAssignedAsync(int patientId, int medicationId) =>
            await _context.PatientMedications.AnyAsync(pm => pm.PatientID == patientId && pm.MedicationID == medicationId);

        public async Task AssignMedicationAsync(PatientMedication link) => await _context.PatientMedications.AddAsync(link);

        public async Task<PatientMedication?> GetPatientMedicationAsync(int patientId, int medicationId) =>
            await _context.PatientMedications
                .Include(pm => pm.Medication)
                .Include(pm => pm.Patient)
                .FirstOrDefaultAsync(pm => pm.PatientID == patientId && pm.MedicationID == medicationId);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
