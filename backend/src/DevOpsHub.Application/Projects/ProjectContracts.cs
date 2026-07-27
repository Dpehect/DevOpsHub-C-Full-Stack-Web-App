using DevOpsHub.Domain.Projects;

namespace DevOpsHub.Application.Projects;

public sealed record CreateProjectRequest(string Name, string Key, string? Description);
public sealed record UpdateProjectRequest(string Name, string? Description, ProjectStatus Status);
public sealed record CreateSprintRequest(string Name, DateOnly StartDate, DateOnly EndDate, string? Goal);
public sealed record CreateEpicRequest(string Title, string? Description, string Color);
public sealed record CreateWorkItemRequest(string Title, string? Description, WorkItemType Type, WorkItemPriority Priority, int StoryPoints, Guid? AssigneeId, Guid? SprintId, Guid? EpicId, Guid? ParentId, DateOnly? DueDate);
public sealed record UpdateWorkItemRequest(string Title, string? Description, WorkItemPriority Priority, int StoryPoints, Guid? AssigneeId, Guid? EpicId, DateOnly? DueDate);
public sealed record MoveWorkItemRequest(WorkItemStatus Status, int Position, Guid? SprintId);
public sealed record ProjectResponse(Guid Id, Guid WorkspaceId, string Name, string Key, string? Description, ProjectStatus Status, int OpenItems, int CompletedItems);
public sealed record SprintResponse(Guid Id, string Name, string? Goal, DateOnly StartDate, DateOnly EndDate, SprintStatus Status);
public sealed record EpicResponse(Guid Id, string Title, string? Description, string Color, int ItemCount);
public sealed record WorkItemResponse(Guid Id, string Key, string Title, string? Description, WorkItemType Type, WorkItemStatus Status, WorkItemPriority Priority, int StoryPoints, Guid? AssigneeId, string? AssigneeName, Guid? SprintId, Guid? EpicId, Guid? ParentId, DateOnly? DueDate, int Position);
public sealed record ProjectBoardResponse(ProjectResponse Project, IReadOnlyList<SprintResponse> Sprints, IReadOnlyList<EpicResponse> Epics, IReadOnlyList<WorkItemResponse> Items);
