using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            if (result.Success)
            {
                Response.Headers["X-Confirmation-Sent"] = "true";
                return CreatedAtAction(nameof(Register), null);
            }

            return result.Error switch
            {
                "Email already registered" => Conflict(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            if (result.Success)
                return Ok(result.Data);

            return Unauthorized(new { error = result.Error });
        }
    }
}
