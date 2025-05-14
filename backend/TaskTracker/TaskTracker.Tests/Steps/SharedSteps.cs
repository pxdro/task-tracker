using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using TechTalk.SpecFlow;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class SharedSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
    {
        private readonly HttpClient _client = factory.CreateClient();
        private readonly ScenarioContext _ctx = ctx;

        [Given(@"the API is running")]
        public async Task GivenTheAPIIsRunning()
        {
            var response = await _client.GetAsync("/api/ping");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Given(@"I have registered with email ""(.*)"" and password ""(.*)""")]
        public async Task GivenIHaveRegisteredWithEmailAndPassword(string email, string password)
        {
            var dto = new { Email = email, Password = password };
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _ctx["response"] = response;
        }

        [Then(@"I should be returned code (\d+)")]
        public void ThenIShouldBeReturnedCode(int expectedStatus)
        {
            var response = (HttpResponseMessage)_ctx["response"]!;
            Assert.Equal((HttpStatusCode)expectedStatus, response.StatusCode);
        }

        [Then(@"I should see the message ""(.*)""")]
        public async Task ThenIShouldSeeTheMessage(string expectedMessage)
        {
            var response = (HttpResponseMessage)_ctx["response"]!;
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedMessage, content);
        }
    }
}
