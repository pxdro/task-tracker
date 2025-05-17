using TaskTracker.Domain.DTOs;

namespace TaskTracker.Domain.Interfaces
{
    public interface IUserService
    {
        Task<ResultDto<UnitDto>> RegisterAsync(UserDto dto);
        Task<ResultDto<AuthResponseDto>> LoginAsync(UserDto dto);
    }
}
