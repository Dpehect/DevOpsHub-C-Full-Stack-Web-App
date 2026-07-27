namespace DevOpsHub.Domain.Notifications;

public enum NotificationType { Info, Success, Warning, Error, Assignment, Mention, Pipeline, Incident }

public sealed class Notification : Entity
{
    public Guid UserId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public NotificationType Type { get; set; } = NotificationType.Info;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? Source { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
