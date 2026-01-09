using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Medication_Reminder_API.Application.Services
{
    public class AuthenticationService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuthenticationService> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> LoginAsync(LoginDTO dto)
        {

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed for email: {Email}", dto.Email);
                return new BadRequestObjectResult("Invalid email or password");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: inactive account for email {Email}", dto.Email);
                return new UnauthorizedObjectResult("Account is deactivated");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Invalid password for email {Email}", dto.Email);
                return new BadRequestObjectResult("Invalid email or password");
            }

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

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return new OkObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                RefreshToken = refreshToken
            });
        }

        public async Task<IActionResult> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token attempt");
                return new UnauthorizedObjectResult("Invalid or expired refresh token");
            }

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

            return new OkObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        public async Task<IActionResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            if (userId == null)
            {
                _logger.LogWarning("Change password failed: unauthorized request");
                return new UnauthorizedResult();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Change password failed: user {UserId} not found", userId);
                return new NotFoundObjectResult("User not found");
            }

            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                _logger.LogWarning("Change password failed for user {UserId}: password mismatch", user.Id);
                return new BadRequestObjectResult("New password and confirmation do not match");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                _logger.LogWarning(
                    "Change password failed for user {UserId}. Errors: {Errors}",
                    user.Id,
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );

                return new BadRequestObjectResult(result.Errors);
            }

            user.TokenVersion++;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Password changed successfully for user {UserId}", user.Id);
            return new OkObjectResult("Password changed successfully");
        }
    }

 }