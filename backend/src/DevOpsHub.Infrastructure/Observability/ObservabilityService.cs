using DevOpsHub.Application.Observability;
using DevOpsHub.Domain.Observability;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Observability;

public sealed class ObservabilityService(AppDbContext db) : IObservabilityService
{
    public async Task<PagedResult<LogDto>> GetLogsAsync(LogQuery query, CancellationToken ct)
    {
        var q = db.SystemLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Level) && Enum.TryParse<LogLevelType>(query.Level, true, out var level)) q = q.Where(x => x.Level == level);
        if (!string.IsNullOrWhiteSpace(query.Category)) q = q.Where(x => x.Category.Contains(query.Category));
        if (!string.IsNullOrWhiteSpace(query.Search)) q = q.Where(x => x.Message.Contains(query.Search) || (x.RequestId != null && x.RequestId.Contains(query.Search)) || (x.Path != null && x.Path.Contains(query.Search)));
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.CreatedAtUtc).Skip((Math.Max(query.Page,1)-1)*Math.Clamp(query.PageSize,1,100)).Take(Math.Clamp(query.PageSize,1,100))
            .Select(x => new LogDto(x.Id,x.Level.ToString(),x.Message,x.Category,x.Exception,x.RequestId,x.UserId,x.Path,x.Method,x.StatusCode,x.DurationMs,x.CreatedAtUtc)).ToListAsync(ct);
        return new(items,total,Math.Max(query.Page,1),Math.Clamp(query.PageSize,1,100));
    }

    public async Task<PagedResult<AuditDto>> GetAuditAsync(AuditQuery query, CancellationToken ct)
    {
        var q = db.AuditEntries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Action)) q = q.Where(x => x.Action.Contains(query.Action));
        if (!string.IsNullOrWhiteSpace(query.EntityType)) q = q.Where(x => x.EntityType.Contains(query.EntityType));
        if (!string.IsNullOrWhiteSpace(query.Search)) q = q.Where(x => (x.UserEmail != null && x.UserEmail.Contains(query.Search)) || (x.EntityId != null && x.EntityId.Contains(query.Search)) || (x.RequestId != null && x.RequestId.Contains(query.Search)));
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.CreatedAtUtc).Skip((Math.Max(query.Page,1)-1)*Math.Clamp(query.PageSize,1,100)).Take(Math.Clamp(query.PageSize,1,100))
            .Select(x => new AuditDto(x.Id,x.Action,x.EntityType,x.EntityId,x.UserId,x.UserEmail,x.IpAddress,x.RequestId,x.Succeeded,x.CreatedAtUtc)).ToListAsync(ct);
        return new(items,total,Math.Max(query.Page,1),Math.Clamp(query.PageSize,1,100));
    }

    public async Task<ObservabilityStats> GetStatsAsync(CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddHours(-24);
        var logs = db.SystemLogs.AsNoTracking().Where(x => x.CreatedAtUtc >= since);
        var count = await logs.CountAsync(ct);
        var errors = await logs.CountAsync(x => x.Level == LogLevelType.Error || x.Level == LogLevelType.Critical, ct);
        var audits = await db.AuditEntries.CountAsync(x => x.CreatedAtUtc >= since, ct);
        var requestLogs = logs.Where(x => x.DurationMs != null);
        var average = await requestLogs.AnyAsync(ct) ? await requestLogs.AverageAsync(x => (double)x.DurationMs!, ct) : 0;
        return new(count, errors, audits, Math.Round(average, 1), count == 0 ? 0 : Math.Round(errors * 100d / count, 2));
    }

    public async Task ClearLogsAsync(CancellationToken ct)
    {
        await db.SystemLogs.ExecuteDeleteAsync(ct);
    }
}
