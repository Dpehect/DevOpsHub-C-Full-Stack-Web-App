using DevOpsHub.Application.Incidents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController, Route("api/incidents"), Authorize]
public sealed class IncidentsController(IIncidentService service) : ControllerBase
{
    [HttpGet("dashboard/{workspaceId:guid}")] public async Task<ActionResult<IncidentDashboardDto>> Dashboard(Guid workspaceId,CancellationToken ct)=>Ok(await service.GetDashboardAsync(workspaceId,ct));
    [HttpPost] public async Task<ActionResult<IncidentDto>> Create(CreateIncidentRequest request,CancellationToken ct)=>Ok(await service.CreateAsync(request,ct));
    [HttpPost("{incidentId:guid}/updates")] public async Task<ActionResult<IncidentDto>> Update(Guid incidentId,AddIncidentUpdateRequest request,CancellationToken ct){var result=await service.AddUpdateAsync(incidentId,request,ct); return result is null?NotFound():Ok(result);}
    [HttpPost("services")] public async Task<ActionResult<ServiceDto>> CreateService(CreateServiceRequest request,CancellationToken ct)=>Ok(await service.CreateServiceAsync(request,ct));
}
