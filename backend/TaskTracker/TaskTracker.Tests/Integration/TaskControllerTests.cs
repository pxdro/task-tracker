using System.Net.Http.Json;
using System.Net;
using TaskTracker.Application.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using TaskTracker.Application.DTOs;

namespace TaskTracker.Tests.Integration
{
    public class TasksControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();

        private async Task AuthenticateAsync()
        {
            // Register user
            var userDto = new UserRequestDto { Email = "testuser@tasks.com", Password = "Pass123!" };
            await _client.PostAsJsonAsync("/api/auth/register", userDto);

            // Login
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", userDto);
            var authToken = loginResponse.Headers.GetValues("X-Auth-Token").First();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        private async Task<Guid?> CreateTaskViaEndpointAsync()
        {
            var taskDto = new TaskRequestDto { Title = "Test Task", Description = "Test Description" };
            var response = await _client.PostAsJsonAsync("/api/tasks", taskDto);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            return result?.Data?.Id;
        }

        [Fact]
        public async Task GetAll_AuthenticatedUser_ReturnsUserTasks()
        {
            // Arrange
            await AuthenticateAsync();
            await CreateTaskViaEndpointAsync();
            await CreateTaskViaEndpointAsync();

            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<IEnumerable<TaskReturnDto>>>();
            Assert.Equal(2, result?.Data?.Count());
        }

        [Fact]
        public async Task GetById_ValidTaskId_ReturnsTask()
        {
            // Arrange
            await AuthenticateAsync();
            var taskId = await CreateTaskViaEndpointAsync();

            // Act
            var response = await _client.GetAsync($"/api/tasks/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            Assert.Equal("Test Task", result?.Data?.Title);
        }

        [Fact]
        public async Task Update_ValidTask_ReturnsUpdatedTask()
        {
            // Arrange
            await AuthenticateAsync();
            var taskId = await CreateTaskViaEndpointAsync();
            var updateDto = new TaskRequestDto { Title = "Updated Title" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            Assert.Equal("Updated Title", result?.Data?.Title);
        }

        [Fact]
        public async Task Delete_ValidTask_ReturnsNoContent()
        {
            // Arrange
            await AuthenticateAsync();
            var taskId = await CreateTaskViaEndpointAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/tasks/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verifica se a task foi removida (opcional)
            var getResponse = await _client.GetAsync($"/api/tasks/{taskId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task MarkAsCompleted_ValidTask_ReturnsUpdatedStatus()
        {
            // Arrange
            await AuthenticateAsync();
            var taskId = await CreateTaskViaEndpointAsync();

            // Act
            var response = await _client.PatchAsync($"/api/tasks/{taskId}/completed", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            Assert.Equal(EnumTaskStatus.Completed, result?.Data?.Status);
        }
    }
}
