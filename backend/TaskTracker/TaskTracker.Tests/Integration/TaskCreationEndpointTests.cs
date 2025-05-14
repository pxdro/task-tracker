using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Tests.Integration
{
    public class TaskCreationEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TaskCreationEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task AuthenticateAsync()
        {
            var user = new { email = "user@example.com", password = "Str0ngP@ss!" };
            await _client.PostAsJsonAsync("/api/auth/register", user);
            var login = await _client.PostAsJsonAsync("/api/auth/login", user);

            var body = await login.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            var token = doc.RootElement.GetProperty("authToken").GetString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task PostTask_ShouldReturn200_WhenTaskCreated()
        {
            await AuthenticateAsync();

            var task = new TaskItem
            {
                Title = "New Task",
                Status = EnumTaskStatus.Active
            };

            var response = await _client.PostAsJsonAsync("/api/tasks", task);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PostTask_ShouldReturn409_WhenDuplicateId()
        {
            await AuthenticateAsync();

            var task = new TaskItem
            {
                Title = "New Task",
                Status = EnumTaskStatus.Active
            };

            await _client.PostAsJsonAsync("/api/tasks", task);
            var response = await _client.PostAsJsonAsync("/api/tasks", task);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Task already exists", content);
        }
    }
}
