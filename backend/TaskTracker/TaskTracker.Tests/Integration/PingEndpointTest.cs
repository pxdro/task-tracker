using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace TaskTracker.Tests.Integration
{
    public class PingEndpointTest(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task Ping_Test()
        {
            var response = await _client.PostAsync("/test", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
