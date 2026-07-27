namespace DevOpsHub.Application.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectResponse>> GetByWorkspaceAsync(Guid workspaceId, Guid userId, CancellationToken ct);
    Task<ProjectBoardResponse?> GetBoardAsync(Guid projectId, Guid userId, CancellationToken ct);
    Task<ProjectResponse?> CreateAsync(Guid workspaceId, Guid userId, CreateProjectRequest request, CancellationToken ct);
    Task<SprintResponse?> CreateSprintAsync(Guid projectId, Guid userId, CreateSprintRequest request, CancellationToken ct);
    Task<EpicResponse?> CreateEpicAsync(Guid projectId, Guid userId, CreateEpicRequest request, CancellationToken ct);
    Task<WorkItemResponse?> CreateWorkItemAsync(Guid projectId, Guid userId, CreateWorkItemRequest request, CancellationToken ct);
    Task<bool> UpdateWorkItemAsync(Guid itemId, Guid userId, UpdateWorkItemRequest request, CancellationToken ct);
    Task<bool> MoveWorkItemAsync(Guid itemId, Guid userId, MoveWorkItemRequest request, CancellationToken ct);
    Task<bool> DeleteWorkItemAsync(Guid itemId, Guid userId, CancellationToken ct);
}
