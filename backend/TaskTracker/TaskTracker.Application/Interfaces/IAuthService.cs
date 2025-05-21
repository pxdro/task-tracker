using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ResultDto<UserReturnDto>> RegisterAsync(UserRequestDto registerDto);
        Task<ResultDto<TokensDto>> LoginAsync(UserRequestDto registerDto);
        Task<ResultDto<TokensDto>> RefreshTokenAsync(TokensDto tokensDto);
    }
}
