using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskTracker.Tests.Integration
{
    public class TaskFilteringEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TaskFilteringEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<string> AuthenticateAndSeedTasksAsync()
        {
            var user = new { email = "user@example.com", password = "Str0ngP@ss!" };

            // Register
            await _client.PostAsync("/api/auth/register",
                new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));

            // Login
            var loginResponse = await _client.PostAsync("/api/auth/login",
                new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));

            var loginBody = await loginResponse.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginBody);
            var token = loginDoc.RootElement.GetProperty("authToken").GetString();

            // Add token to Authorization header
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Seed tasks
            var tasks = new[]
            {
                new { title = "Write report", status = "Active" },
                new { title = "Review PR", status = "Completed" },
                new { title = "Plan meeting", status = "Active" }
            };

            foreach (var task in tasks)
            {
                var taskJson = JsonSerializer.Serialize(task);
                await _client.PostAsync("/api/tasks",
                    new StringContent(taskJson, Encoding.UTF8, "application/json"));
            }

            return token!;
        }

        [Fact]
        public async Task FilterTasks_WithValidStatus_ReturnsFilteredList()
        {
            // Arrange
            await AuthenticateAndSeedTasksAsync();

            // Act
            var response = await _client.GetAsync("/api/tasks?status=Active");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var doc = JsonDocument.Parse(body);
            var tasks = doc.RootElement.GetProperty("tasks").EnumerateArray().ToList();
            Assert.Equal(2, tasks.Count);

            foreach (var task in tasks)
            {
                var status = task.GetProperty("status").GetString();
                Assert.Equal("Active", status);
            }
        }

        [Fact]
        public async Task FilterTasks_WithInvalidStatus_ReturnsBadRequest()
        {
            // Arrange
            await AuthenticateAndSeedTasksAsync();

            // Act
            var response = await _client.GetAsync("/api/tasks?status=Unknown");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("\"message\":\"Invalid status\"", body);
        }
    }
}