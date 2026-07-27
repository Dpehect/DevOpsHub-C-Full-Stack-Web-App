using System.Security.Claims;
using DevOpsHub.Api.Hubs;
using DevOpsHub.Application.Notifications;
using DevOpsHub.Domain.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DevOpsHub.Api.Controllers;

[ApiController, Route("api/notifications"), Authorize]
public sealed class NotificationsController(INotificationService service, IHubContext<NotificationHub> hub) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet] public async Task<ActionResult<NotificationPageDto>> Get([FromQuery]bool unreadOnly=false,[FromQuery]int take=30,CancellationToken ct=default)=>Ok(await service.GetAsync(UserId,unreadOnly,take,ct));
    [HttpPatch("{id:guid}/read")] public async Task<IActionResult> Read(Guid id,CancellationToken ct){return await service.MarkReadAsync(id,UserId,ct)?NoContent():NotFound();}
    [HttpPost("read-all")] public async Task<IActionResult> ReadAll(CancellationToken ct)=>Ok(new{updated=await service.MarkAllReadAsync(UserId,ct)});
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct){return await service.DeleteAsync(id,UserId,ct)?NoContent():NotFound();}

    [HttpPost("demo")]
    public async Task<ActionResult<NotificationDto>> Demo(CancellationToken ct)
    {
        var created=await service.CreateAsync(new(UserId,null,NotificationType.Success,"Deployment completed","Version 0.9.0 is live in staging.","/pipelines","Deployments"),ct);
        await hub.Clients.Group($"user:{UserId}").SendAsync("notificationReceived",created,ct);
        return Ok(created);
    }
}
