using System.Diagnostics;
using DevOpsHub.Application.Search;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Search;

public sealed class GlobalSearchService(AppDbContext db) : IGlobalSearchService
{
    public async Task<GlobalSearchResponse> SearchAsync(Guid userId, GlobalSearchRequest request, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var query = (request.Query ?? string.Empty).Trim();
        var normalized = query.ToLowerInvariant();
        var workspaceIds = await db.WorkspaceMembers.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.WorkspaceId).ToListAsync(ct);
        var items = new List<SearchResultItem>();
        double Score(string value) => string.IsNullOrWhiteSpace(normalized) ? 1 : value.Equals(normalized, StringComparison.OrdinalIgnoreCase) ? 100 : value.StartsWith(normalized, StringComparison.OrdinalIgnoreCase) ? 70 : value.Contains(normalized, StringComparison.OrdinalIgnoreCase) ? 40 : 0;
        bool Matches(string value) => string.IsNullOrWhiteSpace(normalized) || value.Contains(normalized, StringComparison.OrdinalIgnoreCase);

        var projects = await db.Projects.AsNoTracking().Where(x => workspaceIds.Contains(x.WorkspaceId) && (!request.WorkspaceId.HasValue || x.WorkspaceId == request.WorkspaceId)).Take(300).ToListAsync(ct);
        foreach (var x in projects.Where(x => Matches($"{x.Key} {x.Name}"))) items.Add(new(x.Id,"project",x.Name,$"Project {x.Key}","active",x.Key,(x.UpdatedAtUtc ?? x.CreatedAtUtc),Score($"{x.Key} {x.Name}"),new Dictionary<string,string>{{"workspaceId",x.WorkspaceId.ToString()}}));

        var workItems = await db.WorkItems.AsNoTracking().Where(x => projects.Select(p=>p.Id).Contains(x.ProjectId)).Take(700).ToListAsync(ct);
        foreach (var x in workItems.Where(x => Matches($"{x.Title} {x.Number} {x.Description}"))) items.Add(new(x.Id,"work-item",x.Title,$"{x.Type} · #{x.Number}",x.Status.ToString().ToLowerInvariant(),$"#{x.Number}",(x.UpdatedAtUtc ?? x.CreatedAtUtc),Score(x.Title),new Dictionary<string,string>{{"priority",x.Priority.ToString()},{"projectId",x.ProjectId.ToString()}}));

        var repos = await db.Repositories.AsNoTracking().Where(x => workspaceIds.Contains(x.WorkspaceId) && (!request.WorkspaceId.HasValue || x.WorkspaceId == request.WorkspaceId)).Take(300).ToListAsync(ct);
        foreach (var x in repos.Where(x => Matches($"{x.Name} {x.Description}"))) items.Add(new(x.Id,"repository",x.Name,"Source repository","active",x.DefaultBranch,(x.UpdatedAtUtc ?? x.CreatedAtUtc),Score(x.Name),new Dictionary<string,string>{{"defaultBranch",x.DefaultBranch}}));

        var incidents = await db.Incidents.AsNoTracking().Where(x => workspaceIds.Contains(x.WorkspaceId) && (!request.WorkspaceId.HasValue || x.WorkspaceId == request.WorkspaceId)).Take(400).ToListAsync(ct);
        foreach (var x in incidents.Where(x => Matches($"{x.Title} {x.Summary} {x.Number}"))) items.Add(new(x.Id,"incident",x.Title,$"Incident #{x.Number} · {x.Severity}",x.Status.ToString().ToLowerInvariant(),$"INC-{x.Number}",(x.UpdatedAtUtc ?? x.CreatedAtUtc),Score(x.Title),new Dictionary<string,string>{{"severity",x.Severity.ToString()}}));

        var docs = await db.WikiDocuments.AsNoTracking().Include(x=>x.WikiSpace).Where(x => workspaceIds.Contains(x.WikiSpace.WorkspaceId) && (!request.WorkspaceId.HasValue || x.WikiSpace.WorkspaceId == request.WorkspaceId)).Take(400).ToListAsync(ct);
        foreach (var x in docs.Where(x => Matches($"{x.Title} {x.Slug} {x.Content}"))) items.Add(new(x.Id,"document",x.Title,$"Documentation · {x.Slug}",x.Status.ToString().ToLowerInvariant(),x.Slug,x.UpdatedAtUtc,Score(x.Title),new Dictionary<string,string>{{"slug",x.Slug}}));

        if (request.Types is { Length: > 0 }) items = items.Where(x => request.Types.Contains(x.Type, StringComparer.OrdinalIgnoreCase)).ToList();
        if (request.Statuses is { Length: > 0 }) items = items.Where(x => request.Statuses.Contains(x.Status, StringComparer.OrdinalIgnoreCase)).ToList();
        items = request.Sort switch { "updated" => items.OrderByDescending(x=>x.UpdatedAt).ToList(), "title" => items.OrderBy(x=>x.Title).ToList(), _ => items.OrderByDescending(x=>x.Score).ThenByDescending(x=>x.UpdatedAt).ToList() };
        var total = items.Count;
        var typeFacets = items.GroupBy(x=>x.Type).Select(x=>new SearchFacet(x.Key,x.Count())).OrderByDescending(x=>x.Count).ToList();
        var statusFacets = items.GroupBy(x=>x.Status).Select(x=>new SearchFacet(x.Key,x.Count())).OrderByDescending(x=>x.Count).ToList();
        var page = Math.Max(1, request.Page); var size = Math.Clamp(request.PageSize, 5, 100);
        var paged = items.Skip((page-1)*size).Take(size).ToList(); sw.Stop();
        return new(query,page,size,total,paged,typeFacets,statusFacets,sw.ElapsedMilliseconds);
    }
}
