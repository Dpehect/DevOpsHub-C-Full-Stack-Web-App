using DevOpsHub.Application.Repositories;
using DevOpsHub.Domain.Repositories;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Repositories;

public sealed class RepositoryService(AppDbContext db) : IRepositoryService
{
    private Task<bool> Member(Guid workspaceId,Guid userId,CancellationToken ct)=>db.WorkspaceMembers.AnyAsync(x=>x.WorkspaceId==workspaceId&&x.UserId==userId,ct);
    public async Task<IReadOnlyList<RepositorySummary>> GetByWorkspaceAsync(Guid workspaceId,Guid userId,CancellationToken ct)
    {
        if(!await Member(workspaceId,userId,ct)) return [];
        return await db.Repositories.Where(x=>x.WorkspaceId==workspaceId).Select(x=>new RepositorySummary(x.Id,x.Name,x.Description,x.DefaultBranch,x.IsPrivate,x.Branches.Count,x.PullRequests.Count(p=>p.Status==PullRequestStatus.Open||p.Status==PullRequestStatus.Draft),x.CreatedAt)).ToListAsync(ct);
    }
    public async Task<RepositoryDetails?> GetAsync(Guid repositoryId,Guid userId,CancellationToken ct)
    {
        var r=await db.Repositories.Include(x=>x.Branches).Include(x=>x.Commits).Include(x=>x.PullRequests).SingleOrDefaultAsync(x=>x.Id==repositoryId,ct);
        if(r is null||!await Member(r.WorkspaceId,userId,ct)) return null;
        var summary=new RepositorySummary(r.Id,r.Name,r.Description,r.DefaultBranch,r.IsPrivate,r.Branches.Count,r.PullRequests.Count(x=>x.Status is PullRequestStatus.Open or PullRequestStatus.Draft),r.CreatedAt);
        return new(summary,r.Branches.OrderByDescending(x=>x.Name==r.DefaultBranch).ThenBy(x=>x.Name).Select(x=>new BranchDto(x.Id,x.Name,x.IsProtected,x.UpdatedAt)).ToList(),r.Commits.OrderByDescending(x=>x.CommittedAt).Take(30).Select(x=>new CommitDto(x.Id,x.Sha,x.Message,x.AuthorName,x.BranchName,x.CommittedAt,x.Additions,x.Deletions)).ToList(),r.PullRequests.OrderByDescending(x=>x.Number).Select(Map).ToList());
    }
    public async Task<RepositorySummary?> CreateAsync(Guid workspaceId,Guid userId,CreateRepositoryRequest request,CancellationToken ct)
    {
        if(!await Member(workspaceId,userId,ct)) return null;
        var r=new Repository(workspaceId,request.Name,request.Description,request.DefaultBranch);db.Repositories.Add(r);db.Branches.Add(new Branch(r.Id,request.DefaultBranch,true));await db.SaveChangesAsync(ct);return new(r.Id,r.Name,r.Description,r.DefaultBranch,r.IsPrivate,1,0,r.CreatedAt);
    }
    public async Task<BranchDto?> CreateBranchAsync(Guid repositoryId,Guid userId,CreateBranchRequest request,CancellationToken ct)
    {
        var r=await db.Repositories.FindAsync([repositoryId],ct);if(r is null||!await Member(r.WorkspaceId,userId,ct))return null;var b=new Branch(repositoryId,request.Name,request.IsProtected);db.Branches.Add(b);await db.SaveChangesAsync(ct);return new(b.Id,b.Name,b.IsProtected,b.UpdatedAt);
    }
    public async Task<CommitDto?> CreateCommitAsync(Guid repositoryId,Guid userId,CreateCommitRequest request,CancellationToken ct)
    {
        var r=await db.Repositories.FindAsync([repositoryId],ct);if(r is null||!await Member(r.WorkspaceId,userId,ct))return null;var c=new Commit(repositoryId,Guid.NewGuid().ToString("N")[..7],request.Message,request.AuthorName,request.AuthorEmail,request.BranchName,DateTime.UtcNow);c.SetDiff(request.Additions,request.Deletions);db.Commits.Add(c);await db.SaveChangesAsync(ct);return new(c.Id,c.Sha,c.Message,c.AuthorName,c.BranchName,c.CommittedAt,c.Additions,c.Deletions);
    }
    public async Task<PullRequestDto?> CreatePullRequestAsync(Guid repositoryId,Guid userId,CreatePullRequestRequest request,CancellationToken ct)
    {
        var r=await db.Repositories.FindAsync([repositoryId],ct);if(r is null||!await Member(r.WorkspaceId,userId,ct))return null;var n=(await db.PullRequests.Where(x=>x.RepositoryId==repositoryId).MaxAsync(x=>(int?)x.Number,ct)??0)+1;var p=new PullRequest(repositoryId,n,request.Title,request.Description,request.SourceBranch,request.TargetBranch,request.AuthorName);p.SetStats(request.ChangedFiles,request.Additions,request.Deletions);db.PullRequests.Add(p);await db.SaveChangesAsync(ct);return Map(p);
    }
    public async Task<bool> ReviewAsync(Guid pullRequestId,Guid userId,ReviewPullRequestRequest request,CancellationToken ct)
    {
        var p=await db.PullRequests.Include(x=>x.Repository).SingleOrDefaultAsync(x=>x.Id==pullRequestId,ct);if(p is null||!await Member(p.Repository.WorkspaceId,userId,ct))return false;if(request.State==ReviewState.Approved)p.Approve();else if(request.State==ReviewState.ChangesRequested)p.RequestChanges();await db.SaveChangesAsync(ct);return true;
    }
    public async Task<bool> MergeAsync(Guid pullRequestId,Guid userId,CancellationToken ct)
    {
        var p=await db.PullRequests.Include(x=>x.Repository).SingleOrDefaultAsync(x=>x.Id==pullRequestId,ct);if(p is null||!await Member(p.Repository.WorkspaceId,userId,ct))return false;p.Merge();await db.SaveChangesAsync(ct);return true;
    }

    public async Task<IReadOnlyList<RepositoryTreeNode>?> GetTreeAsync(Guid repositoryId,Guid userId,string branch,CancellationToken ct)
    {
        var r=await db.Repositories.FindAsync([repositoryId],ct);if(r is null||!await Member(r.WorkspaceId,userId,ct))return null;
        return DemoTree();
    }
    public async Task<RepositoryFileDto?> GetFileAsync(Guid repositoryId,Guid userId,string branch,string path,CancellationToken ct)
    {
        var r=await db.Repositories.FindAsync([repositoryId],ct);if(r is null||!await Member(r.WorkspaceId,userId,ct))return null;
        var normalized=path.Replace('\\','/').TrimStart('/');
        var content=normalized.EndsWith("Program.cs",StringComparison.OrdinalIgnoreCase)
            ? "using DevOpsHub.Application;\nusing DevOpsHub.Infrastructure;\n\nvar builder = WebApplication.CreateBuilder(args);\nbuilder.Services.AddApplication();\nbuilder.Services.AddInfrastructure(builder.Configuration);\nbuilder.Services.AddControllers();\n\nvar app = builder.Build();\napp.UseAuthentication();\napp.UseAuthorization();\napp.MapControllers();\napp.Run();"
            : normalized.EndsWith("README.md",StringComparison.OrdinalIgnoreCase)
                ? "# DevOpsHub\n\nZero-cost engineering operations platform built with ASP.NET Core and React."
                : "// Repository content preview\n// This endpoint is intentionally provider-free and reads project-owned demo content.";
        var language=normalized.EndsWith(".cs")?"csharp":normalized.EndsWith(".tsx")?"tsx":normalized.EndsWith(".md")?"markdown":"text";
        return new RepositoryFileDto(normalized,language,content,System.Text.Encoding.UTF8.GetByteCount(content),"a8d31f2","feat: repository code browser",DateTime.UtcNow);
    }
    public async Task<FileDiffDto?> GetDiffAsync(Guid repositoryId,Guid userId,string from,string to,string path,CancellationToken ct)
    {
        var r=await db.Repositories.FindAsync([repositoryId],ct);if(r is null||!await Member(r.WorkspaceId,userId,ct))return null;
        var lines=new List<DiffLineDto>{new("context",1,1,"using DevOpsHub.Application;"),new("deletion",3,null,"builder.Services.AddEndpointsApiExplorer();"),new("addition",null,3,"builder.Services.AddProblemDetails();"),new("addition",null,4,"builder.Services.AddHealthChecks();"),new("context",4,5,"builder.Services.AddControllers();"),new("deletion",9,null,"app.MapGet(\"/health\", () => \"ok\");"),new("addition",null,10,"app.MapHealthChecks(\"/health\");")};
        return new FileDiffDto(path,path,"csharp",3,2,lines);
    }
    private static IReadOnlyList<RepositoryTreeNode> DemoTree()=>new List<RepositoryTreeNode>{
        new("backend","backend","folder",null,null,new List<RepositoryTreeNode>{new("src","backend/src","folder",null,null,new List<RepositoryTreeNode>{new("DevOpsHub.Api","backend/src/DevOpsHub.Api","folder",null,null,new List<RepositoryTreeNode>{new("Program.cs","backend/src/DevOpsHub.Api/Program.cs","file","csharp",2841,null),new("RepositoriesController.cs","backend/src/DevOpsHub.Api/Controllers/RepositoriesController.cs","file","csharp",2310,null)})})}),
        new("frontend","frontend","folder",null,null,new List<RepositoryTreeNode>{new("src","frontend/src","folder",null,null,new List<RepositoryTreeNode>{new("RepositoryPage.tsx","frontend/src/pages/RepositoryPage.tsx","file","tsx",12842,null),new("styles.css","frontend/src/styles.css","file","css",17342,null)})}),
        new("README.md","README.md","file","markdown",1412,null)
    };
    private static PullRequestDto Map(PullRequest x)=>new(x.Id,x.Number,x.Title,x.Description,x.SourceBranch,x.TargetBranch,x.AuthorName,x.Status,x.ReviewState,x.ChangedFiles,x.Additions,x.Deletions,x.CreatedAt);
}
