using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using TaskTracker.Domain.DTOs;

namespace TaskTracker.Tests.Integration
{
    public class AuthLoginEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task Login_Should_Succeed_With_Valid_Credentials()
        {
            // Arrange
            var dto = new UserDto { Email = "user2@example.com", Password = "Str0ngP@ss!" };
            await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(content?["authToken"]);
            Assert.NotNull(content?["refreshToken"]);
        }

        [Fact]
        public async Task Login_Should_Fail_With_Unregistered_Email()
        {
            // Arrange
            var dto = new UserDto
            {
                Email = "notfound@example.com",
                Password = "AnyPass123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.Equal("Email unregistered", content?["message"]);
        }

        [Fact]
        public async Task Login_Should_Fail_With_Wrong_Password()
        {
            // Arrange
            var register = new UserDto { Email = "user3@example.com", Password = "Str0ngP@ss!" };
            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new UserDto { Email = "user3@example.com", Password = "WrongPass123" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", login);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.Equal("Wrong password", content?["message"]);
        }
    }
}
