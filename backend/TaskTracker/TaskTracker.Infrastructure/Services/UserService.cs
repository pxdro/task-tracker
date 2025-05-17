using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Context;

namespace TaskTracker.Infrastructure.Services
{
    public class UserService(
        TaskTrackerDbContext dbContext,
        IConfiguration configuration) : IUserService
    {
        private readonly TaskTrackerDbContext _dbContext = dbContext;
        private readonly IConfiguration _configuration = configuration;

        public async Task<ResultDto<UnitDto>> RegisterAsync(UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return ResultDto<UnitDto>.Fail("Missing required data");

            if (await _dbContext.Users.AnyAsync(u => u.Email == dto.Email))
                return ResultDto<UnitDto>.Fail("Email already registered");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password)
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return ResultDto<UnitDto>.Ok(UnitDto.Value);
        }

        public async Task<ResultDto<AuthResponseDto>> LoginAsync(UserDto dto)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return ResultDto<AuthResponseDto>.Fail("Email unregistered");

            if (!VerifyPassword(dto.Password, user.PasswordHash))
                return ResultDto<AuthResponseDto>.Fail("Invalid credentials");

            var token = GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid().ToString();

            var authData = new AuthResponseDto
            {
                AuthToken = token,
                RefreshToken = refreshToken
            };
            return ResultDto<AuthResponseDto>.Ok(authData);
        }

        private string GenerateJwtToken(User user)
        {
            var settings = _configuration.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(settings["Key"]!);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(settings["ExpiresInMinutes"]!));
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                issuer: settings["Issuer"],
                audience: settings["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                MemorySize = 64 * 1024,
                Iterations = 4
            };
            var hash = argon2.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.', 2);
            if (parts.Length != 2) return false;
            var salt = Convert.FromBase64String(parts[0]);
            var expected = parts[1];
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                MemorySize = 64 * 1024,
                Iterations = 4
            };
            var computed = argon2.GetBytes(32);
            return Convert.ToBase64String(computed) == expected;
        }
    }
}
