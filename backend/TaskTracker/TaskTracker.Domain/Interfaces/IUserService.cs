using TaskTracker.Domain.DTOs;

namespace TaskTracker.Domain.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<LoginResultDto> LoginAsync(RegisterDto registerDto);
    }
}
