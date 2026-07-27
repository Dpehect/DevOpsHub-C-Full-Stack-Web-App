using System.Diagnostics;
using System.Security.Claims;
using DevOpsHub.Domain.Observability;
using DevOpsHub.Infrastructure.Persistence;

namespace DevOpsHub.Api.Middleware;

public sealed class RequestObservabilityMiddleware(RequestDelegate next, ILogger<RequestObservabilityMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var sw = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;
        Exception? failure = null;
        try { await next(context); }
        catch (Exception ex) { failure = ex; throw; }
        finally
        {
            sw.Stop();
            var status = failure is null ? context.Response.StatusCode : 500;
            var level = failure is not null || status >= 500 ? LogLevelType.Error : status >= 400 ? LogLevelType.Warning : LogLevelType.Information;
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = context.User.FindFirstValue(ClaimTypes.Email);
            db.SystemLogs.Add(new SystemLog
            {
                Level = level, Category = "HTTP", Message = $"{context.Request.Method} {context.Request.Path} completed with {status}",
                Exception = failure?.ToString(), RequestId = requestId, UserId = userId, Path = context.Request.Path,
                Method = context.Request.Method, StatusCode = status, DurationMs = sw.ElapsedMilliseconds
            });
            if (context.Request.Method is "POST" or "PUT" or "PATCH" or "DELETE")
            {
                db.AuditEntries.Add(new AuditEntry
                {
                    Action = context.Request.Method, EntityType = ResolveEntity(context.Request.Path), EntityId = ResolveId(context.Request.Path),
                    UserId = userId, UserEmail = email, IpAddress = context.Connection.RemoteIpAddress?.ToString(), RequestId = requestId,
                    Succeeded = failure is null && status < 400
                });
            }
            try { await db.SaveChangesAsync(); }
            catch (Exception ex) { logger.LogError(ex, "Observability persistence failed for {RequestId}", requestId); }
        }
    }

    private static string ResolveEntity(PathString path) => path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault() ?? "system";
    private static string? ResolveId(PathString path) => path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => Guid.TryParse(x, out _));
}
