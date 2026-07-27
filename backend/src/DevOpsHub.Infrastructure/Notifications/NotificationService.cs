using DevOpsHub.Application.Notifications;
using DevOpsHub.Domain.Notifications;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Notifications;

public sealed class NotificationService(AppDbContext db) : INotificationService
{
    public async Task<NotificationPageDto> GetAsync(Guid userId, bool unreadOnly, int take, CancellationToken ct)
    {
        await EnsureSeedAsync(userId, ct);
        var query = db.Notifications.AsNoTracking().Where(x => x.UserId == userId);
        if (unreadOnly) query = query.Where(x => !x.IsRead);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).Take(Math.Clamp(take, 1, 100)).Select(x => Map(x)).ToListAsync(ct);
        var unread = await db.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead, ct);
        return new(items, unread, total);
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationRequest r, CancellationToken ct)
    {
        var entity = new Notification { UserId=r.UserId, WorkspaceId=r.WorkspaceId, Type=r.Type, Title=r.Title.Trim(), Message=r.Message.Trim(), ActionUrl=r.ActionUrl, Source=r.Source };
        db.Notifications.Add(entity); await db.SaveChangesAsync(ct); return Map(entity);
    }

    public async Task<bool> MarkReadAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var item=await db.Notifications.FirstOrDefaultAsync(x=>x.Id==id&&x.UserId==userId,ct); if(item is null)return false;
        item.IsRead=true; item.ReadAtUtc=DateTime.UtcNow; await db.SaveChangesAsync(ct); return true;
    }

    public async Task<int> MarkAllReadAsync(Guid userId, CancellationToken ct)
    {
        var items=await db.Notifications.Where(x=>x.UserId==userId&&!x.IsRead).ToListAsync(ct);
        foreach(var item in items){item.IsRead=true;item.ReadAtUtc=DateTime.UtcNow;} await db.SaveChangesAsync(ct); return items.Count;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct)
    { var item=await db.Notifications.FirstOrDefaultAsync(x=>x.Id==id&&x.UserId==userId,ct); if(item is null)return false; db.Remove(item); await db.SaveChangesAsync(ct); return true; }

    private async Task EnsureSeedAsync(Guid userId,CancellationToken ct)
    {
        if(await db.Notifications.AnyAsync(x=>x.UserId==userId,ct)) return;
        db.Notifications.AddRange(
            new Notification{UserId=userId,Type=NotificationType.Pipeline,Title="Pipeline succeeded",Message="DevOpsHub CI #148 completed successfully.",Source="Pipelines",ActionUrl="/pipelines",CreatedAtUtc=DateTime.UtcNow.AddMinutes(-8)},
            new Notification{UserId=userId,Type=NotificationType.Incident,Title="Incident escalated",Message="API latency incident is approaching its SLA deadline.",Source="Incidents",ActionUrl="/incidents",CreatedAtUtc=DateTime.UtcNow.AddMinutes(-21)},
            new Notification{UserId=userId,Type=NotificationType.Assignment,Title="Work item assigned",Message="You were assigned DEV-142: Add deployment approval gate.",Source="Projects",ActionUrl="/board",CreatedAtUtc=DateTime.UtcNow.AddHours(-2),IsRead=true,ReadAtUtc=DateTime.UtcNow.AddHours(-1)});
        await db.SaveChangesAsync(ct);
    }
    private static NotificationDto Map(Notification x)=>new(x.Id,x.UserId,x.WorkspaceId,x.Type,x.Title,x.Message,x.ActionUrl,x.Source,x.IsRead,x.CreatedAtUtc,x.ReadAtUtc);
}
