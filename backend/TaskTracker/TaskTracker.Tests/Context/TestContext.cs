using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Infrastructure.Context;

namespace TaskTracker.Tests.Context
{
    public class TestContext
    {
        public HttpClient Client { get; private set; }
        public string DbName { get; } = $"TestDb_{Guid.NewGuid()}";

        public TestContext()
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<TaskTrackerDbContext>));
                        if (descriptor != null)
                            services.Remove(descriptor);

                        services.AddDbContext<TaskTrackerDbContext>(options =>
                            options.UseInMemoryDatabase(DbName));
                    });
                });

            Client = factory.CreateClient();
        }
    }
}
