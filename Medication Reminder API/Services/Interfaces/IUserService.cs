public interface IUserService
{
    public  Task<ApplicationUser> Createuser(CreateUserDTO dto);
    public Task<List<UserDto>> GetAllUsersAsync();
}
