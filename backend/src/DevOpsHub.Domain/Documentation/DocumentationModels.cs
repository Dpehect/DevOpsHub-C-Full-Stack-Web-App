namespace DevOpsHub.Domain.Documentation;

public enum DocumentStatus { Draft, Published, Archived }

public sealed class WikiSpace : Entity
{
    private WikiSpace() { }
    public WikiSpace(Guid workspaceId, string name, string slug, string description)
    { WorkspaceId = workspaceId; Name = name; Slug = slug; Description = description; }
    public Guid WorkspaceId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ICollection<WikiDocument> Documents { get; private set; } = new List<WikiDocument>();
}

public sealed class WikiDocument : Entity
{
    private WikiDocument() { }
    public WikiDocument(Guid wikiSpaceId, string title, string slug, string content, string category, Guid authorId)
    { WikiSpaceId = wikiSpaceId; Title = title; Slug = slug; Content = content; Category = category; AuthorId = authorId; Status = DocumentStatus.Published; UpdatedAtUtc = DateTime.UtcNow; }
    public Guid WikiSpaceId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public Guid AuthorId { get; private set; }
    public DocumentStatus Status { get; private set; }
    public bool IsFavorite { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public WikiSpace WikiSpace { get; private set; } = null!;
    public ICollection<WikiRevision> Revisions { get; private set; } = new List<WikiRevision>();
    public void Update(string title, string content, string category, Guid editorId)
    {
        Revisions.Add(new WikiRevision(Id, Title, Content, Category, editorId));
        Title = title; Content = content; Category = category; UpdatedAtUtc = DateTime.UtcNow;
    }
    public void ToggleFavorite() => IsFavorite = !IsFavorite;
}

public sealed class WikiRevision : Entity
{
    private WikiRevision() { }
    public WikiRevision(Guid documentId, string title, string content, string category, Guid editorId)
    { DocumentId = documentId; Title = title; Content = content; Category = category; EditorId = editorId; }
    public Guid DocumentId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public Guid EditorId { get; private set; }
    public WikiDocument Document { get; private set; } = null!;
}
