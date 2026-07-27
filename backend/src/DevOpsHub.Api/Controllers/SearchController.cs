using System.Security.Claims;
using DevOpsHub.Application.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController, Authorize, Route("api/search")]
public sealed class SearchController(IGlobalSearchService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GlobalSearchResponse>> Search([FromQuery]string q="", [FromQuery]string[]? type=null, [FromQuery]string[]? status=null, [FromQuery]Guid? workspaceId=null, [FromQuery]int page=1, [FromQuery]int pageSize=20, [FromQuery]string sort="relevance", CancellationToken ct=default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await service.SearchAsync(userId,new(q,type,status,workspaceId,page,pageSize,sort),ct));
    }
}
