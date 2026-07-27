using DevOpsHub.Application.Observability;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController, Route("api/observability"), Authorize]
public sealed class ObservabilityController(IObservabilityService service) : ControllerBase
{
    [HttpGet("logs")]
    public Task<PagedResult<LogDto>> Logs([FromQuery] string? level, [FromQuery] string? search, [FromQuery] string? category, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => service.GetLogsAsync(new(level, search, category, page, pageSize), ct);

    [HttpGet("audit")]
    public Task<PagedResult<AuditDto>> Audit([FromQuery] string? action, [FromQuery] string? entityType, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        => service.GetAuditAsync(new(action, entityType, search, page, pageSize), ct);

    [HttpGet("stats")]
    public Task<ObservabilityStats> Stats(CancellationToken ct) => service.GetStatsAsync(ct);

    [HttpDelete("logs"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Clear(CancellationToken ct) { await service.ClearLogsAsync(ct); return NoContent(); }
}
