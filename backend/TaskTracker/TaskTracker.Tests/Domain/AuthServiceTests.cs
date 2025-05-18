using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskTracker.Domain.DTOs;
using TaskTracker.Infrastructure.Context;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Tests.Domain
{
    public class AuthServiceTests
    {
        private readonly AuthService _service;
        private readonly TaskTrackerDbContext _dbContext;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<TaskTrackerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TaskTrackerDbContext(options);

            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "test-secret-key-1234567890123456" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiresInMinutes", "60" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _service = new AuthService(_dbContext, configuration);
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccess()
        {
            var dto = new UserReturnDto { Email = "user@example.com", Password = "Str0ngP@ss!" };

            var result = await _service.RegisterAsync(dto);

            Assert.True(result.Success);
        }

        [Theory]
        [InlineData("", "Str0ngP@ss!", "Missing required data")]
        [InlineData("user@example.com", "", "Missing required data")]
        public async Task Register_WithMissingData_ReturnsFail(string email, string password, string expectedError)
        {
            var dto = new UserReturnDto { Email = email, Password = password };

            var result = await _service.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsFail()
        {
            var dto = new UserReturnDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            await _service.RegisterAsync(dto);

            var result = await _service.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Email already registered", result.Error);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsAuthData()
        {
            var dto = new UserReturnDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            await _service.RegisterAsync(dto);

            var loginResult = await _service.LoginAsync(dto);

            Assert.True(loginResult.Success);
            Assert.NotNull(loginResult.Data);
            Assert.False(string.IsNullOrWhiteSpace(loginResult.Data!.AuthToken));
            Assert.False(string.IsNullOrWhiteSpace(loginResult.Data.RefreshToken));
        }

        [Fact]
        public async Task Login_WithUnregisteredEmail_ReturnsFail()
        {
            var dto = new UserReturnDto { Email = "unknown@example.com", Password = "AnyPass123" };

            var result = await _service.LoginAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Email unregistered", result.Error);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsFail()
        {
            var dto = new UserReturnDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            await _service.RegisterAsync(dto);
            var wrongDto = new UserReturnDto { Email = dto.Email, Password = "WrongPass123" };

            var result = await _service.LoginAsync(wrongDto);

            Assert.False(result.Success);
            Assert.Equal("Invalid credentials", result.Error);
        }
    }
}
