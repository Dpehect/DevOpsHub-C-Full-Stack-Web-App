namespace DevOpsHub.Application.Admin;

public sealed record AdminOverviewDto(
    int TotalUsers,
    int ActiveUsers,
    int Workspaces,
    int OpenIncidents,
    int FailedPipelines,
    int UnreadNotifications,
    IReadOnlyList<AdminActivityDto> RecentActivity);

public sealed record AdminActivityDto(Guid Id, string Action, string EntityType, string? UserEmail, DateTime CreatedAtUtc);
public sealed record AdminUserDto(Guid Id, string Email, string DisplayName, string Role, bool IsActive, DateTime CreatedAtUtc, DateTime? LastLoginAtUtc);
public sealed record AdminWorkspaceDto(Guid Id, string Name, string Slug, int Members, int Projects, DateTime CreatedAtUtc);
public sealed record FeatureFlagDto(Guid Id, string Key, string Description, bool IsEnabled, DateTime UpdatedAtUtc);
public sealed record SystemSettingDto(Guid Id, string Key, string Value, string Category, bool IsSecret, DateTime UpdatedAtUtc);
public sealed record HealthSnapshotDto(string Api, string Database, long DatabaseSizeBytes, int LogsLast24Hours, int ErrorsLast24Hours, DateTime CheckedAtUtc);

public sealed record UpdateUserRoleRequest(string Role);
public sealed record UpdateUserStatusRequest(bool IsActive);
public sealed record UpdateFeatureFlagRequest(bool IsEnabled);
public sealed record UpdateSystemSettingRequest(string Value);

public interface IAdminService
{
    Task<AdminOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(string? search, CancellationToken cancellationToken);
    Task<AdminUserDto?> SetUserRoleAsync(Guid userId, string role, CancellationToken cancellationToken);
    Task<AdminUserDto?> SetUserStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminWorkspaceDto>> GetWorkspacesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<FeatureFlagDto>> GetFeatureFlagsAsync(CancellationToken cancellationToken);
    Task<FeatureFlagDto?> SetFeatureFlagAsync(Guid id, bool isEnabled, CancellationToken cancellationToken);
    Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken cancellationToken);
    Task<SystemSettingDto?> SetSettingAsync(Guid id, string value, CancellationToken cancellationToken);
    Task<HealthSnapshotDto> GetHealthAsync(CancellationToken cancellationToken);
}
