using DevOpsHub.Api.Extensions;
using DevOpsHub.Api.Filters;
using DevOpsHub.Api.Hubs;
using DevOpsHub.Api.Middleware;
using DevOpsHub.Application;
using DevOpsHub.Infrastructure;
using DevOpsHub.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "DevOpsHub.Api"));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<DevOpsHub.Application.Auth.LoginRequestValidator>();
builder.Services.AddProductionHealthChecks();

builder.Services.AddScoped<FluentValidationFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<FluentValidationFilter>();
});

builder.Services.AddOpenApi();
builder.Services.AddOutputCache();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 32 * 1024;
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.AddPolicy("api", context =>
    {
        var subject = context.User.FindFirst("sub")?.Value;
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetSlidingWindowLimiter(
            subject ?? ip,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

var origins = builder.Configuration
    .GetSection("Cors:Origins")
    .Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.Services.InitializeDatabaseAsync();
}
else
{
    await app.Services.WaitForDatabaseAsync();
}

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnostics, context) =>
    {
        diagnostics.Set("TraceId", context.TraceIdentifier);
        diagnostics.Set("UserId", context.User.FindFirst("sub")?.Value);
        diagnostics.Set("RequestPath", context.Request.Path);
    };
});

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRateLimiter();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

app.MapControllers().RequireRateLimiting("api");
app.MapHub<NotificationHub>("/hubs/notifications")
    .RequireAuthorization()
    .RequireRateLimiting("api");
app.MapProductionHealthChecks();

app.Run();

public partial class Program;
