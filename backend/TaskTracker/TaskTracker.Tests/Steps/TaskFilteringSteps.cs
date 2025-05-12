using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using TechTalk.SpecFlow;
using System.Text.Json;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class TaskFilteringSteps
    {
        private readonly HttpClient _client;
        private readonly ScenarioContext _ctx;

        public TaskFilteringSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
        {
            _client = factory.CreateClient();
            _ctx = ctx;
        }

        [When(@"I filter tasks by status ""(.*)""")]
        public async Task WhenIFilterTasksByStatus(string status)
        {
            SetAuthorizationHeader();

            var response = await _client.GetAsync($"/api/tasks?status={status}");
            _ctx["response"] = response;
        }

        [Then(@"I should see (.*) active tasks")]
        public async Task ThenIShouldSeeActiveTasks(int expectedCount)
        {
            var response = (HttpResponseMessage)_ctx["response"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(content);
            Assert.NotNull(tasks);
            Assert.Equal(expectedCount, tasks.Count);

            // Ensure that the tasks are of "Active" status
            foreach (var task in tasks)
            {
                Assert.Equal("Active", task["status"].ToString());
            }
        }

        private void SetAuthorizationHeader()
        {
            var token = _ctx["authToken"].ToString();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
