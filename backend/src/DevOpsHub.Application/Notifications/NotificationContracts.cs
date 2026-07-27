using DevOpsHub.Domain.Notifications;

namespace DevOpsHub.Application.Notifications;

public sealed record NotificationDto(Guid Id, Guid UserId, Guid? WorkspaceId, NotificationType Type, string Title, string Message, string? ActionUrl, string? Source, bool IsRead, DateTime CreatedAtUtc, DateTime? ReadAtUtc);
public sealed record NotificationPageDto(IReadOnlyList<NotificationDto> Items, int UnreadCount, int TotalCount);
public sealed record CreateNotificationRequest(Guid UserId, Guid? WorkspaceId, NotificationType Type, string Title, string Message, string? ActionUrl, string? Source);

public interface INotificationService
{
    Task<NotificationPageDto> GetAsync(Guid userId, bool unreadOnly, int take, CancellationToken ct);
    Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken ct);
    Task<bool> MarkReadAsync(Guid id, Guid userId, CancellationToken ct);
    Task<int> MarkAllReadAsync(Guid userId, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct);
}
