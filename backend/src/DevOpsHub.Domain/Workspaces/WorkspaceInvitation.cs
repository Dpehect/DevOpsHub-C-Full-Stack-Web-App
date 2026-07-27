namespace DevOpsHub.Domain.Workspaces;

public sealed class WorkspaceInvitation : Entity
{
    private WorkspaceInvitation() { }
    public WorkspaceInvitation(Guid workspaceId, string email, WorkspaceRole role, Guid invitedByUserId, string token)
    { WorkspaceId = workspaceId; Email = email.Trim().ToLowerInvariant(); Role = role; InvitedByUserId = invitedByUserId; Token = token; ExpiresAt = DateTimeOffset.UtcNow.AddDays(7); }
    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public string Email { get; private set; } = string.Empty;
    public WorkspaceRole Role { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public bool IsActive => AcceptedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
    public void Accept() { AcceptedAt = DateTimeOffset.UtcNow; Touch(); }
}
