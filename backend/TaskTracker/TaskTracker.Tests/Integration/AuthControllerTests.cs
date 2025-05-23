using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using TaskTracker.Application.DTOs;
using Microsoft.AspNetCore.Hosting;

namespace TaskTracker.Tests.Integration
{
    public class AuthControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();

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
        public async Task Register_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var dto1 = new UserRequestDto { Email = "invalid-email", Password = "Password123!" };
            var dto2 = new UserRequestDto { Email = "user@test.com", Password = "" };

            // Act
            var response1 = await _client.PostAsJsonAsync("/api/auth/register", dto1);
            var response2 = await _client.PostAsJsonAsync("/api/auth/register", dto2);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
            var result1 = await response1.Content.ReadAsStringAsync();
            var result2 = await response2.Content.ReadAsStringAsync();
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Contains("errors", result1);
            Assert.Contains("errors", result2);
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