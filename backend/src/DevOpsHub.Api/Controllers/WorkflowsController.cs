using DevOpsHub.Application.Workflows; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
namespace DevOpsHub.Api.Controllers;
[ApiController,Route("api/v1/workflows"),Authorize] public sealed class WorkflowsController(IWorkflowService service):ControllerBase { [HttpGet] public IActionResult Get()=>Ok(service.GetAll()); [HttpPost] public IActionResult Create(CreateApprovalRequest request)=>Ok(service.Create(request)); [HttpPost("{id:guid}/decision")] public IActionResult Decide(Guid id,[FromQuery]string decision)=>Ok(service.Decide(id,decision)); }
