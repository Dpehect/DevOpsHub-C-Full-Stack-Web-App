using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController]
[Route("api/native")]
[Authorize(Roles = "Admin")]
public sealed class NativeDiagnosticsController : ControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType<NativeStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<NativeStatusResponse> GetStatus() =>
        Ok(new NativeStatusResponse(
            Enabled: false,
            SafetyMode: "managed-runtime",
            Message:
                "No native runtime module is loaded. Native safety helpers are build-time only."));
}

public sealed record NativeStatusResponse(
    bool Enabled,
    string SafetyMode,
    string Message);
