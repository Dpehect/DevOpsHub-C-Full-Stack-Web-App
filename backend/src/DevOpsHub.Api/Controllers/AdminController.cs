using DevOpsHub.Application.Admin;
using DevOpsHub.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminController(IAdminService service) : ControllerBase
{
    [HttpGet("overview")] public async Task<IActionResult> Overview(CancellationToken ct) => Ok(await service.GetOverviewAsync(ct));
    [HttpGet("users")] public async Task<IActionResult> Users([FromQuery]string? search,CancellationToken ct)=>Ok(await service.GetUsersAsync(search,ct));
    [HttpPut("users/{id:guid}/role")] public async Task<IActionResult> Role(Guid id,UpdateUserRoleRequest request,CancellationToken ct){var result=await service.SetUserRoleAsync(id,request.Role,ct);return result is null?BadRequest():Ok(result);}
    [HttpPut("users/{id:guid}/status")] public async Task<IActionResult> Status(Guid id,UpdateUserStatusRequest request,CancellationToken ct){var result=await service.SetUserStatusAsync(id,request.IsActive,ct);return result is null?NotFound():Ok(result);}
    [HttpGet("workspaces")] public async Task<IActionResult> Workspaces(CancellationToken ct)=>Ok(await service.GetWorkspacesAsync(ct));
    [HttpGet("feature-flags")] public async Task<IActionResult> Flags(CancellationToken ct)=>Ok(await service.GetFeatureFlagsAsync(ct));
    [HttpPut("feature-flags/{id:guid}")] public async Task<IActionResult> Flag(Guid id,UpdateFeatureFlagRequest request,CancellationToken ct){var result=await service.SetFeatureFlagAsync(id,request.IsEnabled,ct);return result is null?NotFound():Ok(result);}
    [HttpGet("settings")] public async Task<IActionResult> Settings(CancellationToken ct)=>Ok(await service.GetSettingsAsync(ct));
    [HttpPut("settings/{id:guid}")] public async Task<IActionResult> Setting(Guid id,UpdateSystemSettingRequest request,CancellationToken ct){var result=await service.SetSettingAsync(id,request.Value,ct);return result is null?NotFound():Ok(result);}
    [HttpGet("health")] public async Task<IActionResult> Health(CancellationToken ct)=>Ok(await service.GetHealthAsync(ct));
}
