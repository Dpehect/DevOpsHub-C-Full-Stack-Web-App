using System.Security.Claims;
using DevOpsHub.Application.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController]
[Route("api/workspaces")]
[Authorize]
public sealed class WorkspacesController(IWorkspaceService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkspaceSummary>>> Mine(CancellationToken ct) => Ok(await service.GetMineAsync(UserId, ct));

    [HttpGet("{workspaceId:guid}")]
    public async Task<ActionResult<WorkspaceDetails>> Get(Guid workspaceId, CancellationToken ct)
    { var result = await service.GetAsync(workspaceId, UserId, ct); return result is null ? NotFound() : Ok(result); }

    [HttpPost]
    public async Task<ActionResult<WorkspaceSummary>> Create(CreateWorkspaceRequest request, CancellationToken ct)
    { var created = await service.CreateAsync(UserId, request, ct); return CreatedAtAction(nameof(Get), new { workspaceId = created.Id }, created); }

    [HttpPut("{workspaceId:guid}")]
    public async Task<IActionResult> Update(Guid workspaceId, UpdateWorkspaceRequest request, CancellationToken ct) =>
        await service.UpdateAsync(workspaceId, UserId, request, ct) ? NoContent() : Forbid();

    [HttpPost("{workspaceId:guid}/invitations")]
    public async Task<ActionResult<WorkspaceInvitationResponse>> Invite(Guid workspaceId, InviteMemberRequest request, CancellationToken ct)
    { var invitation = await service.InviteAsync(workspaceId, UserId, request, ct); return invitation is null ? Forbid() : Ok(invitation); }

    [HttpPut("{workspaceId:guid}/members/{memberId:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid workspaceId, Guid memberId, ChangeMemberRoleRequest request, CancellationToken ct) =>
        await service.ChangeRoleAsync(workspaceId, UserId, memberId, request, ct) ? NoContent() : Forbid();

    [HttpDelete("{workspaceId:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> Remove(Guid workspaceId, Guid memberId, CancellationToken ct) =>
        await service.RemoveMemberAsync(workspaceId, UserId, memberId, ct) ? NoContent() : Forbid();
}
