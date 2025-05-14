using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity.Data;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<ITaskService, TaskService>();

// Ensure Enums return names instead of numbers
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/ping", () => "ok");

app.MapPost("/api/auth/register", async (UserDto dto, IUserService userService, HttpContext context) =>
{
    var userResultDto = await userService.RegisterAsync(dto);

    if (userResultDto.StatusCode == 201)
        context.Response.Headers["X-Confirmation-Sent"] = "true";

    return Results.Json(new { message = userResultDto.Message }, statusCode: userResultDto.StatusCode);
});

app.MapPost("/api/auth/login", async (UserDto dto, IUserService userService, HttpContext context) =>
{
    var userResultDto = await userService.LoginAsync(dto);

    return userResultDto.StatusCode switch
    {
        200 => Results.Json(new
        {
            authToken = userResultDto.AuthToken,
            refreshToken = userResultDto.RefreshToken
        }),
        _ => Results.Json(new { message = userResultDto.Message }, statusCode: userResultDto.StatusCode)
    };
});

app.MapGet("/api/tasks", (HttpContext http, string userEmail, string? field, string? value, ITaskService taskService) =>
{
    Task<List<TaskItem>> result;
    if (field == null)
        result = taskService.GetAll(userEmail);
    else
        result = taskService.Where(userEmail, field, value);
    return Results.Json(result);
});


app.MapPost("/api/tasks", (HttpContext http, TaskItem task, ITaskService taskService) =>
{
    var user = http.User.Identity?.Name ?? "user@example.com";
    var response = taskService.Add(user, task);
    return Results.Json(response);
});

app.MapPut("/api/tasks", (HttpContext http, string title, TaskItem task, ITaskService taskService) =>
{
    var user = http.User.Identity?.Name ?? "user@example.com";
    var updated = taskService.Update(user, title, task);
    return updated is null ? Results.NotFound() : Results.Json(updated);
});

app.MapPost("/api/tasks/{title}/complete", (HttpContext http, string title, ITaskService taskService) =>
{
    var user = http.User.Identity?.Name ?? "user@example.com";
    var updated = taskService.Update(user, title, new TaskItem { Title = title,  Description = null, Status = EnumTaskStatus.Active });
    return updated is null ? Results.NotFound() : Results.Json(updated);
});

app.MapDelete("api/tasks/{title}", (HttpContext http, string title, ITaskService taskService) =>
{
    var user = http.User.Identity?.Name ?? "user@example.com";
    var success = taskService.Delete(user, title);
    return success.Result ? Results.NoContent() : Results.NotFound();
});

app.Run();

public partial class Program { }

static class ResultExtensions
{
    public static IResult WithMessage(this IResult result, string message)
        => Results.Json(new { message }, statusCode: ((IStatusCodeHttpResult)result).StatusCode ?? 500);
}