using Medication_Reminder_API.DTOS;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medication_Reminder_API.Services.Interfaces
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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
            var users = _userManager.Users.ToList();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "No Role",
                    Password = user.PasswordHash
                });

            }
            return userDtos;
        }
    }
}
