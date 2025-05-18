using TaskTracker.Domain.DTOs;

namespace TaskTracker.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<ResultDto<UserReturnDto>> RegisterAsync(UserRequestDto registerDto);
        Task<ResultDto<TokensDto>> LoginAsync(UserRequestDto registerDto);
        Task<ResultDto<TokensDto>> RefreshTokenAsync(TokensDto tokensDto);
    }
}
