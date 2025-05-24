using System.Net.Http.Json;
using System.Net;
using TaskTracker.Application.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using TaskTracker.Application.DTOs;
using System.Threading.Tasks;

namespace TaskTracker.Tests.Integration
{
    public class TasksControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();


        private async Task AuthenticateAsync(string userEmail = "user@example.com")
        {
            // Register user
            var userDto = new UserRequestDto { Email = userEmail, Password = "Pass123!" };
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
        public async Task GetAll_ReturnsUserTasks()
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
            Assert.NotNull(result?.Data);
            Assert.Null(result?.ErrorMessage);
            Assert.True(result?.Data?.Any());
        }

        [Fact]
        public async Task GetByFilter_ReturnsUserFilteredTasks()
        {
            // Arrange
            await AuthenticateAsync();
            await CreateTaskViaEndpointAsync();

            // Act
            var response = await _client.GetAsync("/api/tasks/filter?field=title&value=Test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<IEnumerable<TaskReturnDto>>>();
            Assert.NotNull(result?.Data);
            Assert.Null(result?.ErrorMessage);
            Assert.True(result?.Data?.Any());
            foreach (var task in result?.Data!)
            {
                Assert.Contains("Test", task.Title);
            }
        }

        [Fact]
        public async Task GetByFilter_InvalidField_ReturnsBadRequest()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("/api/tasks/filter?field=test&value=Test");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<IEnumerable<TaskReturnDto>>>();
            Assert.Null(result?.Data);
            Assert.NotNull(result?.ErrorMessage);
            Assert.Equal("Invalid field", result?.ErrorMessage);
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
            Assert.NotNull(result?.Data);
            Assert.Null(result?.ErrorMessage);
            Assert.Equal("Test Task", result?.Data?.Title);
        }

        [Fact]
        public async Task GetById_NoTaskWithId_ReturnsNotFound()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            Assert.Null(result?.Data);
            Assert.NotNull(result?.ErrorMessage);
            Assert.Equal("Task not found", result?.ErrorMessage);
        }

        [Fact]
        public async Task GetById_OtherUserTask_ReturnsUnauthorized()
        {
            // Arrange
            await AuthenticateAsync();
            var taskId = await CreateTaskViaEndpointAsync();
            await AuthenticateAsync("otheruser@example.com");
            await CreateTaskViaEndpointAsync();

            // Act
            var response = await _client.GetAsync($"/api/tasks/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            Assert.Null(result?.Data);
            Assert.NotNull(result?.ErrorMessage);
            Assert.Equal("Access denied", result?.ErrorMessage);
        }

        [Fact]
        public async Task Create_ValidTask_ReturnsTask()
        {
            // Arrange
            await AuthenticateAsync();
            var taskDto = new TaskRequestDto { Title = "Other Test Task", Description = "Other Test Description" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/tasks", taskDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            Assert.NotNull(result?.Data);
            Assert.Null(result?.ErrorMessage);
            Assert.Equal("Other Test Task", result?.Data?.Title);
            Assert.Equal("Other Test Description", result?.Data?.Description);
            Assert.Equal(EnumTaskStatus.Active, result?.Data?.Status);
        }

        [Fact]
        public async Task Update_ValidTask_ReturnsUpdatedTask()
        {
            // Arrange
            await AuthenticateAsync();
            var taskId = await CreateTaskViaEndpointAsync();
            var updateDto = new TaskRequestDto { Title = "New title", Description = "New description", Status = EnumTaskStatus.Completed };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            Assert.NotNull(result?.Data);
            Assert.Null(result?.ErrorMessage);
            Assert.Equal("New title", result?.Data?.Title);
            Assert.Equal("New description", result?.Data?.Description);
            Assert.Equal(EnumTaskStatus.Completed, result?.Data?.Status);
        }

        [Fact]
        public async Task Update_NoExistentTask_ReturnsNotFound()
        {
            // Arrange
            await AuthenticateAsync();
            var updateDto = new TaskRequestDto { Title = "Updated Title" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            Assert.Null(result?.Data);
            Assert.NotNull(result?.ErrorMessage);
            Assert.Equal("Task not found", result?.ErrorMessage);
        }

        [Fact]
        public async Task Update_OtherUserTask_ReturnsUnauthorized()
        {
            // Arrange
            await AuthenticateAsync();
            var taskId = await CreateTaskViaEndpointAsync();
            await AuthenticateAsync("otheruser@example.com");
            await CreateTaskViaEndpointAsync();
            var updateDto = new TaskRequestDto { Title = "Updated Title" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();
            Assert.Null(result?.Data);
            Assert.NotNull(result?.ErrorMessage);
            Assert.Equal("Access denied", result?.ErrorMessage);
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
            var result = await response.Content.ReadFromJsonAsync<ResultDto<bool>>();
            Assert.True(result?.Data);
            Assert.Null(result?.ErrorMessage);
        }

        [Fact]
        public async Task Delete_NoExistentTask_ReturnsNotFound()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<bool>>();
            Assert.False(result?.Data);
            Assert.NotNull(result?.ErrorMessage);
            Assert.Equal("Task not found", result?.ErrorMessage);
        }

        [Fact]
        public async Task Delete_OtherUserTask_ReturnsUnauthorized()
        {
            // Arrange
            await AuthenticateAsync();
            var taskId = await CreateTaskViaEndpointAsync();
            await AuthenticateAsync("otheruser@example.com");
            await CreateTaskViaEndpointAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/tasks/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ResultDto<bool>>();
            Assert.False(result?.Data);
            Assert.NotNull(result?.ErrorMessage);
            Assert.Equal("Access denied", result?.ErrorMessage);
        }
    }
}
