using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Infrastructure;

namespace Medication_Reminder_API.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<ApplicationUser?> GetByIdAsync(string userId);
        Task UpdateAsync(ApplicationUser user);
    }
}
