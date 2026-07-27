using DevOpsHub.Application.Documentation;
using DevOpsHub.Domain.Documentation;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Documentation;

public sealed class DocumentationService(AppDbContext db) : IDocumentationService
{
    public async Task<IReadOnlyList<WikiSpaceDto>> GetSpacesAsync(Guid workspaceId, CancellationToken ct) =>
        await db.WikiSpaces.AsNoTracking().Where(x => x.WorkspaceId == workspaceId)
            .Select(x => new WikiSpaceDto(x.Id, x.Name, x.Slug, x.Description, x.Documents.Count)).ToListAsync(ct);

    public async Task<IReadOnlyList<WikiDocumentListDto>> SearchAsync(Guid workspaceId, string? query, string? category, bool favoritesOnly, CancellationToken ct)
    {
        var q = db.WikiDocuments.AsNoTracking().Where(x => x.WikiSpace.WorkspaceId == workspaceId);
        if (!string.IsNullOrWhiteSpace(query)) { var term = query.Trim(); q = q.Where(x => x.Title.Contains(term) || x.Content.Contains(term)); }
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(x => x.Category == category);
        if (favoritesOnly) q = q.Where(x => x.IsFavorite);
        return await q.OrderByDescending(x => x.UpdatedAtUtc).Select(x => new WikiDocumentListDto(x.Id, x.Title, x.Slug, x.Category, x.Status.ToString(), x.IsFavorite, x.UpdatedAtUtc)).ToListAsync(ct);
    }

    public async Task<WikiDocumentDto?> GetDocumentAsync(Guid id, CancellationToken ct) => Map(await db.WikiDocuments.AsNoTracking().Include(x => x.Revisions).FirstOrDefaultAsync(x => x.Id == id, ct));

    public async Task<WikiDocumentDto> CreateAsync(CreateWikiDocumentRequest request, Guid userId, CancellationToken ct)
    {
        var slug = request.Title.Trim().ToLowerInvariant().Replace(' ', '-');
        var doc = new WikiDocument(request.WikiSpaceId, request.Title.Trim(), slug, request.Content, request.Category.Trim(), userId);
        db.WikiDocuments.Add(doc); await db.SaveChangesAsync(ct); return Map(doc)!;
    }

    public async Task<WikiDocumentDto?> UpdateAsync(Guid id, UpdateWikiDocumentRequest request, Guid userId, CancellationToken ct)
    {
        var doc = await db.WikiDocuments.Include(x => x.Revisions).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (doc is null) return null; doc.Update(request.Title.Trim(), request.Content, request.Category.Trim(), userId); await db.SaveChangesAsync(ct); return Map(doc);
    }

    public async Task<bool> ToggleFavoriteAsync(Guid id, CancellationToken ct)
    { var doc = await db.WikiDocuments.FirstOrDefaultAsync(x => x.Id == id, ct); if (doc is null) return false; doc.ToggleFavorite(); await db.SaveChangesAsync(ct); return true; }

    private static WikiDocumentDto? Map(WikiDocument? x) => x is null ? null : new(x.Id, x.WikiSpaceId, x.Title, x.Slug, x.Content, x.Category, x.Status.ToString(), x.IsFavorite, x.UpdatedAtUtc, x.Revisions.OrderByDescending(r => r.CreatedAtUtc).Select(r => new WikiRevisionDto(r.Id, r.Title, r.Category, r.EditorId, r.CreatedAtUtc)).ToList());
}
