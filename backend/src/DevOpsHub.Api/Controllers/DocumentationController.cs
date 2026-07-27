using System.Security.Claims;
using DevOpsHub.Application.Documentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController, Route("api/documentation"), Authorize]
public sealed class DocumentationController(IDocumentationService service) : ControllerBase
{
    [HttpGet("spaces/{workspaceId:guid}")] public async Task<IActionResult> Spaces(Guid workspaceId, CancellationToken ct) => Ok(await service.GetSpacesAsync(workspaceId, ct));
    [HttpGet("documents")] public async Task<IActionResult> Search([FromQuery] Guid workspaceId, [FromQuery] string? query, [FromQuery] string? category, [FromQuery] bool favoritesOnly, CancellationToken ct) => Ok(await service.SearchAsync(workspaceId, query, category, favoritesOnly, ct));
    [HttpGet("documents/{id:guid}")] public async Task<IActionResult> Get(Guid id, CancellationToken ct) { var x = await service.GetDocumentAsync(id, ct); return x is null ? NotFound() : Ok(x); }
    [HttpPost("documents")] public async Task<IActionResult> Create(CreateWikiDocumentRequest request, CancellationToken ct) { var x = await service.CreateAsync(request, UserId(), ct); return CreatedAtAction(nameof(Get), new { id = x.Id }, x); }
    [HttpPut("documents/{id:guid}")] public async Task<IActionResult> Update(Guid id, UpdateWikiDocumentRequest request, CancellationToken ct) { var x = await service.UpdateAsync(id, request, UserId(), ct); return x is null ? NotFound() : Ok(x); }
    [HttpPost("documents/{id:guid}/favorite")] public async Task<IActionResult> Favorite(Guid id, CancellationToken ct) => await service.ToggleFavoriteAsync(id, ct) ? NoContent() : NotFound();
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
