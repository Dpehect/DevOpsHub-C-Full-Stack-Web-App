namespace DevOpsHub.Domain.Repositories;

public enum PullRequestStatus { Open, Draft, Merged, Closed }
public enum ReviewState { Pending, Approved, ChangesRequested }

public sealed class Repository : Entity
{
    private Repository() { }
    public Repository(Guid workspaceId, string name, string description, string defaultBranch = "main")
    { WorkspaceId = workspaceId; Name = name.Trim(); Description = description.Trim(); DefaultBranch = defaultBranch.Trim(); }
    public Guid WorkspaceId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string DefaultBranch { get; private set; } = "main";
    public bool IsPrivate { get; private set; } = true;
    public ICollection<Branch> Branches { get; private set; } = new List<Branch>();
    public ICollection<Commit> Commits { get; private set; } = new List<Commit>();
    public ICollection<PullRequest> PullRequests { get; private set; } = new List<PullRequest>();
}

public sealed class Branch : Entity
{
    private Branch() { }
    public Branch(Guid repositoryId, string name, bool isProtected = false)
    { RepositoryId = repositoryId; Name = name.Trim(); IsProtected = isProtected; }
    public Guid RepositoryId { get; private set; }
    public Repository Repository { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public bool IsProtected { get; private set; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
}

public sealed class Commit : Entity
{
    private Commit() { }
    public Commit(Guid repositoryId, string sha, string message, string authorName, string authorEmail, string branchName, DateTime committedAt)
    { RepositoryId = repositoryId; Sha = sha; Message = message; AuthorName = authorName; AuthorEmail = authorEmail; BranchName = branchName; CommittedAt = committedAt; }
    public Guid RepositoryId { get; private set; }
    public Repository Repository { get; private set; } = null!;
    public string Sha { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;
    public string AuthorEmail { get; private set; } = string.Empty;
    public string BranchName { get; private set; } = string.Empty;
    public DateTime CommittedAt { get; private set; }
    public int Additions { get; private set; }
    public int Deletions { get; private set; }
    public void SetDiff(int additions, int deletions) { Additions = additions; Deletions = deletions; }
}

public sealed class PullRequest : Entity
{
    private PullRequest() { }
    public PullRequest(Guid repositoryId, int number, string title, string description, string sourceBranch, string targetBranch, string authorName)
    { RepositoryId = repositoryId; Number = number; Title = title; Description = description; SourceBranch = sourceBranch; TargetBranch = targetBranch; AuthorName = authorName; }
    public Guid RepositoryId { get; private set; }
    public Repository Repository { get; private set; } = null!;
    public int Number { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SourceBranch { get; private set; } = string.Empty;
    public string TargetBranch { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;
    public PullRequestStatus Status { get; private set; } = PullRequestStatus.Open;
    public ReviewState ReviewState { get; private set; } = ReviewState.Pending;
    public int ChangedFiles { get; private set; }
    public int Additions { get; private set; }
    public int Deletions { get; private set; }
    public void SetStats(int files, int additions, int deletions) { ChangedFiles = files; Additions = additions; Deletions = deletions; }
    public void Approve() => ReviewState = ReviewState.Approved;
    public void RequestChanges() => ReviewState = ReviewState.ChangesRequested;
    public void Merge() { Status = PullRequestStatus.Merged; ReviewState = ReviewState.Approved; }
    public void Close() => Status = PullRequestStatus.Closed;
}
