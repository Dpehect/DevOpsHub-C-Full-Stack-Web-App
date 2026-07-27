namespace DevOpsHub.Application.Documentation;

public sealed record WikiSpaceDto(Guid Id, string Name, string Slug, string Description, int DocumentCount);
public sealed record WikiDocumentListDto(Guid Id, string Title, string Slug, string Category, string Status, bool IsFavorite, DateTime UpdatedAtUtc);
public sealed record WikiRevisionDto(Guid Id, string Title, string Category, Guid EditorId, DateTime CreatedAtUtc);
public sealed record WikiDocumentDto(Guid Id, Guid WikiSpaceId, string Title, string Slug, string Content, string Category, string Status, bool IsFavorite, DateTime UpdatedAtUtc, IReadOnlyList<WikiRevisionDto> Revisions);
public sealed record CreateWikiDocumentRequest(Guid WikiSpaceId, string Title, string Content, string Category);
public sealed record UpdateWikiDocumentRequest(string Title, string Content, string Category);
public interface IDocumentationService
{
    Task<IReadOnlyList<WikiSpaceDto>> GetSpacesAsync(Guid workspaceId, CancellationToken ct);
    Task<IReadOnlyList<WikiDocumentListDto>> SearchAsync(Guid workspaceId, string? query, string? category, bool favoritesOnly, CancellationToken ct);
    Task<WikiDocumentDto?> GetDocumentAsync(Guid id, CancellationToken ct);
    Task<WikiDocumentDto> CreateAsync(CreateWikiDocumentRequest request, Guid userId, CancellationToken ct);
    Task<WikiDocumentDto?> UpdateAsync(Guid id, UpdateWikiDocumentRequest request, Guid userId, CancellationToken ct);
    Task<bool> ToggleFavoriteAsync(Guid id, CancellationToken ct);
}
