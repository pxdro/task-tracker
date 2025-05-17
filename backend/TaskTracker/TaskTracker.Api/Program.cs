using System.Text;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Services;
using TaskTracker.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add context to the container (only if it's not Testing env)
var isTesting = builder.Environment.EnvironmentName == "Testing";
if (!isTesting)
{
    builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
}

// Add JWT Auth settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["AuthKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromMinutes(double.Parse(jwtSettings["ClockSkew"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();

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

app.MapGet("/api/tasks", (HttpContext http, string? field, string? value, ITaskService taskService) =>
{
    var user = http.User.Identity?.Name ?? "user@example.com";
    Task<List<TaskItem>> result;
    result = taskService.GetAll(user, field, value);
    return Results.Json(result.Result);
});

app.MapPost("/api/tasks", (HttpContext http, TaskItem task, ITaskService taskService) =>
{
    var user = http.User.Identity?.Name ?? "user@example.com";
    var response = taskService.Add(user, task);
    return Results.Json(response.Result);
});

app.MapPut("/api/tasks", (HttpContext http, string title, TaskItem task, ITaskService taskService) =>
{
    var user = http.User.Identity?.Name ?? "user@example.com";
    var updated = taskService.Update(user, title, task);
    return updated is null ? Results.NotFound() : Results.Json(updated.Result);
});

app.MapPatch("/api/tasks/{title}", async (string title, EnumTaskStatus status, HttpContext http, ITaskService taskService) =>
{
    var user = http.User.Identity?.Name ?? "user@example.com";
    var updated = await taskService.ChangeStatus(user, title, status);
    return updated is null ? Results.NotFound() : Results.Json(updated);
});

app.MapDelete("api/tasks/", (HttpContext http, string title, ITaskService taskService) =>
{
    var user = http.User.Identity?.Name ?? "user@example.com";
    var success = taskService.Delete(user, title);
    return success.Result ? Results.NoContent() : Results.NotFound();
});

app.Run();

public partial class Program { }