using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<ServiceResult> ChangeUserStatusAsync(string userId, UpdateUserStatusDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return new ServiceResult { Success = false, Message = "User not found" };

        user.IsActive = dto.IsActive;
        user.IsVisible = dto.IsVisible;
        user.TokenVersion++;

        await _userRepository.UpdateAsync(user);

        return new ServiceResult { Success = true, Message = "User status updated successfully" };
    }
}
