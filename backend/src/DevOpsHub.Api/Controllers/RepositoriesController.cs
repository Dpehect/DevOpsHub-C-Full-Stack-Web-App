using System.Security.Claims;
using DevOpsHub.Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController,Authorize,Route("api")]
public sealed class RepositoriesController(IRepositoryService service):ControllerBase
{
    private Guid UserId=>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet("workspaces/{workspaceId:guid}/repositories")] public async Task<IActionResult> List(Guid workspaceId,CancellationToken ct)=>Ok(await service.GetByWorkspaceAsync(workspaceId,UserId,ct));
    [HttpPost("workspaces/{workspaceId:guid}/repositories")] public async Task<IActionResult> Create(Guid workspaceId,CreateRepositoryRequest request,CancellationToken ct){var x=await service.CreateAsync(workspaceId,UserId,request,ct);return x is null?Forbid():Ok(x);}
    [HttpGet("repositories/{id:guid}")] public async Task<IActionResult> Get(Guid id,CancellationToken ct){var x=await service.GetAsync(id,UserId,ct);return x is null?NotFound():Ok(x);}
    [HttpPost("repositories/{id:guid}/branches")] public async Task<IActionResult> Branch(Guid id,CreateBranchRequest request,CancellationToken ct){var x=await service.CreateBranchAsync(id,UserId,request,ct);return x is null?Forbid():Ok(x);}
    [HttpPost("repositories/{id:guid}/commits")] public async Task<IActionResult> Commit(Guid id,CreateCommitRequest request,CancellationToken ct){var x=await service.CreateCommitAsync(id,UserId,request,ct);return x is null?Forbid():Ok(x);}
    [HttpPost("repositories/{id:guid}/pull-requests")] public async Task<IActionResult> PullRequest(Guid id,CreatePullRequestRequest request,CancellationToken ct){var x=await service.CreatePullRequestAsync(id,UserId,request,ct);return x is null?Forbid():Ok(x);}
    [HttpPatch("pull-requests/{id:guid}/review")] public async Task<IActionResult> Review(Guid id,ReviewPullRequestRequest request,CancellationToken ct)=>await service.ReviewAsync(id,UserId,request,ct)?NoContent():NotFound();
    [HttpGet("repositories/{id:guid}/tree")] public async Task<IActionResult> Tree(Guid id,[FromQuery]string branch="main",CancellationToken ct=default){var x=await service.GetTreeAsync(id,UserId,branch,ct);return x is null?NotFound():Ok(x);}
    [HttpGet("repositories/{id:guid}/file")] public async Task<IActionResult> File(Guid id,[FromQuery]string path,[FromQuery]string branch="main",CancellationToken ct=default){var x=await service.GetFileAsync(id,UserId,branch,path,ct);return x is null?NotFound():Ok(x);}
    [HttpGet("repositories/{id:guid}/diff")] public async Task<IActionResult> Diff(Guid id,[FromQuery]string from,[FromQuery]string to,[FromQuery]string path,CancellationToken ct){var x=await service.GetDiffAsync(id,UserId,from,to,path,ct);return x is null?NotFound():Ok(x);}
    [HttpPost("pull-requests/{id:guid}/merge")] public async Task<IActionResult> Merge(Guid id,CancellationToken ct)=>await service.MergeAsync(id,UserId,ct)?NoContent():NotFound();
}
