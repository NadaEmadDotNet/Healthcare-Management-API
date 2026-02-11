using Medication_Reminder_API.Infrastructure;

namespace Medication_Reminder_API.Application.Interfaces
{
    public interface ITokenService
    {
        Task<object> GenerateTokenAsync(ApplicationUser user);
        Task<object> RefreshTokenAsync(string refreshToken);
    }
}
