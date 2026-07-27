using DevOpsHub.Domain.Users;
using DevOpsHub.Domain.Workspaces;
using DevOpsHub.Domain.Documentation;
using DevOpsHub.Domain.Administration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync())
        {
            var admin = new AppUser("admin@devopshub.local", "Demo Admin", string.Empty, Roles.Admin);
            typeof(AppUser).GetProperty(nameof(AppUser.PasswordHash))!.SetValue(admin, hasher.HashPassword(admin, "Admin123!"));
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }

        if (!await db.FeatureFlags.AnyAsync())
        {
            db.FeatureFlags.AddRange(
                new FeatureFlag("pipeline.manual-approval", "Require manual approval before production deployments", true),
                new FeatureFlag("incidents.public-status", "Expose sanitized incident updates on the status page", false),
                new FeatureFlag("wiki.ai-search", "Experimental semantic documentation search", false),
                new FeatureFlag("analytics.health-score", "Display composite engineering health score", true));
        }
        if (!await db.SystemSettings.AnyAsync())
        {
            db.SystemSettings.AddRange(
                new SystemSetting("platform.name", "DevOpsHub", "General"),
                new SystemSetting("retention.logs.days", "30", "Observability"),
                new SystemSetting("security.session.minutes", "60", "Security"),
                new SystemSetting("notifications.digest.enabled", "true", "Notifications"));
            await db.SaveChangesAsync();
        }

        var demoAdmin = await db.Users.FirstAsync();
        var workspace = await db.Workspaces.FirstOrDefaultAsync();
        if (workspace is null)
        {
            workspace = new Workspace("DevOpsHub Engineering", "devopshub-engineering", demoAdmin.Id);
            workspace.Update("DevOpsHub Engineering", "Product engineering workspace");
            db.Workspaces.Add(workspace);
            await db.SaveChangesAsync();
        }
        if (!await db.WikiSpaces.AnyAsync())
        {
            var space = new WikiSpace(workspace.Id, "Engineering Handbook", "engineering-handbook", "Runbooks, architecture decisions and team standards.");
            db.WikiSpaces.Add(space);
            await db.SaveChangesAsync();
            db.WikiDocuments.AddRange(
                new WikiDocument(space.Id, "Production Deployment Runbook", "production-deployment-runbook", "# Production Deployment Runbook\n\n## Pre-flight\n- Confirm pipeline is green\n- Review database migrations\n- Announce deployment window\n\n## Rollback\n1. Stop traffic promotion\n2. Redeploy the previous stable artifact\n3. Validate health checks", "Runbooks", demoAdmin.Id),
                new WikiDocument(space.Id, "API Architecture", "api-architecture", "# API Architecture\n\nDevOpsHub follows Clean Architecture with Domain, Application, Infrastructure and API boundaries.\n\n## Rules\n- Domain never references infrastructure\n- Controllers remain thin\n- Business logic lives behind interfaces", "Architecture", demoAdmin.Id),
                new WikiDocument(space.Id, "Incident Communication Standard", "incident-communication-standard", "# Incident Communication\n\nEvery incident update must state impact, current status, next action and expected update time.", "Operations", demoAdmin.Id));
            await db.SaveChangesAsync();
        }
    }
}
