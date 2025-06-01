using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRequestDto registerDto)
        {
            _logger.LogInformation($"POST {nameof(Register)} called");
            var result = await _authService.RegisterAsync(registerDto);
            if (result.IsSuccess)
                Response.Headers["X-Confirmation-Sent"] = "true";
            LogResult<UserReturnDto>("POST", "/api/auth/register", result);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserRequestDto registerDto)
        {
            _logger.LogInformation($"POST {nameof(Login)} called");
            var result = await _authService.LoginAsync(registerDto);
            if (result.IsSuccess)
            {
                Response.Headers["X-Auth-Token"] = result.Data?.AuthToken;
                Response.Headers["X-Refresh-Token"] = result.Data?.RefreshToken;
            }
            LogResult<TokensDto>("POST", "/api/auth/login", result);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokensDto tokensDto)
        {
            _logger.LogInformation($"POST {nameof(RefreshToken)} called");
            var result = await _authService.RefreshTokenAsync(tokensDto);
            if (result.IsSuccess)
            {
                Response.Headers["X-Auth-Token"] = result.Data?.AuthToken;
                Response.Headers["X-Refresh-Token"] = result.Data?.RefreshToken;
            }
            LogResult<TokensDto>("POST", "/api/auth/refresh", result);
            return StatusCode((int)result.StatusCode, result);
        }

        private void LogResult<T>(string method, string action, ResultDto<T> result)
        {
            if (result.StatusCode == HttpStatusCode.InternalServerError)
                _logger.LogError("{Method} {Action} internal error with message {ErrorMessage}", method, action, result.ErrorMessage);
            else if (result.IsSuccess)
                _logger.LogInformation("{Method} {Action} succeeded with code {StatusCode} and data {@Data}", method, action, (int)result.StatusCode, result.Data);
            else
                _logger.LogInformation("{Method} {Action} failed with code {StatusCode} and message {ErrorMessage}", method, action, (int)result.StatusCode, result.ErrorMessage);
        }
    }
}
