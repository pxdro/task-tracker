using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Profiles;
using TaskTracker.Infrastructure.Consumers;
using TaskTracker.Infrastructure.Context;
using TaskTracker.Infrastructure.Services;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

// Capture startup logs before build
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting TaskTracker in {Environment}", environment);

    var builder = WebApplication.CreateBuilder(args);

    // Load appsettings based on the environment
    builder.Configuration
           .SetBasePath(builder.Environment.ContentRootPath)
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables();

    // Configure Serilog
    Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine($"[Serilog SelfLog] {msg}"));
    builder.Host.UseSerilog((ctx, lc) =>
    {
        var serilogSection = ctx.Configuration.GetSection("Serilog");
        lc.ReadFrom.Configuration(serilogSection)
          .Enrich.FromLogContext()
          .Enrich.WithMachineName()
          .Enrich.WithEnvironmentName();
    });

    // Configure DbContext
    builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    {
        if (environment == "Testing")
            options.UseInMemoryDatabase("TestDb");
        else
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure());
    });

    // Configure JWT Auth
    var jwt = builder.Configuration.GetSection("JwtSettings");
    var key = Encoding.UTF8.GetBytes(jwt["AuthKey"]!);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                ClockSkew = TimeSpan.Zero
            };
        });

    // Configure MassTransit + RabbitMQ
    var rabbit = builder.Configuration.GetSection("RabbitMq");
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<TaskCreateEventConsumer>();
        x.AddConsumer<TaskUpdateEventConsumer>();
        x.AddConsumer<TaskDeleteEventConsumer>();

        if (environment == "Testing")
        {
            x.UsingInMemory((ctx, cfg) => {
                cfg.ReceiveEndpoint("task-create-log-events", e =>
                    e.ConfigureConsumer<TaskCreateEventConsumer>(ctx));
                cfg.ReceiveEndpoint("task-update-log-events", e =>
                    e.ConfigureConsumer<TaskUpdateEventConsumer>(ctx));
                cfg.ReceiveEndpoint("task-delete-log-events", e =>
                    e.ConfigureConsumer<TaskDeleteEventConsumer>(ctx));
            });
        }
        else
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbit["Host"]!, h =>
                {
                    h.Username(rabbit["User"]!);
                    h.Password(rabbit["Password"]!);
                });

                cfg.Message<TaskCreateEventDto>(m => m.SetEntityName("task.create"));
                cfg.Publish<TaskCreateEventDto>(p => p.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
                cfg.Message<TaskUpdateEventDto>(m => m.SetEntityName("task.update"));
                cfg.Publish<TaskUpdateEventDto>(p => p.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
                cfg.Message<TaskDeleteEventDto>(m => m.SetEntityName("task.delete"));
                cfg.Publish<TaskDeleteEventDto>(p => p.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);

                cfg.ReceiveEndpoint("task-create-log-events", e =>
                {
                    e.Bind("task.create", b => b.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
                    e.ConfigureConsumer<TaskCreateEventConsumer>(ctx);
                });
                cfg.ReceiveEndpoint("task-update-log-events", e =>
                {
                    e.Bind("task.update", b => b.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
                    e.ConfigureConsumer<TaskUpdateEventConsumer>(ctx);
                });
                cfg.ReceiveEndpoint("task-delete-log-events", e =>
                {
                    e.Bind("task.delete", b => b.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
                    e.ConfigureConsumer<TaskDeleteEventConsumer>(ctx);
                });
            });
        }
    });

    builder.Services.AddAuthorization();                    // Auth
    builder.Services.AddControllers();                      // Controllers
    builder.Services.AddEndpointsApiExplorer();             // API Explorer for Swagger
    builder.Services.AddSwaggerGen();                       // Swagger
    builder.Services.AddAutoMapper(typeof(MapperProfile));  // AutoMapper
    
    // Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/api/ping", () => Results.Text("ok"));

    // Ensure database is created and migrations are applied
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
        db.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("TaskTracker startup finished");
    Log.CloseAndFlush();
}

// For WebApplicationFactory tests on xUnit
public partial class Program { }
