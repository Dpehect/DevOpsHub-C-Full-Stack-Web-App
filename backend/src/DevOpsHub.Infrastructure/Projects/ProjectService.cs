using DevOpsHub.Application.Projects;
using DevOpsHub.Domain.Projects;
using DevOpsHub.Domain.Workspaces;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Projects;

public sealed class ProjectService(AppDbContext db) : IProjectService
{
    private async Task<WorkspaceRole?> RoleForProject(Guid projectId, Guid userId, CancellationToken ct) =>
        await db.Projects.Where(p => p.Id == projectId).Select(p => p.Workspace.Members.Where(m => m.UserId == userId).Select(m => (WorkspaceRole?)m.Role).FirstOrDefault()).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ProjectResponse>> GetByWorkspaceAsync(Guid workspaceId, Guid userId, CancellationToken ct) =>
        await db.Projects.Where(p => p.WorkspaceId == workspaceId && p.Workspace.Members.Any(m => m.UserId == userId))
            .OrderBy(p => p.Name).Select(p => new ProjectResponse(p.Id,p.WorkspaceId,p.Name,p.Key,p.Description,p.Status,p.WorkItems.Count(i=>i.Status!=WorkItemStatus.Done),p.WorkItems.Count(i=>i.Status==WorkItemStatus.Done))).ToListAsync(ct);

    public async Task<ProjectBoardResponse?> GetBoardAsync(Guid projectId, Guid userId, CancellationToken ct)
    {
        var project = await db.Projects.AsNoTracking().Where(p => p.Id == projectId && p.Workspace.Members.Any(m => m.UserId == userId))
            .Select(p => new ProjectResponse(p.Id,p.WorkspaceId,p.Name,p.Key,p.Description,p.Status,p.WorkItems.Count(i=>i.Status!=WorkItemStatus.Done),p.WorkItems.Count(i=>i.Status==WorkItemStatus.Done))).FirstOrDefaultAsync(ct);
        if (project is null) return null;
        var sprints = await db.Sprints.Where(x=>x.ProjectId==projectId).OrderByDescending(x=>x.StartDate).Select(x=>new SprintResponse(x.Id,x.Name,x.Goal,x.StartDate,x.EndDate,x.Status)).ToListAsync(ct);
        var epics = await db.Epics.Where(x=>x.ProjectId==projectId).Select(x=>new EpicResponse(x.Id,x.Title,x.Description,x.Color,x.WorkItems.Count)).ToListAsync(ct);
        var items = await db.WorkItems.Where(x=>x.ProjectId==projectId).OrderBy(x=>x.Position).Select(x=>new WorkItemResponse(x.Id,project.Key+"-"+x.Number,x.Title,x.Description,x.Type,x.Status,x.Priority,x.StoryPoints,x.AssigneeId,x.AssigneeId==null?null:db.Users.Where(u=>u.Id==x.AssigneeId).Select(u=>u.DisplayName).FirstOrDefault(),x.SprintId,x.EpicId,x.ParentId,x.DueDate,x.Position)).ToListAsync(ct);
        return new(project,sprints,epics,items);
    }

    public async Task<ProjectResponse?> CreateAsync(Guid workspaceId, Guid userId, CreateProjectRequest r, CancellationToken ct)
    {
        var allowed = await db.WorkspaceMembers.AnyAsync(m=>m.WorkspaceId==workspaceId && m.UserId==userId && m.Role>=WorkspaceRole.Manager,ct);
        if(!allowed || await db.Projects.AnyAsync(p=>p.WorkspaceId==workspaceId && p.Key==r.Key.ToUpper(),ct)) return null;
        var p = new Project(workspaceId,r.Name,r.Key,r.Description,userId); db.Projects.Add(p); await db.SaveChangesAsync(ct);
        return new(p.Id,p.WorkspaceId,p.Name,p.Key,p.Description,p.Status,0,0);
    }
    public async Task<SprintResponse?> CreateSprintAsync(Guid projectId, Guid userId, CreateSprintRequest r, CancellationToken ct)
    { if((await RoleForProject(projectId,userId,ct))<WorkspaceRole.Manager) return null; var x=new Sprint(projectId,r.Name,r.StartDate,r.EndDate,r.Goal); db.Sprints.Add(x); await db.SaveChangesAsync(ct); return new(x.Id,x.Name,x.Goal,x.StartDate,x.EndDate,x.Status); }
    public async Task<EpicResponse?> CreateEpicAsync(Guid projectId, Guid userId, CreateEpicRequest r, CancellationToken ct)
    { if((await RoleForProject(projectId,userId,ct))<WorkspaceRole.Member) return null; var x=new Epic(projectId,r.Title,r.Color); x.Update(r.Title,r.Description,r.Color); db.Epics.Add(x); await db.SaveChangesAsync(ct); return new(x.Id,x.Title,x.Description,x.Color,0); }
    public async Task<WorkItemResponse?> CreateWorkItemAsync(Guid projectId, Guid userId, CreateWorkItemRequest r, CancellationToken ct)
    {
        if((await RoleForProject(projectId,userId,ct))<WorkspaceRole.Member) return null;
        var project=await db.Projects.FindAsync([projectId],ct); if(project is null)return null;
        var number=(await db.WorkItems.Where(i=>i.ProjectId==projectId).MaxAsync(i=>(int?)i.Number,ct)??0)+1;
        var x=new WorkItem(projectId,number,r.Title,r.Type,r.Priority,userId); x.Update(r.Title,r.Description,r.Priority,r.StoryPoints,r.AssigneeId,r.DueDate,r.EpicId); x.Move(r.SprintId is null?WorkItemStatus.Backlog:WorkItemStatus.Todo,await db.WorkItems.CountAsync(i=>i.ProjectId==projectId,ct),r.SprintId); db.WorkItems.Add(x); await db.SaveChangesAsync(ct);
        return new(x.Id,$"{project.Key}-{x.Number}",x.Title,x.Description,x.Type,x.Status,x.Priority,x.StoryPoints,x.AssigneeId,null,x.SprintId,x.EpicId,x.ParentId,x.DueDate,x.Position);
    }
    public async Task<bool> UpdateWorkItemAsync(Guid itemId, Guid userId, UpdateWorkItemRequest r, CancellationToken ct)
    { var x=await db.WorkItems.FindAsync([itemId],ct); if(x is null || (await RoleForProject(x.ProjectId,userId,ct))<WorkspaceRole.Member)return false; x.Update(r.Title,r.Description,r.Priority,r.StoryPoints,r.AssigneeId,r.DueDate,r.EpicId); await db.SaveChangesAsync(ct); return true; }
    public async Task<bool> MoveWorkItemAsync(Guid itemId, Guid userId, MoveWorkItemRequest r, CancellationToken ct)
    { var x=await db.WorkItems.FindAsync([itemId],ct); if(x is null || (await RoleForProject(x.ProjectId,userId,ct))<WorkspaceRole.Member)return false; x.Move(r.Status,r.Position,r.SprintId); await db.SaveChangesAsync(ct); return true; }
    public async Task<bool> DeleteWorkItemAsync(Guid itemId, Guid userId, CancellationToken ct)
    { var x=await db.WorkItems.FindAsync([itemId],ct); if(x is null || (await RoleForProject(x.ProjectId,userId,ct))<WorkspaceRole.Manager)return false; db.WorkItems.Remove(x); await db.SaveChangesAsync(ct); return true; }
}
