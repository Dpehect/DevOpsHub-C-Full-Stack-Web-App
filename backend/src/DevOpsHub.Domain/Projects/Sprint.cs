namespace DevOpsHub.Domain.Projects;

public sealed class Sprint : Entity
{
    private Sprint() { }
    public Sprint(Guid projectId, string name, DateOnly startDate, DateOnly endDate, string? goal)
    {
        if (endDate < startDate) throw new ArgumentException("Sprint end date cannot precede start date.");
        ProjectId = projectId; Name = name.Trim(); StartDate = startDate; EndDate = endDate; Goal = goal?.Trim();
    }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Goal { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public SprintStatus Status { get; private set; } = SprintStatus.Planned;
    public ICollection<WorkItem> WorkItems { get; private set; } = new List<WorkItem>();
    public void Start() { Status = SprintStatus.Active; Touch(); }
    public void Complete() { Status = SprintStatus.Completed; Touch(); }
}
public enum SprintStatus { Planned = 0, Active = 1, Completed = 2 }
