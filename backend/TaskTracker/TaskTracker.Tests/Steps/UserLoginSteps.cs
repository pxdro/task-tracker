using TechTalk.SpecFlow;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class UserLoginSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
    {
        private readonly HttpClient _client = factory.CreateClient();
        private readonly ScenarioContext _ctx = ctx;

        [When(@"I login with email ""(.*)"" and password ""(.*)""")]
        public async Task WhenILoginWithEmailAndPassword(string email, string password)
        {
            var dto = new { Email = email, Password = password };
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            _ctx["response"] = response;

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var token = doc.RootElement.GetProperty("authToken").GetString();
                _ctx["authToken"] = token;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        [Then(@"I should receive a valid JWT auth token")]
        public async Task ThenIShouldReceiveAValidJWTAuthToken()
        {
            var response = (HttpResponseMessage)_ctx["response"]!;
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("authToken", out var token));
            Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
        }

        [Then(@"I should receive a valid refresh token")]
        public async Task ThenIShouldReceiveAValidRefreshToken()
        {
            var response = (HttpResponseMessage)_ctx["response"]!;
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("refreshToken", out var token));
            Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
        }
    }
}