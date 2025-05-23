using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TaskTracker.Application.Entities;
using TechTalk.SpecFlow;

namespace TaskTracker.Tests.Steps
{
    /*
    [Binding]
    public class TaskSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
    {
        private readonly HttpClient _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();
        private readonly ScenarioContext _ctx = ctx;

        [Given(@"I am logged in as ""(.*)"" with password ""(.*)""")]
        public async Task GivenIAmLoggedIn(string email, string password)
        {
            var dto = new { Email = email, Password = password };
            await _client.PostAsJsonAsync("/api/auth/register", dto);
            var loginResp = await _client.PostAsJsonAsync("/api/auth/login", dto);
            _ctx["response"] = loginResp;
            var content = await loginResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetProperty("authToken").GetString();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        [Given(@"I have the following tasks")]
        public async Task GivenIHaveTheFollowingTasks(Table table)
        {
            foreach (var row in table.Rows)
            {
                var task = new
                {
                    Title = row["title"],
                    Description = row["description"],
                    Status = row["status"]
                };
                var resp = await _client.PostAsJsonAsync("/api/tasks", task);
                _ctx["response"] = resp;
            }
        }

        [When(@"I request my task list")]
        public async Task WhenIRequestMyTaskList()
        {
            var resp = await _client.GetAsync("/api/tasks");
            _ctx["response"] = resp;
        }

        [When(@"I request tasks filtered by (.*) ""(.*)""")]
        public async Task WhenIRequestTasksFilteredBy(string field, string value)
        {
            var url = $"/api/tasks?field={Uri.EscapeDataString(field)}&value={Uri.EscapeDataString(value)}";
            var resp = await _client.GetAsync(url);
            _ctx["response"] = resp;
        }

        [When(@"I create a task with title ""(.*)"" and description ""(.*)""")]
        public async Task WhenICreateTask(string title, string description)
        {
            var dto = new { Title = title, Description = description };
            var resp = await _client.PostAsJsonAsync("/api/tasks", dto);
            _ctx["response"] = resp;
        }

        [When(@"I update the task titled ""(.*)"" to have title ""(.*)""")]
        public async Task WhenIUpdateTaskTitle(string oldTitle, string newTitle)
        {
            var dto = new { Title = newTitle, Description = "", Status = "Active" };
            var resp = await _client.PutAsJsonAsync($"/api/tasks?title={Uri.EscapeDataString(oldTitle)}", dto);
            _ctx["response"] = resp;
        }

        [When(@"I mark the task titled ""(.*)"" as completed")]
        public async Task WhenIMarkTaskAsCompleted(string title)
        {
            var url = $"/api/tasks/{Uri.EscapeDataString(title)}?status=Completed";
            var resp = await _client.PatchAsync(url, JsonContent.Create(new { status = "Completed" }));
            _ctx["response"] = resp;
        }

        [When(@"I delete the task titled ""(.*)""")]
        public async Task WhenIDeleteTask(string title)
        {
            var resp = await _client.DeleteAsync($"/api/tasks?title={Uri.EscapeDataString(title)}");
            _ctx["response"] = resp;
        }

        [Then(@"I should see (\d+) tasks")]
        public async Task ThenIShouldSeeTasksCount(int expected)
        {
            var resp = (HttpResponseMessage)_ctx["response"];
            var content = await resp.Content.ReadAsStringAsync();

            int count;
            if (content.TrimStart().StartsWith("["))
            {
                var list = JsonSerializer.Deserialize<List<TaskItem>>(content);
                count = list?.Count ?? 0;
            }
            else
            {
                var single = JsonSerializer.Deserialize<TaskItem>(content);
                count = single is null ? 0 : 1;
            }

            Assert.Equal(expected, count);
        }

        [Then(@"the task should be saved with status ""(.*)""")]
        public async Task ThenTaskShouldBeSavedWithStatus(string status)
        {
            var resp = (HttpResponseMessage)_ctx["response"];
            var task = await resp.Content.ReadFromJsonAsync<TaskItem>();
            Assert.Equal(status, task?.Status.ToString());
        }

        [Then(@"the task should have title ""(.*)""")]
        public async Task ThenTaskShouldHaveTitle(string expected)
        {
            var resp = (HttpResponseMessage)_ctx["response"];
            var task = await resp.Content.ReadFromJsonAsync<TaskItem>();
            Assert.Equal(expected, task?.Title);
        }

        [Then(@"the task ""(.*)"" should have status ""(.*)""")]
        public async Task ThenTaskShouldHaveStatus(string title, string status)
        {
            // fetch all
            await WhenIRequestMyTaskList();
            var tasks = await ((HttpResponseMessage)_ctx["response"]).Content.ReadFromJsonAsync<List<TaskItem>>();
            var task = tasks?.FirstOrDefault(t => t.Title == title);
            Assert.Equal(status, task?.Status.ToString());
        }

        [Then(@"the task ""(.*)"" should not exist anymore")]
        public async Task ThenTaskShouldNotExist(string title)
        {
            await WhenIRequestMyTaskList();
            var tasks = await ((HttpResponseMessage)_ctx["response"]).Content.ReadFromJsonAsync<List<TaskItem>>();
            Assert.DoesNotContain(tasks, t => t.Title == title);
        }
    }
    */
}