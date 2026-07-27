using DevOpsHub.Application.Admin;
using DevOpsHub.Application.Search;
using DevOpsHub.Infrastructure.Search;
using DevOpsHub.Application.Analytics;
using DevOpsHub.Infrastructure.Admin;
using DevOpsHub.Infrastructure.Analytics;
using DevOpsHub.Application.Workspaces;
using DevOpsHub.Infrastructure.Workspaces;
using DevOpsHub.Application.Projects;
using DevOpsHub.Infrastructure.Projects;
using DevOpsHub.Application.Repositories;
using DevOpsHub.Infrastructure.Repositories;
using DevOpsHub.Application.Pipelines;
using DevOpsHub.Infrastructure.Pipelines;
using DevOpsHub.Application.Incidents;
using DevOpsHub.Application.Notifications;
using DevOpsHub.Infrastructure.Notifications;
using DevOpsHub.Infrastructure.Incidents;
using DevOpsHub.Application.Observability;
using DevOpsHub.Infrastructure.Observability;
using DevOpsHub.Application.Documentation;
using DevOpsHub.Infrastructure.Documentation;
using System.Text;
using DevOpsHub.Application.Auth;
using DevOpsHub.Domain.Users;
using DevOpsHub.Infrastructure.Auth;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevOpsHub.Application.Workflows;
using DevOpsHub.Infrastructure.Workflows;
using Microsoft.IdentityModel.Tokens;

namespace DevOpsHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=devopshub.db"));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                  ?? throw new InvalidOperationException("JWT configuration is missing.");
        if (jwt.SigningKey.Length < 32) throw new InvalidOperationException("JWT signing key must be at least 32 characters.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Query["access_token"];
                    if (!string.IsNullOrWhiteSpace(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs/notifications")) context.Token = token;
                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization();
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IRepositoryService, RepositoryService>();
        services.AddScoped<IPipelineService, PipelineService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IObservabilityService, ObservabilityService>();
        services.AddScoped<IDocumentationService, DocumentationService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IGlobalSearchService, GlobalSearchService>();
        services.AddSingleton<IWorkflowService, WorkflowService>();
        return services;
    }
}
