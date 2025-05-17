using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (result.IsSuccess)
            {
                Response.Headers["X-Confirmation-Sent"] = "true";
                return CreatedAtAction(nameof(Register), null);
            }

            return result.ErrorMessage switch
            {
                "Email already registered" => Conflict(result.ErrorMessage),
                _ => StatusCode(500),
            };
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.LoginAsync(registerDto);
            if (result.IsSuccess)
            {
                Response.Headers["X-Auth-Token"] = result.Data?.AuthToken;
                Response.Headers["X-Refresh-Token"] = result.Data?.RefreshToken;
                return Ok(result.Data);
            }

            return result.ErrorMessage switch
            {
                "Email unregistered" => BadRequest(result.ErrorMessage),
                "Invalid credentials" => Unauthorized(result.ErrorMessage),
                _ => StatusCode(500),
            };
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokensDto tokensDto)
        {
            var result = await _authService.RefreshTokenAsync(tokensDto);

            if (result.IsSuccess)
            {
                Response.Headers["X-Auth-Token"] = result.Data?.AuthToken;
                Response.Headers["X-Refresh-Token"] = result.Data?.RefreshToken;
                return Ok(result.Data);
            }

            return result.ErrorMessage switch
            {
                "Invalid user ID in token" => NotFound(result.ErrorMessage),
                "Invalid credentials" => Unauthorized(result.ErrorMessage),
                _ => StatusCode(500),
            };
        }
    }
}
