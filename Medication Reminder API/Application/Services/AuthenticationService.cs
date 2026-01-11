using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Web;

namespace Medication_Reminder_API.Application.Services
{
    public class AuthenticationService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IEmailService _emailService;

        public AuthenticationService(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuthenticationService> logger, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }
        public async Task<AuthResult> Register(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Email '{dto.Email}' is already used."
                };
            }

            var user = new ApplicationUser
            {
                UserName = dto.FullName.Replace(" ", ""),
                Email = dto.Email,
                FullName = dto.FullName,
                IsActive = false
            };



            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);

            var confirmationLink = $"{_configuration["FrontendUrl"]}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            await _emailService.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by clicking this link: {confirmationLink}");

            return new AuthResult
            {
                Success = true,
                Message = "Registration successful. Please check your email to confirm your account."
            };
        }
        public async Task<AuthResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // اعمل ديكود للتوكن قبل التأكيد
            var decodedToken = System.Web.HttpUtility.UrlDecode(token);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return new AuthResult
            {
                Success = true,
                Message = "Email confirmed successfully"
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

            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim("tokenVersion", user.TokenVersion.ToString())
    };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new ServiceResult<object>
            {
                Success = true,
                Message = "Login successful",
                Data = new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    RefreshToken = refreshToken
                }
            };
        }

        public async Task<ServiceResult<object>> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return new ServiceResult<object> { Success = false, Message = "Invalid or expired refresh token" };

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim("tokenVersion", user.TokenVersion.ToString())
    };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new ServiceResult<object>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                }
            };
        }


        public async Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            if (userId == null)
                return new ServiceResult { Success = false, Message = "Unauthorized request" };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ServiceResult { Success = false, Message = "User not found" };

            if (dto.NewPassword != dto.ConfirmNewPassword)
                return new ServiceResult { Success = false, Message = "New password and confirmation do not match" };

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!result.Succeeded)
                return new ServiceResult { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            user.TokenVersion++;
            await _userManager.UpdateAsync(user);

            return new ServiceResult { Success = true, Message = "Password changed successfully" };
        }

    }

}