using System.Security.Claims;
using DevOpsHub.Application.Pipelines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController, Route("api/pipelines"), Authorize]
public sealed class PipelinesController(IPipelineService service) : ControllerBase
{
    [HttpGet("dashboard")] public async Task<ActionResult<PipelineDashboardDto>> Dashboard(CancellationToken ct) => Ok(await service.GetDashboardAsync(ct));
    [HttpGet("runs/{id:guid}")] public async Task<ActionResult<PipelineRunDto>> Run(Guid id, CancellationToken ct) => (await service.GetRunAsync(id,ct)) is { } run ? Ok(run) : NotFound();
    [HttpPost] public async Task<ActionResult<PipelineDefinitionDto>> Create(CreatePipelineRequest request, CancellationToken ct) => Ok(await service.CreateAsync(request,ct));
    [HttpPost("{id:guid}/runs")] public async Task<ActionResult<PipelineRunDto>> Trigger(Guid id, RunPipelineRequest request, CancellationToken ct) => Ok(await service.RunAsync(id,request,User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "Demo User",ct));
    [HttpPost("runs/{id:guid}/retry")] public async Task<ActionResult<PipelineRunDto>> Retry(Guid id,CancellationToken ct) => (await service.RetryAsync(id,User.Identity?.Name ?? "Demo User",ct)) is { } run ? Ok(run) : NotFound();
    [HttpPost("runs/{id:guid}/cancel")] public async Task<IActionResult> Cancel(Guid id,CancellationToken ct) => await service.CancelAsync(id,ct) ? NoContent() : BadRequest();
}
