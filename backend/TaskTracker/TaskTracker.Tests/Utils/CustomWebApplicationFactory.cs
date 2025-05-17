using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Infrastructure.Context;

namespace TaskTracker.Tests.Utils
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove todos os DbContextOptions<ApplicationDbContext>
                var contextDescriptors = services
                    .Where(d => d.ServiceType.FullName?.Contains("DbContextOptions") == true)
                    .ToList();
                foreach (var descriptor in contextDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Registra InMemory isolado
                services.AddDbContext<TaskTrackerDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

                // Cria banco na inicialização
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        }
    }
}
