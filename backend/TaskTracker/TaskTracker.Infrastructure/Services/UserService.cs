using System.Collections.Concurrent;
using System.Linq;
using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly Dictionary<string, string> _users = []; // Emails and passwords

        public Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            if (_users.ContainsKey(registerDto.Email))
                return Task.FromResult<bool>(false);

            _users[registerDto.Email] = registerDto.Password;
            return Task.FromResult<bool>(true);
        }

        public Task<LoginResultDto> LoginAsync(RegisterDto registerDto)
        {
            if (!_users.ContainsKey(registerDto.Email))
                return Task.FromResult(new LoginResultDto { Message = "Email unregistered" });

            if (_users[registerDto.Email] != registerDto.Password)
                return Task.FromResult(new LoginResultDto { Message = "Wrong password" });

            return Task.FromResult(new LoginResultDto
            {
                AuthToken = GenerateFakeJwt(),
                RefreshToken = Guid.NewGuid().ToString()
            });
        }

        private string GenerateFakeJwt() => $"{Guid.NewGuid():N}.{Guid.NewGuid():N}.{Guid.NewGuid():N}";

        public void Reset() => _users.Clear();
    }
}
