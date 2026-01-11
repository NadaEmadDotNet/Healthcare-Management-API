using Medication_Reminder_API.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medication_Reminder_API.Application.Interfaces
{
    public interface ICaregiverRepository
    {
        IQueryable<Caregiver> GetAll();
        Task<Caregiver?> GetByIdAsync(int id);
        Task<Caregiver?> GetByUserIdAsync(string userId);
        Task AddAsync(Caregiver caregiver);
        Task UpdateAsync(Caregiver caregiver);
        Task DeleteAsync(Caregiver caregiver);
        Task<bool> IsPatientAssignedAsync(int caregiverId, int patientId);
        Task AssignPatientAsync(PatientCaregiver patientCaregiver);
        Task<List<Patient>> GetPatientsByCaregiverIdAsync(int caregiverId);
        Task SaveChangesAsync();
    }
}
