using Medication_Reminder_API.Application.DTOS;

namespace Medication_Reminder_API.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AuthResult> ConfirmEmail(string userId, string token);
        Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
