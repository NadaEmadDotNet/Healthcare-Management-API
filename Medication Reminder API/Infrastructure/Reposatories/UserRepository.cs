using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public UserRepository(
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            const string cacheKey = "all_users";

            if (!_cache.TryGetValue(cacheKey, out List<UserDto> cachedUsers))
            {
                var users = await _userManager.Users
                    .Where(u => u.IsVisible)
                    .OrderByDescending(u => u.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = roles.FirstOrDefault()
                    });
                }

                cachedUsers = userDtos;
                _cache.Set(cacheKey, cachedUsers, TimeSpan.FromMinutes(10));
            }

            return cachedUsers;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
        }
    }

