using TaskTracker.Domain.DTOs;

namespace TaskTracker.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<ResultDto<UserDto>> RegisterAsync(RegisterDto registerDto);
        Task<ResultDto<TokensDto>> LoginAsync(RegisterDto registerDto);
        Task<ResultDto<TokensDto>> RefreshTokenAsync(TokensDto tokensDto);
    }
}
