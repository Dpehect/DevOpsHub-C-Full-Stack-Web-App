using DevOpsHub.Domain.Observability;

namespace DevOpsHub.Application.Observability;

public sealed record LogQuery(string? Level, string? Search, string? Category, int Page = 1, int PageSize = 25);
public sealed record AuditQuery(string? Action, string? EntityType, string? Search, int Page = 1, int PageSize = 25);
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record LogDto(Guid Id, string Level, string Message, string Category, string? Exception, string? RequestId, string? UserId, string? Path, string? Method, int? StatusCode, long? DurationMs, DateTime CreatedAtUtc);
public sealed record AuditDto(Guid Id, string Action, string EntityType, string? EntityId, string? UserId, string? UserEmail, string? IpAddress, string? RequestId, bool Succeeded, DateTime CreatedAtUtc);
public sealed record ObservabilityStats(int Logs24h, int Errors24h, int Audits24h, double AverageRequestMs, double ErrorRatePercent);

public interface IObservabilityService
{
    Task<PagedResult<LogDto>> GetLogsAsync(LogQuery query, CancellationToken cancellationToken);
    Task<PagedResult<AuditDto>> GetAuditAsync(AuditQuery query, CancellationToken cancellationToken);
    Task<ObservabilityStats> GetStatsAsync(CancellationToken cancellationToken);
    Task ClearLogsAsync(CancellationToken cancellationToken);
}
