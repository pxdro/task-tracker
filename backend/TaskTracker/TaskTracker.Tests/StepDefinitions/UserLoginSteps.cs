using System.Net;
using TechTalk.SpecFlow;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace TaskTracker.Tests.StepDefinitions
{
    [Binding]
    public class UserLoginSteps
    {
        private readonly HttpClient _client;
        private readonly ScenarioContext _ctx;

        public UserLoginSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
        {
            _client = factory.CreateClient();
            _ctx = ctx;
        }

        [Given(@"I have registered with email ""(.*)"" and password ""(.*)""")]
        public async Task GivenIHaveRegisteredWithEmailAndPassword(string email, string password)
        {
            _ctx["email"] = email;
            _ctx["password"] = password;
            _ctx["dto"] = new { Email = email, Password = password };
            var response = await _client.PostAsJsonAsync("/api/auth/register", _ctx["dto"]);

            Assert.True(response.StatusCode == HttpStatusCode.Created, $"Expected 201, got {response.StatusCode}");
        }

        [When(@"I submit these email and password")]
        public async Task WhenISubmitTheseEmailAndPassword()
        {
            _ctx["response"] = await _client.PostAsJsonAsync("/api/auth/login", _ctx["dto"]);
        }

        [Then(@"I should login successfully")]
        public async Task ThenIShouldLoginSuccessfully()
        {
            var response = (HttpResponseMessage)_ctx["response"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("authToken", body);
            Assert.Contains("refreshToken", body);
        }

        [When(@"I submit this email and wrong password")]
        public async Task WhenISubmitThisEmailAndWrongPassword()
        {
            var wrongDto = new { Email = _ctx["email"], Password = "Wr0ngP@ss!" };
            _ctx["response"] = await _client.PostAsJsonAsync("/api/auth/login", wrongDto);
        }

        [When(@"I submit unregistered email and any password")]
        public async Task WhenISubmitUnregisteredEmailAndAnyPassword()
        {
            var anyDto = new { Email = "unknown@example.com", Password = "AnyPass123" };
            _ctx["response"] = await _client.PostAsJsonAsync("/api/auth/login", anyDto);
        }


        [Then(@"I should see the error message ""(.*)"" for user login")]
        public async Task ThenIShouldSeeTheErrorMessageForUserLogin(string errorMessage)
        {
            var response = (HttpResponseMessage)_ctx["response"];
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(errorMessage, content);
        }
    }
}