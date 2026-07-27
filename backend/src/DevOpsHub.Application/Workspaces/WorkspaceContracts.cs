using DevOpsHub.Domain.Workspaces;

namespace DevOpsHub.Application.Workspaces;

public sealed record CreateWorkspaceRequest(string Name, string Slug, string? Description);
public sealed record UpdateWorkspaceRequest(string Name, string? Description);
public sealed record InviteMemberRequest(string Email, WorkspaceRole Role);
public sealed record ChangeMemberRoleRequest(WorkspaceRole Role);
public sealed record WorkspaceSummary(Guid Id, string Name, string Slug, string? Description, WorkspaceRole CurrentUserRole, int MemberCount);
public sealed record WorkspaceMemberResponse(Guid Id, Guid UserId, string DisplayName, string Email, WorkspaceRole Role, DateTimeOffset JoinedAt);
public sealed record WorkspaceInvitationResponse(Guid Id, string Email, WorkspaceRole Role, DateTimeOffset ExpiresAt, bool IsActive);
public sealed record WorkspaceDetails(WorkspaceSummary Workspace, IReadOnlyList<WorkspaceMemberResponse> Members, IReadOnlyList<WorkspaceInvitationResponse> Invitations);
