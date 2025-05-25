using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

// Add context to the container only if not Testing Env
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
var rabbitMqHost = rabbitMqSettings["Host"];
var rabbitMqUser = rabbitMqSettings["User"];
var rabbitMqPassword = rabbitMqSettings["Password"];
if (rabbitMqHost != null && rabbitMqUser != null && rabbitMqPassword != null)
{
    builder.Services.AddMassTransit(x =>
    {
        // Consumer registration
        x.AddConsumer<TaskCreatedOrUpdatedConsumer>();

        // Bus registration
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqHost, h =>
            {
                h.Username(rabbitMqUser);
                h.Password(rabbitMqPassword);
            });

            // Queue creation attached to consumer
            cfg.ReceiveEndpoint("task-tracker-task-events", e =>
            {
                e.ConfigureConsumer<TaskCreatedOrUpdatedConsumer>(context);
            });
        });
    });
}

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