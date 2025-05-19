using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Context;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Tests.Domain
{
    public class AuthServiceTests : IDisposable
    {
        private readonly TaskTrackerDbContext _dbContext;
        private readonly AuthService _authService;
        private readonly Mock<IPasswordHasherService> _hasherMock = new();
        private readonly Mock<IConfiguration> _configMock = new();

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<TaskTrackerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new TaskTrackerDbContext(options);

            // Configs for JWT
            var jwtSettingsDict = new Dictionary<string, string>
            {
                ["JwtSettings:AuthKey"] = "fake_auth_key_with_at_least_512_bits_1234567890_1234567890_1234567890",
                ["JwtSettings:Issuer"] = "test_issuer",
                ["JwtSettings:Audience"] = "test_audience",
                ["JwtSettings:AuthTokenExpirationMinutes"] = "30",
                ["JwtSettings:RefreshTokenExpirationDays"] = "7"
            };
            _configMock.Setup(x => x.GetSection("JwtSettings")).Returns(() =>
            {
                var sectionMock = new Mock<IConfigurationSection>();

                // Config for specific values
                sectionMock.Setup(x => x["AuthKey"]).Returns(jwtSettingsDict["JwtSettings:AuthKey"]);
                sectionMock.Setup(x => x["Issuer"]).Returns(jwtSettingsDict["JwtSettings:Issuer"]);
                sectionMock.Setup(x => x["Audience"]).Returns(jwtSettingsDict["JwtSettings:Audience"]);
                sectionMock.Setup(x => x["AuthTokenExpirationMinutes"]).Returns(jwtSettingsDict["JwtSettings:AuthTokenExpirationMinutes"]);
                sectionMock.Setup(x => x["RefreshTokenExpirationDays"]).Returns(jwtSettingsDict["JwtSettings:RefreshTokenExpirationDays"]);

                return sectionMock.Object;
            });

            _authService = new AuthService(_hasherMock.Object, _dbContext, _configMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_NewUser_Success()
        {
            // Arrange
            _hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed_password");
            var email = "test@example.com";

            // Act
            var result = await _authService.RegisterAsync(new UserRequestDto
            {
                Email = email,
                Password = "123456"
            });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Equal(result.Data?.Email, email);
            Assert.Single(_dbContext.Users);
        }

        [Fact]
        public async Task RegisterAsync_EmailExists_ReturnsForbidden()
        {
            // Arrange
            await _dbContext.Users.AddAsync(new User { Email = "exists@test.com" });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _authService.RegisterAsync(new UserRequestDto
            {
                Email = "exists@test.com",
                Password = "any"
            });

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.Equal("Email already registered", result.ErrorMessage);
            Assert.Single(_dbContext.Users);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsTokens()
        {
            // Arrange
            var user = new User
            {
                Email = "valid@test.com",
                PasswordHash = "correct_hash",
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            _hasherMock.Setup(x => x.Verify("valid_pass", "correct_hash")).Returns(true);

            // Act
            var result = await _authService.LoginAsync(new UserRequestDto
            {
                Email = "valid@test.com",
                Password = "valid_pass"
            });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data?.AuthToken);
            Assert.NotNull(result.Data.RefreshToken);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _authService.LoginAsync(new UserRequestDto
            {
                Email = "notfound@test.com",
                Password = "any"
            });

            // Assert
            Assert.False(result.IsSuccess); 
            Assert.Equal("Email unregistered", result.ErrorMessage);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            await _dbContext.Users.AddAsync(new User
            {
                Email = "user@test.com",
                PasswordHash = "correct_hash"
            });
            await _dbContext.SaveChangesAsync();

            _hasherMock.Setup(x => x.Verify("wrong_pass", "correct_hash")).Returns(false);

            // Act
            var result = await _authService.LoginAsync(new UserRequestDto
            {
                Email = "user@test.com",
                Password = "wrong_pass"
            });

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid credentials", result.ErrorMessage);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task RefreshTokenAsync_ValidAuthAndRefreshToken_ReturnsNewTokens()
        {
            // Arrange
            var user = new User
            {
                Email = "valid@test.com",
                PasswordHash = "correct_hash",
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            _hasherMock.Setup(x => x.Verify("valid_pass", "correct_hash")).Returns(true);

            // Generates valid tokens
            var loginResult = await _authService.LoginAsync(new UserRequestDto
            {
                Email = "valid@test.com",
                Password = "valid_pass"
            });

            // Act
            var result = await _authService.RefreshTokenAsync(new TokensDto
            {
                AuthToken = loginResult.Data?.AuthToken,
                RefreshToken = loginResult.Data?.RefreshToken
            });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data?.AuthToken);
            Assert.NotNull(result.Data.RefreshToken);
        }

        [Fact]
        public async Task RefreshTokenAsync_ValidAuthButInvalidRefreshToken_ReturnsUnauthorized()
        {
            // Arrange
            var user = new User
            {
                Email = "valid@test.com",
                PasswordHash = "correct_hash",
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            _hasherMock.Setup(x => x.Verify("valid_pass", "correct_hash")).Returns(true);

            var loginResult = await _authService.LoginAsync(new UserRequestDto
            {
                Email = "valid@test.com",
                Password = "valid_pass"
            });

            // Act
            var result = await _authService.RefreshTokenAsync(new TokensDto
            {
                AuthToken = loginResult.Data?.AuthToken,
                RefreshToken = "invalid_refresh_token"
            });

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid refresh token", result.ErrorMessage);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task RefreshTokenAsync_InvalidAuthAndRefreshToken_ReturnsUnauthorized()
        {
            // Act
            var result = await _authService.RefreshTokenAsync(new TokensDto
            {
                AuthToken = "invalid_auth_token",
                RefreshToken = "invalid_refresh_token"
            });

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid auth token", result.ErrorMessage);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        public void Dispose() => _dbContext.Dispose();
    }
}
