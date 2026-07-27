namespace DevOpsHub.Domain.Projects;

public sealed class Epic : Entity
{
    private Epic() { }
    public Epic(Guid projectId, string title, string color) { ProjectId = projectId; Title = title.Trim(); Color = color; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#7c3aed";
    public string? Description { get; private set; }
    public ICollection<WorkItem> WorkItems { get; private set; } = new List<WorkItem>();
    public void Update(string title, string? description, string color) { Title = title.Trim(); Description = description?.Trim(); Color = color; Touch(); }
}
