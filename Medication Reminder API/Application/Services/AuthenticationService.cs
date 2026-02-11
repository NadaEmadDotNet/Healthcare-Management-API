using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Web;

namespace Medication_Reminder_API.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailService emailService,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        public async Task<AuthResult> Register(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return new AuthResult { Success = false, Message = $"Email '{dto.Email}' is already used." };

            var user = new ApplicationUser
            {
                UserName = dto.FullName.Replace(" ", ""),
                Email = dto.Email,
                FullName = dto.FullName,
                IsActive = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);

            var confirmationLink =
                $"{_configuration["FrontendUrl"]}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $"Please confirm your account by clicking this link: {confirmationLink}");

            return new AuthResult
            {
                Success = true,
                Message = "Registration successful. Please check your email to confirm your account."
            };
        }

        public async Task<ServiceResult<object>> LoginAsync(LoginDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new ServiceResult<object> { Success = false, Message = "Invalid email or password" };

            if (!user.IsActive)
                return new ServiceResult<object> { Success = false, Message = "Account is deactivated" };

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
                return new ServiceResult<object> { Success = false, Message = "Invalid email or password" };

            var tokenResult = await _tokenService.GenerateTokenAsync(user);

            return new ServiceResult<object>
            {
                Success = true,
                Message = "Login successful",
                Data = tokenResult
            };
        }
    }
}
