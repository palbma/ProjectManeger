using ProjectManager.Dto.Auth;

namespace ProjectManager.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponseDto> Register(UserRegisterDto registerDto);
        Task<UserResponseDto> Login(UserLoginDto loginDto);
        Task<UserResponseDto> GetCurrentUser(int userId);
    }
}
