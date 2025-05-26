using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Profiles;
using TaskTracker.Infrastructure.Consumers;
using TaskTracker.Infrastructure.Context;
using TaskTracker.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add context to the container only if not Testing Env (InMemory DB)
builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
        options.UseInMemoryDatabase("TestDb");
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
        
// Add JWT Auth settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["AuthKey"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero,
        };
    });

// Add MassTransit and RabbitMQ settings
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq");
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TaskCreateEventConsumer>();
    x.AddConsumer<TaskUpdateEventConsumer>();
    x.AddConsumer<TaskDeleteEventConsumer>();

    // For Testing env, use InMemory transport
    if (builder.Environment.IsEnvironment("Testing"))
    {
        x.UsingInMemory((context, cfg) =>
        {
            cfg.ReceiveEndpoint("task-create-log-events", e =>
                e.ConfigureConsumer<TaskCreateEventConsumer>(context));
            cfg.ReceiveEndpoint("task-update-log-events", e =>
                e.ConfigureConsumer<TaskUpdateEventConsumer>(context));
            cfg.ReceiveEndpoint("task-delete-log-events", e =>
                e.ConfigureConsumer<TaskDeleteEventConsumer>(context));
        });
    }
    // For Normal envs, use RabbitMQ transport
    else
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqSettings["Host"], h =>
            {
                h.Username(rabbitMqSettings["User"]);
                h.Password(rabbitMqSettings["Password"]);
            });

            // exchanges fanout
            cfg.Message<TaskCreateEventDto>(m => m.SetEntityName("task.create"));
            cfg.Publish<TaskCreateEventDto>(p => p.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
            cfg.Message<TaskUpdateEventDto>(m => m.SetEntityName("task.update"));
            cfg.Publish<TaskUpdateEventDto>(p => p.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
            cfg.Message<TaskDeleteEventDto>(m => m.SetEntityName("task.delete"));
            cfg.Publish<TaskDeleteEventDto>(p => p.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);

            // filas ligadas aos exchanges
            cfg.ReceiveEndpoint("task-create-log-events", e =>
            {
                e.Bind("task.create", b => b.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
                e.ConfigureConsumer<TaskCreateEventConsumer>(context);
            });
            cfg.ReceiveEndpoint("task-update-log-events", e =>
            {
                e.Bind("task.update", b => b.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
                e.ConfigureConsumer<TaskUpdateEventConsumer>(context);
            });
            cfg.ReceiveEndpoint("task-delete-log-events", e =>
            {
                e.Bind("task.delete", b => b.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout);
                e.ConfigureConsumer<TaskDeleteEventConsumer>(context);
            });
        });
    }
});

// Add services to the container.
builder.Services.AddAuthorization();                    // Auth
builder.Services.AddControllers();                      // Controllers
builder.Services.AddEndpointsApiExplorer();             // Swagger
builder.Services.AddSwaggerGen();                       // Swagger
builder.Services.AddAutoMapper(typeof(MapperProfile));  // AutoMapper
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/ping", () => "ok");

app.Run();

public partial class Program { }