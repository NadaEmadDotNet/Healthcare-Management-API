using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Identity;

public interface IUserService
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<ServiceResult> ChangeUserStatusAsync(string userId, UpdateUserStatusDto dto);
    }

}
