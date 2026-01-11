using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Medication_Reminder_API.Infrastructure.Reposatories
{
    public class CaregiverRepository : ICaregiverRepository
    {
        private readonly ApplicationDbContext _context;

        public CaregiverRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Caregiver> GetAll() => _context.Caregivers.AsNoTracking();

        public async Task<Caregiver?> GetByIdAsync(int id) =>
            await _context.Caregivers.FindAsync(id);

        public async Task<Caregiver?> GetByUserIdAsync(string userId) =>
            await _context.Caregivers.FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task AddAsync(Caregiver caregiver) =>
            await _context.Caregivers.AddAsync(caregiver);

        public async Task UpdateAsync(Caregiver caregiver) =>
            _context.Caregivers.Update(caregiver);

        public async Task DeleteAsync(Caregiver caregiver) =>
            _context.Caregivers.Remove(caregiver);

        public async Task<bool> IsPatientAssignedAsync(int caregiverId, int patientId) =>
            await _context.PatientCaregivers
                .AnyAsync(pc => pc.CaregiverID == caregiverId && pc.PatientID == patientId);

        public async Task AssignPatientAsync(PatientCaregiver link) =>
            await _context.PatientCaregivers.AddAsync(link);

        public async Task<List<Patient>> GetPatientsByCaregiverIdAsync(int caregiverId) =>
            await _context.PatientCaregivers
                .Where(pc => pc.CaregiverID == caregiverId)
                .Select(pc => pc.Patient)
                .ToListAsync();

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
