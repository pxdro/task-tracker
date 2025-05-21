using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRequestDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (result.IsSuccess)
                Response.Headers["X-Confirmation-Sent"] = "true";
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserRequestDto registerDto)
        {
            var result = await _authService.LoginAsync(registerDto);
            if (result.IsSuccess)
            {
                Response.Headers["X-Auth-Token"] = result.Data?.AuthToken;
                Response.Headers["X-Refresh-Token"] = result.Data?.RefreshToken;
            }
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokensDto tokensDto)
        {
            var result = await _authService.RefreshTokenAsync(tokensDto);
            if (result.IsSuccess)
            {
                Response.Headers["X-Auth-Token"] = result.Data?.AuthToken;
                Response.Headers["X-Refresh-Token"] = result.Data?.RefreshToken;
            }
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
