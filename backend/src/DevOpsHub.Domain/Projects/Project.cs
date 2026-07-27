using DevOpsHub.Domain.Workspaces;

namespace DevOpsHub.Domain.Projects;

public sealed class Project : Entity
{
    private Project() { }
    public Project(Guid workspaceId, string name, string key, string? description, Guid createdByUserId)
    {
        WorkspaceId = workspaceId;
        Name = name.Trim();
        Key = key.Trim().ToUpperInvariant();
        Description = description?.Trim();
        CreatedByUserId = createdByUserId;
    }

    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; } = ProjectStatus.Active;
    public Guid CreatedByUserId { get; private set; }
    public ICollection<Sprint> Sprints { get; private set; } = new List<Sprint>();
    public ICollection<Epic> Epics { get; private set; } = new List<Epic>();
    public ICollection<WorkItem> WorkItems { get; private set; } = new List<WorkItem>();

    public void Update(string name, string? description, ProjectStatus status)
    { Name = name.Trim(); Description = description?.Trim(); Status = status; Touch(); }
}

public enum ProjectStatus { Active = 0, OnHold = 1, Completed = 2, Archived = 3 }
