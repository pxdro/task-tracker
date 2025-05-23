using TechTalk.SpecFlow;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using TaskTracker.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class UserLoginSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
    {
        private readonly HttpClient _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();
        private readonly ScenarioContext _ctx = ctx;

        [When(@"I login with email ""(.*)"" and password ""(.*)""")]
        public async Task WhenILoginWithEmailAndPassword(string email, string password)
        {
            var dto = new { Email = email, Password = password };
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            _ctx["response"] = response;
        }

        [Then(@"I should receive valid tokens")]
        public async Task ThenIShouldReceiveValidTokens()
        {
            var response = (HttpResponseMessage)_ctx["response"]!;
            var content = await response.Content.ReadFromJsonAsync<ResultDto<TokensDto>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.Contains("X-Auth-Token"));
            Assert.True(response.Headers.Contains("X-Refresh-Token"));
            Assert.NotNull(content);
            Assert.Null(content?.ErrorMessage);
            Assert.NotNull(content?.Data?.AuthToken);
            Assert.NotNull(content?.Data?.RefreshToken);
        }
    }
}