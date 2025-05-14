using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Tests.Integration
{
    public class TaskCompletionEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();
        private Guid _id = Guid.Empty;

        private async Task SeedUserAndTasksAsync()
        {
            var user = new { email = "user@example.com", password = "Str0ngP@ss!" };

            // Register & login
            await _client.PostAsJsonAsync("/api/auth/register", user);
            var login = await _client.PostAsJsonAsync("/api/auth/login", user);
            var body = await login.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            var token = doc.RootElement.GetProperty("authToken").GetString();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Seed tasks
            var task1 = new TaskItem { Title = "Write report", Status = EnumTaskStatus.Active };
            var task2 = new TaskItem { Title = "Buy groceries", Status = EnumTaskStatus.Completed };
            var tasks = new[] { task1, task2 };
            _id = task1.Id;

            foreach (var task in tasks)
                await _client.PostAsJsonAsync("/api/tasks", task);
        }

        [Fact]
        public async Task MarkTaskAsCompleted_ShouldReturnOk_AndUpdateStatus()
        {
            await SeedUserAndTasksAsync();

            var response = await _client.PatchAsync($"/api/tasks/{_id}/complete", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var get = await _client.GetAsync("/api/tasks?status=Completed");
            var content = await get.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var tasks = doc.RootElement.GetProperty("tasks").EnumerateArray().ToList();
            Assert.Contains(tasks, t => t.GetProperty("id").GetString() == _id.ToString());
        }
    }

}
