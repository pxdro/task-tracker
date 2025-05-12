using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using TechTalk.SpecFlow;
using System.Net.Http.Headers;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class TaskCrudSteps
    {
        private readonly HttpClient _client;
        private readonly ScenarioContext _ctx;

        public TaskCrudSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
        {
            _client = factory.CreateClient();
            _ctx = ctx;
        }

        [Given(@"I have 2 existing tasks in my account")]
        public async Task GivenIHave2ExistingTasksInMyAccount()
        {
            SetAuthorizationHeader();

            var task1 = new { Title = "Task 1", Description = "Description 1" };
            var task2 = new { Title = "Task 2", Description = "Description 2" };
            await _client.PostAsJsonAsync("/api/tasks", task1);
            await _client.PostAsJsonAsync("/api/tasks", task2);
        }

        [When(@"I request my task list")]
        public async Task WhenIRequestMyTaskList()
        {
            SetAuthorizationHeader();

            var response = await _client.GetAsync("/api/tasks");
            _ctx["response"] = response;
        }

        [Then(@"I should see my tasks")]
        public async Task ThenIShouldSeeMyTasks()
        {
            var response = (HttpResponseMessage)_ctx["response"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(content);
            Assert.NotNull(tasks);
            Assert.Equal(2, tasks.Count);
        }

        [When(@"I create a task with title ""(.*)"" and description ""(.*)""")]
        public async Task WhenICreateATaskWithTitleAndDescription(string title, string description)
        {
            SetAuthorizationHeader();

            var taskDto = new { Title = title, Description = description };
            var response = await _client.PostAsJsonAsync("/api/tasks", taskDto);
            _ctx["response"] = response;
        }

        [Then(@"the task should be saved")]
        public async Task ThenTheTaskShouldBeSaved()
        {
            var response = (HttpResponseMessage)_ctx["response"];
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var savedTask = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            Assert.NotNull(savedTask);
            Assert.True(savedTask.ContainsKey("id"), "Task should contain an id");

            var taskId = savedTask["id"].ToString();
            Assert.NotNull(taskId);
            Assert.False(string.IsNullOrEmpty(taskId), "Task id should not be empty");
        }

        [Given(@"I have a task with ID ""(.*)"" and title ""(.*)"" and description ""(.*)""")]
        public async Task GivenIHaveATaskWithIDAndTitleAndDescription(string taskId, string title, string description)
        {
            SetAuthorizationHeader();

            var taskDto = new { ID = taskId , Title = title, Description = description };
            await _client.PostAsJsonAsync("/api/tasks", taskDto);
            _ctx["taskId"] = taskId;
        }

        [When(@"I update this task's title to ""(.*)"" and description to ""(.*)""")]
        public async Task WhenIUpdateThisTaskTitleToAndDescriptionTo(string newTitle, string newDescription)
        {
            SetAuthorizationHeader();

            var taskId = _ctx["taskId"].ToString();
            var updatedTaskDto = new { ID = taskId, Title = newTitle, Description = newDescription };
            var response = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updatedTaskDto);
            _ctx["response"] = response;
            _ctx["newTitle"] = newTitle;
            _ctx["newDescription"] = newDescription;
        }

        [Then(@"the task should be updated")]
        public async Task ThenTheTaskShouldBeUpdated()
        {
            var response = (HttpResponseMessage)_ctx["response"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var updatedTask = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

            Assert.NotNull(updatedTask);
            Assert.Contains("title", updatedTask.Keys);
            Assert.Contains("description", updatedTask.Keys);

            var title = updatedTask["title"].ToString();
            var description = updatedTask["description"].ToString();

            Assert.Equal(_ctx["newTitle"], title);
            Assert.Equal(_ctx["newDescription"], description);
        }

        [When(@"I delete this task")]
        public async Task WhenIDeleteThisTask()
        {
            var taskId = _ctx["taskId"].ToString();
            var response = await _client.DeleteAsync($"/api/tasks/{taskId}");
            _ctx["response"] = response;
        }

        [Then(@"the task should no longer exist")]
        public void ThenTheTaskShouldNoLongerExist()
        {
            var response = (HttpResponseMessage)_ctx["response"];
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private void SetAuthorizationHeader()
        {
            var token = _ctx["authToken"].ToString();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
