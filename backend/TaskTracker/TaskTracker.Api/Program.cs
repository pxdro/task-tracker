using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Context;
using TaskTracker.Infrastructure.Services;

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
builder.Services.AddAuthorization();                // Auth
builder.Services.AddControllers();                  // Controllers
builder.Services.AddEndpointsApiExplorer();         // Swagger
builder.Services.AddSwaggerGen();                   // Swagger
builder.Services.AddAutoMapper(typeof(Program));    // AutoMapper
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

app.Run();

public partial class Program { }