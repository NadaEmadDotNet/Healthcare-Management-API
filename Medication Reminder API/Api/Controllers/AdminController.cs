using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Medication_Reminder_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;


        public AdminController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;

        }

        [HttpPost("AssignRole/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            return Ok($"User {user.UserName} assigned to role {role}");
        }
        //[Authorize(Roles = "Admin")]
        //[HttpPost("CreateUser")]
        //public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        //{
        //    if (dto == null)
        //        return BadRequest("User data is required.");

        //    try
        //    {


        //        var user = await _userService.Createuser(dto);

        //        return Ok(new
        //        {
        //            user.Id,
        //            user.UserName,
        //            user.Email,
        //            dto.Role,
        //            dto.FullName
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [Authorize(Roles = "Admin")]
        [HttpGet("AllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            if (!users.Any())
                return NotFound("No users found.");

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ChangeUserStatus")]
        public async Task<IActionResult> ChangeUserStatus(string userId, [FromBody] UpdateUserStatusDto dto)
        {
            var result = await _userService.ChangeUserStatusAsync(userId, dto, _userManager);
            if (!result.Success) return NotFound(result.Message);
            return Ok(result.Message);
        }
    }
}
