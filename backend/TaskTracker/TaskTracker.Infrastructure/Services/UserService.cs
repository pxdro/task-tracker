using System.Text.RegularExpressions;
using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly Dictionary<string, string> _users = []; // Emails and passwords


        public Task<UserResultDto> RegisterAsync(UserDto UserDto)
        {
            if (string.IsNullOrWhiteSpace(UserDto.Email) || string.IsNullOrWhiteSpace(UserDto.Password))
                return Task.FromResult(new UserResultDto { StatusCode = 400,  Message = "Missing required data" });

            if (!IsValidEmail(UserDto.Email))
                return Task.FromResult(new UserResultDto { StatusCode = 400, Message = "Invalid email format" });

            if (_users.ContainsKey(UserDto.Email))
                return Task.FromResult(new UserResultDto { StatusCode = 409, Message = "Email already registered" });

            _users[UserDto.Email] = UserDto.Password;

            // Simular envio de e-mail
            SendConfirmationEmail(UserDto.Email);

            return Task.FromResult(new UserResultDto { StatusCode = 201, Message = "Email registered successfully" });
        }

        public Task<UserResultDto> LoginAsync(UserDto UserDto)
        {
            if (!_users.TryGetValue(UserDto.Email, out string? value))
                return Task.FromResult(new UserResultDto { StatusCode = 404, Message = "Email unregistered" });

            if (value != UserDto.Password)
                return Task.FromResult(new UserResultDto { StatusCode = 401, Message = "Wrong password" });

            return Task.FromResult(new UserResultDto
            {
                StatusCode = 200,
                AuthToken = GenerateFakeJwt(),
                RefreshToken = Guid.NewGuid().ToString()
            });
        }

        private static bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }

        private static void SendConfirmationEmail(string email)
        {
            // Simulação de envio
            Console.WriteLine($"Email enviado para: {email}");
        }

        private static string GenerateFakeJwt() => $"{Guid.NewGuid():N}.{Guid.NewGuid():N}.{Guid.NewGuid():N}";

        public void Reset() => _users.Clear();
    }
}
