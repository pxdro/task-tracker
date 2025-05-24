using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Json;
using TaskTracker.Infrastructure.Context;
using TechTalk.SpecFlow;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class UserRegistrationSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
    {
        private readonly HttpClient _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();
        private readonly ScenarioContext _ctx = ctx;

        [When(@"I register email ""(.*)"" and password ""(.*)""")]
        public async Task WhenIRegisterEmailAndPassword(string email, string password)
        {
            var dto = new { Email = email, Password = password };
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            _ctx["response"] = response;
        }

        [Then(@"I should receive a confirmation email")]
        public void ThenIShouldReceiveAConfirmationEmail()
        {
            var response = (HttpResponseMessage)_ctx["response"]!;
            Assert.True(response.Headers.Contains("X-Confirmation-Sent"));
        }
    }
}
