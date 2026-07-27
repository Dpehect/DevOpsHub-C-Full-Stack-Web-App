using System.Text;
using DevOpsHub.Application.Admin;
using DevOpsHub.Application.Analytics;
using DevOpsHub.Application.Auth;
using DevOpsHub.Application.Documentation;
using DevOpsHub.Application.Incidents;
using DevOpsHub.Application.Notifications;
using DevOpsHub.Application.Observability;
using DevOpsHub.Application.Pipelines;
using DevOpsHub.Application.Projects;
using DevOpsHub.Application.Repositories;
using DevOpsHub.Application.Search;
using DevOpsHub.Application.Workflows;
using DevOpsHub.Application.Workspaces;
using DevOpsHub.Domain.Users;
using DevOpsHub.Infrastructure.Admin;
using DevOpsHub.Infrastructure.Analytics;
using DevOpsHub.Infrastructure.Auth;
using DevOpsHub.Infrastructure.Documentation;
using DevOpsHub.Infrastructure.Incidents;
using DevOpsHub.Infrastructure.Notifications;
using DevOpsHub.Infrastructure.Observability;
using DevOpsHub.Infrastructure.Persistence;
using DevOpsHub.Infrastructure.Pipelines;
using DevOpsHub.Infrastructure.Projects;
using DevOpsHub.Infrastructure.Repositories;
using DevOpsHub.Infrastructure.Search;
using DevOpsHub.Infrastructure.Workflows;
using DevOpsHub.Infrastructure.Workspaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DevOpsHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.SigningKey)
                    && options.SigningKey.Length >= 32,
                "JWT signing key must contain at least 32 characters.")
            .Validate(options =>
                    !options.SigningKey.Contains("DEV_ONLY", StringComparison.OrdinalIgnoreCase)
                    && !options.SigningKey.Contains("change-before-production", StringComparison.OrdinalIgnoreCase),
                "A development/demo JWT key cannot be used.")
            .ValidateOnStart();

        var provider = configuration["Database:Provider"]?.ToLowerInvariant() ?? "sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing.");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider == "postgresql")
            {
                options.UseNpgsql(connectionString, sql =>
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = true;
                options.SaveToken = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(15),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];

                        if (!string.IsNullOrWhiteSpace(token)
                            && context.HttpContext.Request.Path
                                .StartsWithSegments("/hubs/notifications"))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
            options.AddPolicy("WorkspaceMember", policy =>
                policy.RequireAuthenticatedUser());
        });

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
        services.AddScoped<IWorkflowService, WorkflowService>();

        return services;
    }
}
