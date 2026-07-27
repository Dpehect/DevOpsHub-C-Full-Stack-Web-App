namespace DevOpsHub.Domain.Projects;

public sealed class WorkItem : Entity
{
    private WorkItem() { }
    public WorkItem(Guid projectId, int number, string title, WorkItemType type, WorkItemPriority priority, Guid reporterId)
    { ProjectId = projectId; Number = number; Title = title.Trim(); Type = type; Priority = priority; ReporterId = reporterId; }
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;
    public Guid? SprintId { get; private set; }
    public Sprint? Sprint { get; private set; }
    public Guid? EpicId { get; private set; }
    public Epic? Epic { get; private set; }
    public Guid? ParentId { get; private set; }
    public WorkItem? Parent { get; private set; }
    public ICollection<WorkItem> SubTasks { get; private set; } = new List<WorkItem>();
    public int Number { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public WorkItemType Type { get; private set; }
    public WorkItemStatus Status { get; private set; } = WorkItemStatus.Backlog;
    public WorkItemPriority Priority { get; private set; }
    public int StoryPoints { get; private set; }
    public Guid ReporterId { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public int Position { get; private set; }

    public void Update(string title, string? description, WorkItemPriority priority, int storyPoints, Guid? assigneeId, DateOnly? dueDate, Guid? epicId)
    { Title = title.Trim(); Description = description?.Trim(); Priority = priority; StoryPoints = Math.Clamp(storyPoints, 0, 100); AssigneeId = assigneeId; DueDate = dueDate; EpicId = epicId; Touch(); }
    public void Move(WorkItemStatus status, int position, Guid? sprintId) { Status = status; Position = Math.Max(0, position); SprintId = sprintId; Touch(); }
}
public enum WorkItemType { Task = 0, Story = 1, Bug = 2, SubTask = 3 }
public enum WorkItemStatus { Backlog = 0, Todo = 1, InProgress = 2, InReview = 3, Done = 4 }
public enum WorkItemPriority { Low = 0, Medium = 1, High = 2, Critical = 3 }
