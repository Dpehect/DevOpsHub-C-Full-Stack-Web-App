using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController]
[Route("api/system")]
[AllowAnonymous]
public sealed class SystemController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<SystemStatusResponse>(StatusCodes.Status200OK)]
    public ActionResult<SystemStatusResponse> GetStatus() =>
        Ok(new SystemStatusResponse(
            "DevOpsHub API",
            "online",
            "1.0.0"));
}

public sealed record SystemStatusResponse(
    string Name,
    string Status,
    string Version);
