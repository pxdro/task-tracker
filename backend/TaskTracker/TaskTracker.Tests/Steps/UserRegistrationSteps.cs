using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using TechTalk.SpecFlow;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class UserRegistrationSteps
    {
        private readonly HttpClient _client;
        private readonly ScenarioContext _ctx;

        public UserRegistrationSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
        {
            _client = factory.CreateClient();
            _ctx = ctx;
        }
        
        [When(@"I submit valid email ""(.*)"" and password ""(.*)""")]
        public async Task WhenISubmitValidEmailAndPassword(string email, string password)
        {
            _ctx["email"] = email;
            var dto = new { Email = email, Password = password };
            _ctx["response"] = await _client.PostAsJsonAsync("/api/auth/register", dto);
        }
        
        [Then(@"I should receive a confirmation email")]
        public void ThenIShouldReceiveAConfirmationEmail()
        {
            var response = (HttpResponseMessage)_ctx["response"];
            Assert.True(response.Headers.Contains("X-Confirmation-Sent"));
        }
        
        [Given(@"I have already registered with email ""(.*)""")]
        public async Task GivenIHaveAlreadyRegisteredWithEmail(string email)
        {
            var dto = new { Email = email, Password = "AnyP@ss123" };
            var initial = await _client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, initial.StatusCode);
            _ctx["email"] = email;
        }

        [When(@"I submit this email and any password")]
        public async Task WhenISubmitThisEmailAndAnyPassword()
        {
            var email = _ctx["email"].ToString();
            var dto = new { Email = email, Password = "AnyP@ss123" };
            _ctx["response"] = await _client.PostAsJsonAsync("/api/auth/register", dto);
        }
    }
}
