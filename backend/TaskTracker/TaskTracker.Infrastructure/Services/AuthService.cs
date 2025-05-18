using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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

        public async Task<ResultDto<UserReturnDto>> RegisterAsync(UserRequestDto registerDto)
        {
            try
            {
                if (await _dbContext.Users.AnyAsync(u => u.Email == registerDto.Email))
                    return ResultDto<UserReturnDto>.Failure("Email already registered", HttpStatusCode.Forbidden);

                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = HashPassword(registerDto.Password)
                };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                return ResultDto<UserReturnDto>.Success(new UserReturnDto { Email = registerDto.Email }, HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Server error");
                return ResultDto<UserReturnDto>.Failure(
                    "Server error",
                    HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ResultDto<TokensDto>> LoginAsync(UserRequestDto registerDto)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
                if (user == null)
                    return ResultDto<TokensDto>.Failure("Email unregistered", HttpStatusCode.NotFound);

                if (!VerifyPassword(registerDto.Password, user.PasswordHash))
                    return ResultDto<TokensDto>.Failure("Invalid credentials", HttpStatusCode.Unauthorized);

                var tokensDto = GenerateTokens(user);
                var jwtSettings = _configuration.GetSection("JwtSettings");
                user.RefreshToken = tokensDto.RefreshToken!;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpirationDays"]!));
                await _dbContext.SaveChangesAsync();
                return ResultDto<TokensDto>.Success(tokensDto, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Server error");
                return ResultDto<TokensDto>.Failure(
                    "Server error",
                    HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ResultDto<TokensDto>> RefreshTokenAsync(TokensDto tokensDto)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(tokensDto.AuthToken!);
                var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userId, out var userGuid))
                    return ResultDto<TokensDto>.Failure("Invalid user ID in token", HttpStatusCode.BadRequest);

                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == userGuid);

                if (user == null || user.RefreshToken != tokensDto.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
                    return ResultDto<TokensDto>.Failure("Invalid refresh token", HttpStatusCode.Unauthorized);

                var newTokensDto = GenerateTokens(user);
                var jwtSettings = _configuration.GetSection("JwtSettings");
                user.RefreshToken = newTokensDto.RefreshToken!;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpirationDays"]!));
                await _dbContext.SaveChangesAsync();
                return ResultDto<TokensDto>.Success(newTokensDto, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Server error");
                return ResultDto<TokensDto>.Failure(
                    "Server error",
                    HttpStatusCode.InternalServerError
                );
            }
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

        // Argon2id For Password Hashing
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
