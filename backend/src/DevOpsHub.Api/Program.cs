using DevOpsHub.Api.Extensions;
using DevOpsHub.Api.Filters;
using DevOpsHub.Api.Hubs;
using DevOpsHub.Api.Middleware;
using DevOpsHub.Api.OpenApi;
using DevOpsHub.Application;
using DevOpsHub.Infrastructure;
using DevOpsHub.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

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
builder.Services.AddValidatorsFromAssemblyContaining<
    DevOpsHub.Application.Auth.LoginRequestValidator>();

builder.Services.AddProductionRateLimiting();
builder.Services.AddProductionHealthChecks();

builder.Services.AddScoped<FluentValidationFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<FluentValidationFilter>();
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
});

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

var origins = builder.Configuration
    .GetSection("Cors:Origins")
    .Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(origins)
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
