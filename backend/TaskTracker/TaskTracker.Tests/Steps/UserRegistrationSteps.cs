using System.Net.Http.Json;
using TechTalk.SpecFlow;
using TaskTracker.Tests.Context;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class UserRegistrationSteps(TestContext dbContext, ScenarioContext scnearioContext)
    {
        private readonly TestContext _dbContext = dbContext;
        private readonly ScenarioContext _scnearioContext = scnearioContext;

        [When(@"I register email ""(.*)"" and password ""(.*)""")]
        public async Task WhenIRegisterEmailAndPassword(string email, string password)
        {
            var dto = new { Email = email, Password = password };
            var response = await _dbContext.Client.PostAsJsonAsync("/api/auth/register", dto);
            _scnearioContext["response"] = response;
        }

        [Then(@"I should receive a confirmation email")]
        public void ThenIShouldReceiveAConfirmationEmail()
        {
            var response = (HttpResponseMessage)_scnearioContext["response"]!;
            Assert.True(response.Headers.Contains("X-Confirmation-Sent"));
        }
    }
}
