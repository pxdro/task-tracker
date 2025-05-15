using TaskTracker.Domain.DTOs;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Tests.Domain
{
    public class UserServiceTests
    {
        private readonly UserService _service = new();

        // Cleanup
        public UserServiceTests()
        {
            _service.Reset();
        }

        [Fact]
        public async Task Register_WithValidData_Returns201()
        {
            // Arrange
            var dto = new UserDto { Email = "user@example.com", Password = "Str0ngP@ss!" };

            // Act
            var result = await _service.RegisterAsync(dto);

            // Assert
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Email registered successfully", result.Message);
        }

        [Fact]
        public async Task Register_WithInvalidEmail_Returns400()
        {
            // Arrange
            var dto = new UserDto { Email = "invalid-email", Password = "Str0ngP@ss!" };

            // Act
            var result = await _service.RegisterAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid email format", result.Message);
        }

        [Fact]
        public async Task Register_WithEmptyEmailOrPassword_Returns400()
        {
            // Arrange
            var dto1 = new UserDto { Email = "", Password = "Str0ngP@ss!" };
            var dto2 = new UserDto { Email = "user@example.com", Password = "" };

            // Act
            var result1 = await _service.RegisterAsync(dto1);
            var result2 = await _service.RegisterAsync(dto2);

            // Assert
            Assert.Equal(400, result1.StatusCode);
            Assert.Equal("Missing required data", result1.Message);

            Assert.Equal(400, result2.StatusCode);
            Assert.Equal("Missing required data", result2.Message);
        }

        [Fact]
        public async Task Register_WithAlreadyRegisteredEmail_Returns409()
        {
            // Arrange
            var dto = new UserDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            await _service.RegisterAsync(dto);

            // Act
            var result = await _service.RegisterAsync(dto);

            // Assert
            Assert.Equal(409, result.StatusCode);
            Assert.Equal("Email already registered", result.Message);
        }
        [Fact]
        public async Task Login_WithValidCredentials_Returns200AndTokens()
        {
            // Arrange
            var dto = new UserDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            await _service.RegisterAsync(dto);

            // Act
            var result = await _service.LoginAsync(dto);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(result.AuthToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        }

        [Fact]
        public async Task Login_WithUnregisteredEmail_Returns404()
        {
            // Arrange
            var dto = new UserDto { Email = "unknown@example.com", Password = "AnyPass123" };

            // Act
            var result = await _service.LoginAsync(dto);

            // Assert
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Email unregistered", result.Message);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            // Arrange
            var dto = new UserDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            await _service.RegisterAsync(dto);

            var wrongLogin = new UserDto { Email = "user@example.com", Password = "WrongPass123" };

            // Act
            var result = await _service.LoginAsync(wrongLogin);

            // Assert
            Assert.Equal(401, result.StatusCode);
            Assert.Equal("Wrong password", result.Message);
        }
    }
}
