using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;
using TaskTracker.Application.Interfaces;
using TaskTracker.Infrastructure.Context;

namespace TaskTracker.Infrastructure.Services
{
    public class AuthService(
        IPasswordHasherService passwordHasher,
        TaskTrackerDbContext dbContext,
        IConfiguration configuration) : IAuthService
    {
        private readonly IPasswordHasherService _passwordHasher = passwordHasher;
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
                    PasswordHash = _passwordHasher.Hash(registerDto.Password)
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

                if (!_passwordHasher.Verify(registerDto.Password, user.PasswordHash))
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
                var principal = new ClaimsPrincipal();
                var userId = Guid.Empty;
                try
                {
                    (principal, userId) = GetPrincipalFromExpiredToken(tokensDto.AuthToken!);
                }
                catch
                {
                    return ResultDto<TokensDto>.Failure("Invalid auth token", HttpStatusCode.Unauthorized);
                }

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

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
                    SecurityAlgorithms.HmacSha512)
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

        public (ClaimsPrincipal principal, Guid userId) GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["AuthKey"]!);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // If not same security method OR not same algorithm OR not valid guid OR principal or userId is null -> Invalid auth token
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase) ||
                !Guid.TryParse(userId, out var userGuid) ||
                principal == null || userId == null)
                throw new Exception();
            
            return (principal, userGuid);
        }
    }
}
