using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Web;

namespace Medication_Reminder_API.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<AuthResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthResult { Success = false, Message = "User not found" };

            var decodedToken = HttpUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return new AuthResult { Success = true, Message = "Email confirmed successfully" };
        }

        public async Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            if (userId == null)
                return new ServiceResult { Success = false, Message = "Unauthorized request" };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ServiceResult { Success = false, Message = "User not found" };

            if (dto.NewPassword != dto.ConfirmNewPassword)
                return new ServiceResult
                {
                    Success = false,
                    Message = "New password and confirmation do not match"
                };

            var result = await _userManager.ChangePasswordAsync(
                user, dto.OldPassword, dto.NewPassword);

            if (!result.Succeeded)
                return new ServiceResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return new ServiceResult
            {
                Success = true,
                Message = "Password changed successfully"
            };
        }
    }
}
