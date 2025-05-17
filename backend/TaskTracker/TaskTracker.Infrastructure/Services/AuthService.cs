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
    public class AuthService(
        TaskTrackerDbContext dbContext,
        IConfiguration configuration) : IAuthService
    {
        private readonly TaskTrackerDbContext _dbContext = dbContext;
        private readonly IConfiguration _configuration = configuration;

        public async Task<ResultDto<UserDto>> RegisterAsync(RegisterDto registerDto)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == registerDto.Email))
                return ResultDto<UserDto>.Fail("Email already registered");

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password)
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return ResultDto<UserDto>.Ok(new UserDto { Email = registerDto.Email });
        }

        public async Task<ResultDto<TokensDto>> LoginAsync(RegisterDto registerDto)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (user == null)
                return ResultDto<TokensDto>.Fail("Email unregistered");

            if (!VerifyPassword(registerDto.Password, user.PasswordHash))
                return ResultDto<TokensDto>.Fail("Invalid credentials");

            var tokensDto = GenerateTokens(user);
            var jwtSettings = _configuration.GetSection("JwtSettings");
            user.RefreshToken = tokensDto.RefreshToken!;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpirationDays"]!));
            await _dbContext.SaveChangesAsync(); 
            return ResultDto<TokensDto>.Ok(tokensDto);
        }

        public async Task<ResultDto<TokensDto>> RefreshTokenAsync(TokensDto tokensDto)
        {
            var principal = GetPrincipalFromExpiredToken(tokensDto.AuthToken!);
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var userGuid))
                return ResultDto<TokensDto>.Fail("Invalid user ID in token");

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null || user.RefreshToken != tokensDto.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
                return ResultDto<TokensDto>.Fail("Invalid refresh token");

            var newTokensDto = GenerateTokens(user);
            var jwtSettings = _configuration.GetSection("JwtSettings");
            user.RefreshToken = newTokensDto.RefreshToken!;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpirationDays"]!));
            await _dbContext.SaveChangesAsync();
            return ResultDto<TokensDto>.Ok(newTokensDto);
        }

        private TokensDto GenerateTokens(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["AuthKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                ]),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AuthTokenExpirationMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var authToken = tokenHandler.WriteToken(token);

            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshToken =  Convert.ToBase64String(randomNumber);

            return new TokensDto { AuthToken = authToken, RefreshToken = refreshToken };
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["RefreshTokenSecret"]!);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512Signature, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
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
