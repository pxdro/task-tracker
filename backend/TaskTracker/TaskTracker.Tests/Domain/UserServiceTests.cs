using System.Threading.Tasks;
using TaskTracker.Domain.DTOs;
using TaskTracker.Infrastructure.Services;
using TechTalk.SpecFlow.Assist;
using TechTalk.SpecFlow.CommonModels;
using Xunit;

namespace TaskTracker.Tests.Domain
{
    public class UserServiceTests
    {
        [Fact]
        public async Task RegisterAsync_NewEmail_ReturnsTrue()
        {
            // Arrange
            var service = new UserService();
            var registerDto = new RegisterDto { Email = "user@example.com", Password = "Str0ngP@ss!" };

            // Act
            var result = await service.RegisterAsync(registerDto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ReturnsFalse()
        {
            // Arrange
            var service = new UserService();
            var registerDto = new RegisterDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            var anotherDto = new RegisterDto { Email = "user@example.com", Password = "AnotherP@ss!" };
            await service.RegisterAsync(registerDto);

            // Act
            var result = await service.RegisterAsync(anotherDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task LoginAsync_WithCorrectCredentials_ReturnsTokens()
        {
            // Arrange
            var service = new UserService();
            var registerDto = new RegisterDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            await service.RegisterAsync(registerDto);

            // Act
            var result = await service.LoginAsync(registerDto);

            // Assert
            Assert.True(string.IsNullOrEmpty(result.Message));
            Assert.True(!string.IsNullOrWhiteSpace(result.AuthToken));
            Assert.True(!string.IsNullOrWhiteSpace(result.RefreshToken));
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsError()
        {
            // Arrange
            var service = new UserService();
            var registerDto = new RegisterDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            var wrongDto = new RegisterDto { Email = "user@example.com", Password = "AnyPass!" };
            await service.RegisterAsync(registerDto);

            // Act
            var result = await service.LoginAsync(wrongDto);

            // Assert
            Assert.Equal("Wrong password", result.Message);
        }

        [Fact]
        public async Task LoginAsync_WithUnregisteredEmail_ReturnsError()
        {
            // Arrange
            var service = new UserService();
            var anyDto = new RegisterDto { Email = "user@example.com", Password = "AnyPass123" };

            // Act
            var result = await service.LoginAsync(anyDto);

            // Assert
            Assert.Equal("Email unregistered", result.Message);
        }
    }
}
