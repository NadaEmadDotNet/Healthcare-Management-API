using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Services;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Medication_Reminder_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        public AuthenticationController(AuthenticationService authenticationService)
        {
            _authService= authenticationService;

        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        { 
        if (ModelState.IsValid)
            {
               return await _authService.LoginAsync(dto);
            }
            return BadRequest("Invalid login data.");
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
            => await _authService.RefreshTokenAsync(refreshToken);

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await _authService.ChangePasswordAsync(userId, dto);
        }
    }
} 

