using System.Security.Claims;
using DevOpsHub.Application.Projects;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController, Authorize, Route("api")]
public sealed class ProjectsController(IProjectService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet("workspaces/{workspaceId:guid}/projects")] public async Task<IActionResult> List(Guid workspaceId,CancellationToken ct)=>Ok(await service.GetByWorkspaceAsync(workspaceId,UserId,ct));
    [HttpPost("workspaces/{workspaceId:guid}/projects")] public async Task<IActionResult> Create(Guid workspaceId,CreateProjectRequest request,IValidator<CreateProjectRequest> validator,CancellationToken ct){var v=await validator.ValidateAsync(request,ct);if(!v.IsValid)return ValidationProblem(new ValidationProblemDetails(v.ToDictionary()));var x=await service.CreateAsync(workspaceId,UserId,request,ct);return x is null?Forbid():CreatedAtAction(nameof(Board),new{id=x.Id},x);}
    [HttpGet("projects/{id:guid}/board")] public async Task<IActionResult> Board(Guid id,CancellationToken ct){var x=await service.GetBoardAsync(id,UserId,ct);return x is null?NotFound():Ok(x);}
    [HttpPost("projects/{id:guid}/sprints")] public async Task<IActionResult> Sprint(Guid id,CreateSprintRequest request,CancellationToken ct){var x=await service.CreateSprintAsync(id,UserId,request,ct);return x is null?Forbid():Ok(x);}
    [HttpPost("projects/{id:guid}/epics")] public async Task<IActionResult> Epic(Guid id,CreateEpicRequest request,CancellationToken ct){var x=await service.CreateEpicAsync(id,UserId,request,ct);return x is null?Forbid():Ok(x);}
    [HttpPost("projects/{id:guid}/items")] public async Task<IActionResult> Item(Guid id,CreateWorkItemRequest request,IValidator<CreateWorkItemRequest> validator,CancellationToken ct){var v=await validator.ValidateAsync(request,ct);if(!v.IsValid)return ValidationProblem(new ValidationProblemDetails(v.ToDictionary()));var x=await service.CreateWorkItemAsync(id,UserId,request,ct);return x is null?Forbid():Ok(x);}
    [HttpPut("items/{id:guid}")] public async Task<IActionResult> Update(Guid id,UpdateWorkItemRequest request,CancellationToken ct)=>await service.UpdateWorkItemAsync(id,UserId,request,ct)?NoContent():NotFound();
    [HttpPatch("items/{id:guid}/move")] public async Task<IActionResult> Move(Guid id,MoveWorkItemRequest request,CancellationToken ct)=>await service.MoveWorkItemAsync(id,UserId,request,ct)?NoContent():NotFound();
    [HttpDelete("items/{id:guid}")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct)=>await service.DeleteWorkItemAsync(id,UserId,ct)?NoContent():NotFound();
}
