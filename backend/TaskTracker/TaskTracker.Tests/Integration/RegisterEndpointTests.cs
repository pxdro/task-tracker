using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Tests.Integration
{
    public class RegisterEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory = factory;

        [Fact]
        public async Task Post_Register_NewUser_Returns201AndHeader()
        {
            // Arrange  
            var userService = _factory.Services.GetRequiredService<IUserService>() as UserService;
            userService?.Reset();
            var client = _factory.CreateClient();
            var UserDto = new UserDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            var content = new StringContent(
                JsonSerializer.Serialize(UserDto),
                Encoding.UTF8,
                "application/json");

            // Act  
            var response = await client.PostAsync("/api/auth/register", content);
            var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var message = responseBody?["message"];

            // Assert  
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("Email registered successfully", message);
            Assert.True(response.Headers.TryGetValues("X-Confirmation-Sent", out var headerValues));
            Assert.Equal("true", headerValues.FirstOrDefault());
        }

        [Fact]
        public async Task Post_Register_ExistingUser_Returns409WithErrorMessage()
        {
            // Arrange  
            var userService = _factory.Services.GetRequiredService<IUserService>() as UserService;
            userService?.Reset();
            var client = _factory.CreateClient();
            var UserDto = new UserDto { Email = "user@example.com", Password = "Str0ngP@ss!" };
            var content = new StringContent(
                JsonSerializer.Serialize(UserDto),
                Encoding.UTF8,
                "application/json");

            // Seed first registration  
            var first = await client.PostAsync("/api/auth/register", content);
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            // Act: try again with same email  
            var second = await client.PostAsync("/api/auth/register", content);
            var responseBody = await second.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var message = responseBody?["message"];

            // Assert  
            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
            Assert.Equal("Email already registered", message);
        }

        public void Dispose()
        {
            var userService = _factory.Services.GetRequiredService<IUserService>() as UserService;
            userService?.Reset();
        }
    }
}
