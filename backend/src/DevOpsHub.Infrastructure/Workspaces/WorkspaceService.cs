using System.Security.Cryptography;
using DevOpsHub.Application.Workspaces;
using DevOpsHub.Domain.Workspaces;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Workspaces;

public sealed class WorkspaceService(AppDbContext db) : IWorkspaceService
{
    public async Task<IReadOnlyList<WorkspaceSummary>> GetMineAsync(Guid userId, CancellationToken ct) =>
        await db.WorkspaceMembers.AsNoTracking().Where(x => x.UserId == userId)
            .Select(x => new WorkspaceSummary(x.Workspace.Id, x.Workspace.Name, x.Workspace.Slug, x.Workspace.Description, x.Role, x.Workspace.Members.Count))
            .OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<WorkspaceDetails?> GetAsync(Guid workspaceId, Guid userId, CancellationToken ct)
    {
        var membership = await db.WorkspaceMembers.AsNoTracking().FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, ct);
        if (membership is null) return null;
        var workspace = await db.Workspaces.AsNoTracking().FirstAsync(x => x.Id == workspaceId, ct);
        var members = await db.WorkspaceMembers.AsNoTracking().Where(x => x.WorkspaceId == workspaceId)
            .OrderByDescending(x => x.Role).ThenBy(x => x.User.DisplayName)
            .Select(x => new WorkspaceMemberResponse(x.Id, x.UserId, x.User.DisplayName, x.User.Email, x.Role, x.JoinedAt)).ToListAsync(ct);
        var invitations = membership.Role >= WorkspaceRole.Admin
            ? await db.WorkspaceInvitations.AsNoTracking().Where(x => x.WorkspaceId == workspaceId && x.AcceptedAt == null)
                .Select(x => new WorkspaceInvitationResponse(x.Id, x.Email, x.Role, x.ExpiresAt, x.AcceptedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow)).ToListAsync(ct)
            : [];
        return new WorkspaceDetails(new WorkspaceSummary(workspace.Id, workspace.Name, workspace.Slug, workspace.Description, membership.Role, members.Count), members, invitations);
    }

    public async Task<WorkspaceSummary> CreateAsync(Guid userId, CreateWorkspaceRequest request, CancellationToken ct)
    {
        if (await db.Workspaces.AnyAsync(x => x.Slug == request.Slug.ToLower(), ct)) throw new InvalidOperationException("Workspace slug already exists.");
        var workspace = new Workspace(request.Name, request.Slug, userId); workspace.Update(request.Name, request.Description);
        db.Workspaces.Add(workspace); db.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, userId, WorkspaceRole.Owner));
        await db.SaveChangesAsync(ct);
        return new WorkspaceSummary(workspace.Id, workspace.Name, workspace.Slug, workspace.Description, WorkspaceRole.Owner, 1);
    }

    public async Task<bool> UpdateAsync(Guid workspaceId, Guid userId, UpdateWorkspaceRequest request, CancellationToken ct)
    {
        var member = await db.WorkspaceMembers.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, ct);
        if (member is null || member.Role < WorkspaceRole.Admin) return false;
        var workspace = await db.Workspaces.FindAsync([workspaceId], ct); if (workspace is null) return false;
        workspace.Update(request.Name, request.Description); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<WorkspaceInvitationResponse?> InviteAsync(Guid workspaceId, Guid userId, InviteMemberRequest request, CancellationToken ct)
    {
        var actor = await db.WorkspaceMembers.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, ct);
        if (actor is null || actor.Role < WorkspaceRole.Admin || request.Role >= actor.Role) return null;
        var email = request.Email.Trim().ToLowerInvariant();
        var existingUser = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (existingUser is not null)
        {
            if (!await db.WorkspaceMembers.AnyAsync(x => x.WorkspaceId == workspaceId && x.UserId == existingUser.Id, ct))
                db.WorkspaceMembers.Add(new WorkspaceMember(workspaceId, existingUser.Id, request.Role));
            await db.SaveChangesAsync(ct);
            return new WorkspaceInvitationResponse(Guid.Empty, email, request.Role, DateTimeOffset.UtcNow, false);
        }
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var invitation = new WorkspaceInvitation(workspaceId, email, request.Role, userId, token);
        db.WorkspaceInvitations.Add(invitation); await db.SaveChangesAsync(ct);
        return new WorkspaceInvitationResponse(invitation.Id, invitation.Email, invitation.Role, invitation.ExpiresAt, true);
    }

    public async Task<bool> ChangeRoleAsync(Guid workspaceId, Guid userId, Guid memberId, ChangeMemberRoleRequest request, CancellationToken ct)
    {
        var actor = await db.WorkspaceMembers.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, ct);
        var target = await db.WorkspaceMembers.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.Id == memberId, ct);
        if (actor is null || target is null || actor.Role < WorkspaceRole.Admin || actor.Role <= target.Role || request.Role >= actor.Role || target.Role == WorkspaceRole.Owner) return false;
        target.ChangeRole(request.Role); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> RemoveMemberAsync(Guid workspaceId, Guid userId, Guid memberId, CancellationToken ct)
    {
        var actor = await db.WorkspaceMembers.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, ct);
        var target = await db.WorkspaceMembers.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.Id == memberId, ct);
        if (actor is null || target is null || target.Role == WorkspaceRole.Owner || (actor.UserId != target.UserId && (actor.Role < WorkspaceRole.Admin || actor.Role <= target.Role))) return false;
        db.WorkspaceMembers.Remove(target); await db.SaveChangesAsync(ct); return true;
    }
}
