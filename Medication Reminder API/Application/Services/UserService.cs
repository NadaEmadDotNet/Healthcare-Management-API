using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Domain.Models;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Medication_Reminder_API.Services.Interfaces
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IMemoryCache cache)
        {
            _userManager = userManager;
            _context = context;
            _cache = cache;
        }

        public async Task<ApplicationUser> Createuser(CreateUserDTO dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, dto.Role);

            switch (dto.Role.ToLower())
            {
                case "patient":
                    _context.Patients.Add(new Patient
                    {
                        UserId = user.Id,
                        Name = dto.FullName,
                        Age = dto.Age,
                        Gender = dto.Gender ?? "NotSpecified",
                        ChronicConditions = dto.ChronicConditions ?? ""
                    });
                    break;

                case "doctor":
                    _context.Doctors.Add(new Doctor
                    {
                        UserId = user.Id,
                        Name = dto.FullName,
                        Specialty = dto.Specialty ?? "",
                    });
                    break;

                case "caregiver":
                    _context.Caregivers.Add(new Caregiver
                    {
                        UserId = user.Id,
                        Name = dto.FullName,
                        RelationToPatient = dto.RelationToPatient ?? "Unknown"
                    });
                    break;

                default:
                    throw new Exception("Invalid role specified.");
            }


            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {

            const string cacheKey = "all_users";
            if (!_cache.TryGetValue(cacheKey, out List<UserDto> cachedUsers))
            {
                var users = await _userManager.Users.OrderByDescending(u => u.CreatedAt)
            .AsNoTracking()
            .Where(u => u.IsVisible) 
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

                var userDtos = new List<UserDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = roles.FirstOrDefault()
                    });
                }
                _cache.Set(cacheKey, cachedUsers = userDtos, TimeSpan.FromMinutes(10));
            }

            return cachedUsers;
        }
        public async Task<ServiceResult> ChangeUserStatusAsync(string userId, UpdateUserStatusDto dto, UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return new ServiceResult { Success = false, Message = "User not found" };

            user.IsActive = dto.IsActive;
            user.IsVisible = dto.IsVisible;
            user.TokenVersion++; // لمنع أي توكن قديم
            await userManager.UpdateAsync(user);

            return new ServiceResult { Success = true, Message = "User status updated successfully" };
        }
    }
}
