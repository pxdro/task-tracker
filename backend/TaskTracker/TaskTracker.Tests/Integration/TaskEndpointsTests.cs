using System.Net.Http.Json;
using System.Net;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Tests.Integration
{
    /*public class TaskEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();
        private readonly WebApplicationFactory<Program> _factory = factory;

        [Fact]
        public async Task CreateTask_Should_Succeed()
        {
            // Arrange
            ResetTaskService();
            var task = new TaskItem
            {
                Title = "Review PR",
                Description = "ASAP",
                Status = EnumTaskStatus.Active
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/tasks", task);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var returned = await response.Content.ReadFromJsonAsync<TaskItem>();
            Assert.Equal("Review PR", returned?.Title);
            Assert.Equal(EnumTaskStatus.Active, returned?.Status);
        }

        [Fact]
        public async Task GetAllTasks_Should_Return_Three_Completed()
        {
            // Arrange
            ResetTaskService();
            await AddSampleTasksAsync();

            // Act
            var response = await _client.GetAsync("/api/tasks");
            var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(tasks);
            Assert.Equal(3, tasks.Count);
        }

        [Theory]
        [InlineData("title", "Write", 1)]
        [InlineData("description", "For home", 2)]
        [InlineData("status", "Completed", 2)]
        [InlineData("status", "Active", 1)]
        public async Task FilterTasks_Should_Return_Expected(string field, string value, int expectedCount)
        {
            // Arrange
            ResetTaskService();
            await AddSampleTasksAsync();

            // Act
            var response = await _client.GetAsync($"/api/tasks?field={field}&value={value}");
            var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(tasks);
            Assert.Equal(expectedCount, tasks.Count);
        }

        [Fact]
        public async Task UpdateTask_Should_Replace_All_Fields()
        {
            // Arrange
            ResetTaskService();
            await AddSampleTasksAsync();

            var updated = new TaskItem
            {
                Title = "Buy groceries and fruits",
                Description = "Extra items",
                Status = EnumTaskStatus.Active
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/tasks?title=Buy groceries", updated);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var task = await response.Content.ReadFromJsonAsync<TaskItem>();
            Assert.Equal("Buy groceries and fruits", task?.Title);
        }

        [Fact]
        public async Task PatchTask_Should_Update_Status()
        {
            // Arrange
            ResetTaskService();
            await AddSampleTasksAsync();
            var requestUri = "/api/tasks/Write report?status=Completed";
            var content = new StringContent("", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PatchAsync(requestUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var task = await response.Content.ReadFromJsonAsync<TaskItem>();
            Assert.NotNull(task);
            Assert.Equal(EnumTaskStatus.Completed, task!.Status);
        }

        [Fact]
        public async Task DeleteTask_Should_Remove_It()
        {
            // Arrange
            ResetTaskService();
            await AddSampleTasksAsync();

            // Act
            var deleteResponse = await _client.DeleteAsync("/api/tasks?title=Buy groceries");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync("/api/tasks");
            var tasks = await getResponse.Content.ReadFromJsonAsync<List<TaskItem>>();
            Assert.DoesNotContain(tasks!, t => t.Title == "Buy groceries");
        }

        private async Task AddSampleTasksAsync()
        {
            var tasks = new[]
                {
                new TaskItem { Title = "Write report", Description = "For work", Status = EnumTaskStatus.Active },
                new TaskItem { Title = "Buy groceries", Description = "For home", Status = EnumTaskStatus.Completed },
                new TaskItem { Title = "Dump the thrash", Description = "For home", Status = EnumTaskStatus.Completed }
            };

            foreach (var task in tasks)
            {
                await _client.PostAsJsonAsync("/api/tasks", task);
            }
        }

        private void ResetTaskService()
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory!.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>() as TaskService;
            taskService?.Reset();
        }
    }*/
}
