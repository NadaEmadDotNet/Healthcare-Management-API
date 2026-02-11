using Medication_Reminder_API.Application.DTOS;

namespace Medication_Reminder_API.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResult> Register(RegisterDto dto);
        Task<ServiceResult<object>> LoginAsync(LoginDTO dto);
    }
}