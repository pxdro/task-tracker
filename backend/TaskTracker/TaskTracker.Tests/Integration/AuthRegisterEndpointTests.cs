using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskTracker.Domain.DTOs;

namespace TaskTracker.Tests.Integration
{
    public class AuthRegisterEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task Register_Should_Succeed_With_Valid_Data()
        {
            // Arrange
            var dto = new UserDto
            {
                Email = "user1@example.com",
                Password = "Str0ngP@ss!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(response.Headers.Contains("X-Confirmation-Sent"));

            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.Equal("Email registered successfully", content?["message"]);
        }

        [Fact]
        public async Task Register_Should_Fail_With_Invalid_Email()
        {
            // Arrange
            var dto = new UserDto
            {
                Email = "invalid-email",
                Password = "Str0ngP@ss!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.Equal("Invalid email format", content?["message"]);
        }
    }
}
