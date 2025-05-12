using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using TechTalk.SpecFlow;
using System.Text.Json;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class TaskCompletionSteps
    {
        private readonly HttpClient _client;
        private readonly ScenarioContext _ctx;

        public TaskCompletionSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
        {
            _client = factory.CreateClient();
            _ctx = ctx;
        }

        [When(@"I mark the task with ID ""(.*)"" as completed")]
        public async Task WhenIMarkTheTaskWithIDAsCompleted(string taskId)
        {
            SetAuthorizationHeader();

            var response = await _client.PatchAsync($"/api/tasks/{taskId}/complete", null);
            _ctx["response"] = response;
        }

        [Then(@"the task status should be ""(.*)""")]
        public async Task ThenTheTaskStatusShouldBe(string expectedStatus)
        {
            var response = (HttpResponseMessage)_ctx["response"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var task = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

            Assert.NotNull(task);
            Assert.Equal(expectedStatus, task["status"].ToString());
        }

        private void SetAuthorizationHeader()
        {
            var token = _ctx["authToken"].ToString();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
