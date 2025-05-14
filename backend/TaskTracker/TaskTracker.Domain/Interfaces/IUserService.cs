using TaskTracker.Domain.DTOs;

namespace TaskTracker.Domain.Interfaces
{
    public interface IUserService
    {
        Task<UserResultDto> RegisterAsync(UserDto UserDto);
        Task<UserResultDto> LoginAsync(UserDto UserDto);
    }
}
