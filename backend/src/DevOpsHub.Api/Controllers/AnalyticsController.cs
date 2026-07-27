using DevOpsHub.Application.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public sealed class AnalyticsController(IAnalyticsService service) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<AnalyticsOverviewDto>> GetOverview(CancellationToken ct) => Ok(await service.GetOverviewAsync(ct));
}
