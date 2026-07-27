using DevOpsHub.Domain.Users;

namespace DevOpsHub.Domain.Workspaces;

public sealed class WorkspaceMember : Entity
{
    private WorkspaceMember() { }
    public WorkspaceMember(Guid workspaceId, Guid userId, WorkspaceRole role)
    { WorkspaceId = workspaceId; UserId = userId; Role = role; JoinedAt = DateTimeOffset.UtcNow; }
    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    public WorkspaceRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public void ChangeRole(WorkspaceRole role) { Role = role; Touch(); }
}

public enum WorkspaceRole { Viewer = 0, Member = 1, Manager = 2, Admin = 3, Owner = 4 }
