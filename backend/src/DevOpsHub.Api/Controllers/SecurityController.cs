using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController]
[Route("api/security")]
public sealed class SecurityController(IWebHostEnvironment environment) : ControllerBase
{
    [Authorize]
    [HttpGet("session")]
    public IActionResult Session() => Ok(new
    {
        authenticated = User.Identity?.IsAuthenticated == true,
        user = User.Identity?.Name,
        roles = User.Claims.Where(x => x.Type.EndsWith("/role") || x.Type == "role").Select(x => x.Value).Distinct(),
        environment = environment.EnvironmentName
    });
}
