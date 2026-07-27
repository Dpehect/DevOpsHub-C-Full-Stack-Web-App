namespace DevOpsHub.Application.Search;

public sealed record GlobalSearchRequest(string Query, string[]? Types, string[]? Statuses, Guid? WorkspaceId, int Page = 1, int PageSize = 20, string Sort = "relevance");
public sealed record SearchResultItem(Guid Id, string Type, string Title, string Subtitle, string Status, string? Reference, DateTime UpdatedAt, double Score, IReadOnlyDictionary<string,string> Metadata);
public sealed record SearchFacet(string Value, int Count);
public sealed record GlobalSearchResponse(string Query, int Page, int PageSize, int Total, IReadOnlyList<SearchResultItem> Items, IReadOnlyList<SearchFacet> Types, IReadOnlyList<SearchFacet> Statuses, long ElapsedMilliseconds);

public interface IGlobalSearchService
{
    Task<GlobalSearchResponse> SearchAsync(Guid userId, GlobalSearchRequest request, CancellationToken cancellationToken);
}
