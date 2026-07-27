namespace DevOpsHub.Application.Workspaces;

public interface IWorkspaceService
{
    Task<IReadOnlyList<WorkspaceSummary>> GetMineAsync(Guid userId, CancellationToken ct);
    Task<WorkspaceDetails?> GetAsync(Guid workspaceId, Guid userId, CancellationToken ct);
    Task<WorkspaceSummary> CreateAsync(Guid userId, CreateWorkspaceRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(Guid workspaceId, Guid userId, UpdateWorkspaceRequest request, CancellationToken ct);
    Task<WorkspaceInvitationResponse?> InviteAsync(Guid workspaceId, Guid userId, InviteMemberRequest request, CancellationToken ct);
    Task<bool> ChangeRoleAsync(Guid workspaceId, Guid userId, Guid memberId, ChangeMemberRoleRequest request, CancellationToken ct);
    Task<bool> RemoveMemberAsync(Guid workspaceId, Guid userId, Guid memberId, CancellationToken ct);
}
