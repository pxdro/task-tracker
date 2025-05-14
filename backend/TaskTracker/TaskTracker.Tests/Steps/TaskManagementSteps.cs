using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TechTalk.SpecFlow;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class TaskManagementSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
    {
        private readonly HttpClient _client = factory.CreateClient();
        private readonly ScenarioContext _ctx = ctx;


        [Given(@"I am logged in as ""(.*)"" with password ""(.*)""")]
        public async Task GivenIAmLoggedInAsWithPassword(string email, string password)
        {
            var dto = new { Email = email, Password = password };

            // Register
            await _client.PostAsJsonAsync("/api/auth/register", dto);

            // Login
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetProperty("authToken").GetString();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(token);

            _ctx["authToken"] = token!;
            _ctx["email"] = email;
            SetAuthorizationHeader();
        }

        [Given(@"I have the following tasks")]
        public async Task GivenIHaveTheFollowingTasks(Table table)
        {
            foreach (var row in table.Rows)
            {
                var task = new TaskItem
                {
                    Title = row["title"],
                    Description = row["description"],
                    Status = Enum.TryParse(row["status"], out EnumTaskStatus status) ? status : EnumTaskStatus.Active
                };

                var response = await _client.PostAsJsonAsync("/api/tasks", task);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [When(@"I request my task list")]
        public async Task WhenIRequestMyTaskList()
        {
            var response = await _client.GetAsync($"/api/tasks?userEmail={_ctx["email"]}");
            _ctx["response"] = response;
        }

        [When(@"I request tasks filtered by (.*) ""(.*)""")]
        public async Task WhenIRequestTasksFilteredBy(string field, string value)
        {
            var url = $"/api/tasks?userEmail={_ctx["email"]}&field={field}&value={value}";
            var response = await _client.GetAsync(url);
            _ctx["response"] = response;
        }

        [Then(@"I should see (\d+) tasks")]
        public async Task ThenIShouldSeeTasksCount(int expected)
        {
            var response = (HttpResponseMessage)_ctx["response"]!;
            var content = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(content);
            var tasks = doc.RootElement.GetProperty("result").Deserialize<List<TaskItem>>()!;
            Assert.Equal(expected, tasks.Count);
        }

        /*--*/
        [When(@"I create a task with title ""(.*)"" and description ""(.*)""")]
        public async Task WhenICreateTask(string title, string description)
        {
            var dto = new { Title = title, Description = description };
            var response = await _client.PostAsJsonAsync("/api/tasks", dto);
            _ctx["response"] = response;
        }

        [When(@"I update the task titled ""(.*)"" to have title ""(.*)""")]
        public async Task WhenIUpdateTaskTitle(string oldTitle, string newTitle)
        {
            await WhenIRequestMyTaskList();
            var content = await ((HttpResponseMessage)_ctx["response"]).Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<JsonElement>>(content)!;
            var task = tasks.First(t => t.GetProperty("title").GetString() == oldTitle);
            var id = task.GetProperty("id").GetGuid();

            var dto = new { Title = newTitle };
            var response = await _client.PutAsJsonAsync($"/api/tasks/{id}", dto);
            _ctx["response"] = response;
        }

        [When(@"I mark the task titled ""(.*)"" as completed")]
        public async Task WhenIMarkTaskAsCompleted(string title)
        {
            // Find ID
            await WhenIRequestMyTaskList();
            var content = await ((HttpResponseMessage)_ctx["response"]).Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<JsonElement>>(content)!;
            var task = tasks.First(t => t.GetProperty("title").GetString() == title);
            var id = task.GetProperty("id").GetGuid();

            var response = await _client.PatchAsync($"/api/tasks/{id}/complete", null);
            _ctx["response"] = response;
        }

        [When(@"I delete the task titled ""(.*)""")]
        public async Task WhenIDeleteTask(string title)
        {
            // Find ID
            await WhenIRequestMyTaskList();
            var content = await ((HttpResponseMessage)_ctx["response"]).Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<JsonElement>>(content)!;
            var task = tasks.First(t => t.GetProperty("title").GetString() == title);
            var id = task.GetProperty("id").GetGuid();

            var response = await _client.DeleteAsync($"/api/tasks/{id}");
            _ctx["response"] = response;
        }

        [Then(@"the task ""(.*)"" should have status ""(.*)""")]
        public async Task ThenTaskShouldHaveStatus(string title, string status)
        {
            await WhenIRequestMyTaskList();
            var content = await ((HttpResponseMessage)_ctx["response"]).Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<JsonElement>>(content)!;
            var task = tasks.First(t => t.GetProperty("title").GetString() == title);
            var actual = task.GetProperty("status").GetString();
            Assert.Equal(status, actual);
        }

        [Then(@"the task ""(.*)"" should have title ""(.*)""")]
        public async Task ThenTaskShouldHaveTitle(string oldTitle, string newTitle)
        {
            await WhenIRequestMyTaskList();
            var content = await ((HttpResponseMessage)_ctx["response"]).Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<JsonElement>>(content)!;
            var task = tasks.First(t => t.GetProperty("id").GetGuid().ToString() == oldTitle ||
                                        t.GetProperty("title").GetString() == newTitle);
            Assert.Equal(newTitle, task.GetProperty("title").GetString());
        }

        [Then(@"the task ""(.*)"" should not exist anymore")]
        public async Task ThenTaskShouldNotExist(string title)
        {
            await WhenIRequestMyTaskList();
            var content = await ((HttpResponseMessage)_ctx["response"]).Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<JsonElement>>(content)!;
            Assert.DoesNotContain(tasks, t => t.GetProperty("title").GetString() == title);
        }

        [Then(@"the task should be saved with status ""(.*)""")]
        public async Task ThenTaskShouldBeSavedWithStatus(string expectedStatus)
        {
            var response = (HttpResponseMessage)_ctx["response"]!;
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var status = doc.RootElement.GetProperty("status").GetString();
            Assert.Equal(expectedStatus, status);
        }
        /*--*/

        private void SetAuthorizationHeader()
        {
            if (_ctx.TryGetValue("authToken", out var tokenObj) && tokenObj is string token)
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
