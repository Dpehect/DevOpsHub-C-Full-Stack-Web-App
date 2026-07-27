using DevOpsHub.Api.Middleware;
using Serilog;
using DevOpsHub.Api.Hubs;
using DevOpsHub.Application;
using DevOpsHub.Infrastructure;
using DevOpsHub.Infrastructure.Persistence;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "DevOpsHub.Api"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 32 * 1024;
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("api", httpContext => RateLimitPartition.GetSlidingWindowLimiter(
        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
        factory: _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(corsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();
await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseResponseCaching();
app.Use(async (context,next)=>{ context.Response.Headers["X-Api-Version"]="1.0"; context.Response.Headers["X-DevOpsHub-Phase"]="25"; await next(); });
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRateLimiter();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RequestObservabilityMiddleware>();
app.MapControllers().RequireRateLimiting("api");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");
app.MapGet("/api/system", () => Results.Ok(new { name = "DevOpsHub API", status = "online", version = "1.0.0" }));
app.Run();

public partial class Program;
