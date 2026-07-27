using DevOpsHub.Domain.Users;

namespace DevOpsHub.Domain.Workspaces;

public sealed class Workspace : Entity
{
    private Workspace() { }
    public Workspace(string name, string slug, Guid ownerId)
    {
        Name = name.Trim(); Slug = slug.Trim().ToLowerInvariant(); OwnerId = ownerId;
    }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid OwnerId { get; private set; }
    public AppUser Owner { get; private set; } = null!;
    public ICollection<WorkspaceMember> Members { get; private set; } = new List<WorkspaceMember>();
    public ICollection<WorkspaceInvitation> Invitations { get; private set; } = new List<WorkspaceInvitation>();
    public void Update(string name, string? description) { Name = name.Trim(); Description = description?.Trim(); Touch(); }
}
