using DevOpsHub.Application.Admin;
using DevOpsHub.Domain.Users;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Admin;

public sealed class AdminService(AppDbContext db) : IAdminService
{
    public async Task<AdminOverviewDto> GetOverviewAsync(CancellationToken ct)
    {
        var activity = await db.AuditEntries.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Take(8)
            .Select(x => new AdminActivityDto(x.Id, x.Action, x.EntityType, x.UserEmail, x.CreatedAtUtc)).ToListAsync(ct);
        return new AdminOverviewDto(
            await db.Users.CountAsync(ct), await db.Users.CountAsync(x => x.IsActive, ct),
            await db.Workspaces.CountAsync(ct), await db.Incidents.CountAsync(x => x.ResolvedAtUtc == null, ct),
            await db.PipelineRuns.CountAsync(x => x.Status == Domain.Pipelines.PipelineStatus.Failed, ct),
            await db.Notifications.CountAsync(x => !x.IsRead, ct), activity);
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(string? search, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();
            query = query.Where(x => x.Email.Contains(value) || x.DisplayName.ToLower().Contains(value));
        }
        return await query.OrderByDescending(x => x.CreatedAtUtc).Select(MapUser).ToListAsync(ct);
    }

    public async Task<AdminUserDto?> SetUserRoleAsync(Guid userId, string role, CancellationToken ct)
    {
        if (role is not (Roles.Admin or Roles.Owner or Roles.Member)) return null;
        var user = await db.Users.FindAsync([userId], ct); if (user is null) return null;
        user.ChangeRole(role); await db.SaveChangesAsync(ct); return ToUser(user);
    }

    public async Task<AdminUserDto?> SetUserStatusAsync(Guid userId, bool isActive, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([userId], ct); if (user is null) return null;
        if (isActive) user.Activate(); else user.Deactivate(); await db.SaveChangesAsync(ct); return ToUser(user);
    }

    public async Task<IReadOnlyList<AdminWorkspaceDto>> GetWorkspacesAsync(CancellationToken ct) =>
        await db.Workspaces.AsNoTracking().OrderBy(x => x.Name).Select(x => new AdminWorkspaceDto(
            x.Id, x.Name, x.Slug, db.WorkspaceMembers.Count(m => m.WorkspaceId == x.Id),
            db.Projects.Count(p => p.WorkspaceId == x.Id), x.CreatedAtUtc)).ToListAsync(ct);

    public async Task<IReadOnlyList<FeatureFlagDto>> GetFeatureFlagsAsync(CancellationToken ct) =>
        await db.FeatureFlags.AsNoTracking().OrderBy(x => x.Key).Select(x => new FeatureFlagDto(x.Id,x.Key,x.Description,x.IsEnabled,x.UpdatedAtUtc)).ToListAsync(ct);

    public async Task<FeatureFlagDto?> SetFeatureFlagAsync(Guid id, bool enabled, CancellationToken ct)
    { var item=await db.FeatureFlags.FindAsync([id],ct); if(item is null)return null; item.SetEnabled(enabled); await db.SaveChangesAsync(ct); return new(item.Id,item.Key,item.Description,item.IsEnabled,item.UpdatedAtUtc); }

    public async Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken ct) =>
        await db.SystemSettings.AsNoTracking().OrderBy(x=>x.Category).ThenBy(x=>x.Key).Select(x=>new SystemSettingDto(x.Id,x.Key,x.IsSecret?"••••••••":x.Value,x.Category,x.IsSecret,x.UpdatedAtUtc)).ToListAsync(ct);

    public async Task<SystemSettingDto?> SetSettingAsync(Guid id,string value,CancellationToken ct)
    { var item=await db.SystemSettings.FindAsync([id],ct); if(item is null)return null; item.UpdateValue(value); await db.SaveChangesAsync(ct); return new(item.Id,item.Key,item.IsSecret?"••••••••":item.Value,item.Category,item.IsSecret,item.UpdatedAtUtc); }

    public async Task<HealthSnapshotDto> GetHealthAsync(CancellationToken ct)
    {
        var logs=await db.SystemLogs.CountAsync(x=>x.CreatedAtUtc>=DateTime.UtcNow.AddHours(-24),ct);
        var errors=await db.SystemLogs.CountAsync(x=>x.CreatedAtUtc>=DateTime.UtcNow.AddHours(-24) && x.Level==Domain.Observability.LogLevelType.Error,ct);
        var path=db.Database.GetDbConnection().DataSource; var size=File.Exists(path)?new FileInfo(path).Length:0;
        return new("Healthy",await db.Database.CanConnectAsync(ct)?"Connected":"Unavailable",size,logs,errors,DateTime.UtcNow);
    }

    private static readonly System.Linq.Expressions.Expression<Func<AppUser,AdminUserDto>> MapUser = x => new(x.Id,x.Email,x.DisplayName,x.Role,x.IsActive,x.CreatedAtUtc,x.LastLoginAtUtc);
    private static AdminUserDto ToUser(AppUser x)=>new(x.Id,x.Email,x.DisplayName,x.Role,x.IsActive,x.CreatedAtUtc,x.LastLoginAtUtc);
}
