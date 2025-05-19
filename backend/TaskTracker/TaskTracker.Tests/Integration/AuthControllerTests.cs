using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Net;
using TaskTracker.Domain.DTOs;
using TaskTracker.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using TechTalk.SpecFlow.CommonModels;

namespace TaskTracker.Tests.Integration
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory
                .WithWebHostBuilder(builder =>
                {
                    // 1) Ensure Testing Env -> Don't create Context
                    builder.UseEnvironment("Testing");

                    // 2) Config JWT Settings
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        var dict = new Dictionary<string, string>
                        {
                            ["JwtSettings:AuthKey"] = "fake_auth_key_with_at_least_512_bits_1234567890_1234567890_1234567890",
                            ["JwtSettings:Issuer"] = "TestIssuer",
                            ["JwtSettings:Audience"] = "TestAudience",
                            ["JwtSettings:AuthTokenExpirationMinutes"] = "60",
                            ["JwtSettings:RefreshTokenExpirationDays"] = "1"
                        };
                        config.AddInMemoryCollection(dict!);
                    });

                    // 3) Register DB InMemory
                    builder.ConfigureTestServices(services =>
                    {
                        // Remove previous DbContext
                        var descriptors = services
                            .Where(d =>
                                d.ServiceType == typeof(TaskTrackerDbContext) ||
                                (d.ServiceType.IsGenericType
                                 && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)
                                 && d.ServiceType.GenericTypeArguments[0] == typeof(TaskTrackerDbContext))
                            )
                            .ToList();
                        foreach (var d in descriptors) services.Remove(d);

                        // Fixed DB InMemory to ensure data permanency
                        services.AddDbContext<TaskTrackerDbContext>(opts =>
                            opts.UseInMemoryDatabase("TestDb_Shared"));
                    });
                })
                .CreateClient();
        }

        [Fact]
        public async Task Register_NewUser_ReturnsCreatedAndHeaderAndData()
        {
            // Arrange
            var dto = new UserRequestDto { Email = "new@test.com", Password = "Password123!" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(response.Headers.Contains("X-Confirmation-Sent"));

            var result = await response.Content.ReadFromJsonAsync<ResultDto<UserReturnDto>>();
            Assert.NotNull(result);
            Assert.Null(result?.ErrorMessage);
            Assert.Equal(dto.Email, result?.Data?.Email);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsForbiddenAndError()
        {
            // Arrange
            var dto = new UserRequestDto { Email = "dup@test.com", Password = "Password123!" };
            await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<UserReturnDto>>();
            Assert.NotNull(result);
            Assert.Equal("Email already registered", result?.ErrorMessage);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task Login_ExistingUser_ReturnsOkAndTokensInBodyAndHeader()
        {
            // Arrange
            var dto = new UserRequestDto { Email = "login@test.com", Password = "Password123!" };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

            // Act
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", dto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            Assert.True(loginResponse.Headers.Contains("X-Auth-Token"));
            Assert.True(loginResponse.Headers.Contains("X-Refresh-Token"));


            var result = await loginResponse.Content.ReadFromJsonAsync<ResultDto<TokensDto>>();
            Assert.NotNull(result);
            Assert.Null(result?.ErrorMessage);
            Assert.NotNull(result?.Data?.AuthToken);
            Assert.NotNull(result?.Data?.RefreshToken);
        }

        [Fact]
        public async Task Login_UnregisteredEmail_ReturnsNotFound()
        {
            // Arrange
            var dto = new UserRequestDto { Email = "nouser@test.com", Password = "Password123!" };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TokensDto>>();
            Assert.NotNull(result);
            Assert.Equal("Email unregistered", result?.ErrorMessage);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorizedAndError()
        {
            // Arrange
            var dto = new UserRequestDto { Email = "user2@test.com", Password = "RightPass1!" };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
            var wrongDto = new UserRequestDto { Email = dto.Email, Password = "WrongPass2!" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", wrongDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TokensDto>>();
            Assert.NotNull(result);
            Assert.Equal("Invalid credentials", result?.ErrorMessage);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task RefreshToken_ValidTokens_ReturnsOkAndNewTokensInBodyAndHeader()
        {
            // Arrange
            var dto = new UserRequestDto { Email = "refresh@test.com", Password = "Password123!" };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", dto);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            var authToken = loginResponse.Headers.GetValues("X-Auth-Token").First();
            var refreshToken = loginResponse.Headers.GetValues("X-Refresh-Token").First();
            Assert.NotNull(authToken);
            Assert.NotNull(refreshToken);
            var tokensDto = new TokensDto { AuthToken = authToken, RefreshToken = refreshToken };

            // Act
            var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", tokensDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
            Assert.True(refreshResponse.Headers.Contains("X-Auth-Token"));
            Assert.True(refreshResponse.Headers.Contains("X-Refresh-Token"));

            var result = await refreshResponse.Content.ReadFromJsonAsync<ResultDto<TokensDto>>();
            Assert.NotNull(result);
            Assert.Null(result?.ErrorMessage);
            Assert.NotNull(result?.Data?.AuthToken);
            Assert.NotNull(result?.Data?.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_InvalidAuthToken_ReturnsUnauthorizedAndError()
        {
            // Arrange
            var tokensDto = new TokensDto { AuthToken = "invalid.token.here", RefreshToken = "any" };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh", tokensDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TokensDto>>();
            Assert.NotNull(result);
            Assert.Equal("Invalid auth token", result?.ErrorMessage);
            Assert.Null(result?.Data);
        }

        [Fact]
        public async Task RefreshToken_InvalidRefreshToken_ReturnsUnauthorizedAndError()
        {
            // Arrange
            var dto = new UserRequestDto { Email = "refresh2@test.com", Password = "Password123!" };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", dto);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            var authToken = loginResponse.Headers.GetValues("X-Auth-Token").First();
            Assert.NotNull(authToken);            
            var tokensDto = new TokensDto { AuthToken = authToken, RefreshToken = "invalidRefresh" };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh", tokensDto);
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}