using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Net;
using TaskTracker.Domain.DTOs;
using TaskTracker.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace TaskTracker.Tests.Integration
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<TaskTrackerDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<TaskTrackerDbContext>(options =>
                        options.UseInMemoryDatabase(_dbName));
                });
            });
        }

        [Fact]
        public async Task Register_ReturnsCreated_WhenValidUser()
        {
            var client = _factory.CreateClient();
            var dto = new UserDto { Email = "test@example.com", Password = "Password123!" };

            var response = await client.PostAsJsonAsync("/api/auth/register", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(response.Headers.Contains("X-Confirmation-Sent"));
        }

        [Fact]
        public async Task Register_ReturnsConflict_WhenEmailAlreadyRegistered()
        {
            var client = _factory.CreateClient();
            var dto = new UserDto { Email = "dup@example.com", Password = "Password123!" };

            // First registration
            var response1 = await client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

            // Duplicate registration
            var response2 = await client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);

            var error = await response2.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.Equal("Email already registered", error.Error);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithTokens_WhenCredentialsValid()
        {
            var client = _factory.CreateClient();
            var registerDto = new UserDto { Email = "login@example.com", Password = "Password123!" };
            await client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new UserDto { Email = "login@example.com", Password = "Password123!" };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.False(string.IsNullOrWhiteSpace(auth.AuthToken));
            Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenEmailNotRegistered()
        {
            var client = _factory.CreateClient();
            var loginDto = new UserDto { Email = "noexist@example.com", Password = "AnyPassword" };

            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.Equal("Email unregistered", error.Error);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordInvalid()
        {
            var client = _factory.CreateClient();
            var registerDto = new UserDto { Email = "user@example.com", Password = "CorrectPass1!" };
            await client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new UserDto { Email = "user@example.com", Password = "WrongPass" };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.Equal("Invalid credentials", error.Error);
        }

        private class ErrorResponse
        {
            public string Error { get; set; }
        }
    }
}
