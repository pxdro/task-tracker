using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TaskTracker.Tests.Integration
{
    public class LoginEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task Login_WithValidCredentials_Returns200AndTokens()
        {
            // Arrange: register user
            var register = new { email = "user@example.com", password = "Str0ngP@ss!" };
            var regContent = new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/auth/register", regContent);

            // Act: login
            var loginContent = new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", loginContent);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("authToken", body);
            Assert.Contains("refreshToken", body);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401WithMessage()
        {
            // Arrange: register
            var register = new { email = "user@example.com", password = "Str0ngP@ss!" };
            var regContent = new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/auth/register", regContent);

            // Act: login with wrong password
            var login = new { email = "user@example.com", password = "WrongPass123" };
            var loginContent = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", loginContent);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains("\"message\":\"Wrong password\"", body);
        }

        [Fact]
        public async Task Login_WithUnregisteredEmail_Returns401WithMessage()
        {
            // Act
            var login = new { email = "unknown@example.com", password = "AnyPass123" };
            var content = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("\"message\":\"Email unregistered\"", body);
        }
    }
}
