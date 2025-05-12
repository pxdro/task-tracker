using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapPost("/test", () => "ok");

app.MapPost("/api/auth/register", async (RegisterDto dto, IUserService userService, HttpContext context) =>
{
    var success = await userService.RegisterAsync(dto);

    if (!success)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new { message = "Email already registered" });
    }
    else
    {
        context.Response.Headers.Append("X-Confirmation-Sent", "true");
        context.Response.StatusCode = StatusCodes.Status201Created;
        await context.Response.WriteAsJsonAsync(new { message = "Email registered successfully" });
    }
});

app.MapPost("/api/auth/login", async (RegisterDto dto, IUserService userService, HttpContext context) =>
{
    var result = await userService.LoginAsync(dto);

    if (result.Message == "Email unregistered")
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { message = result.Message });
    }
    else if (result.Message == "Wrong password")
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { message = result.Message });
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsJsonAsync(new { result.AuthToken, result.RefreshToken });
    }
});

app.Run();

public partial class Program { }