using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace TaskTracker.Tests.StepDefinitions
{
    [Binding]
    public class CommonSteps
    {
        private readonly HttpClient _client;
        private readonly ScenarioContext _ctx;

        public CommonSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
        {
            _client = factory.CreateClient();
            _ctx = ctx;
        }

        [Given(@"I am logged in as ""(.*)"" with password ""(.*)""")]
        public async Task GivenIAmLoggedInAsWithPassword(string email, string password)
        {
            var dto = new { Email = email, Password = password };
            await _client.PostAsJsonAsync("/api/auth/register", dto);
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", dto);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            _ctx["authToken"] = loginContent;
        }

        [Given(@"I have the following tasks")]
        public async Task GivenIHaveTheFollowingTasks(Table table)
        {
            SetAuthorizationHeader();

            foreach (var row in table.Rows)
            {
                var taskDto = new
                {
                    Title = row["title"],
                    Status = row["status"]
                };
                await _client.PostAsJsonAsync("/api/tasks", taskDto);
            }
        }

        private void SetAuthorizationHeader()
        {
            var token = _ctx["authToken"].ToString();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
