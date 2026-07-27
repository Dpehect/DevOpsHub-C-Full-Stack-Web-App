using DevOpsHub.Domain.Repositories;

namespace DevOpsHub.Application.Repositories;

public sealed record RepositorySummary(Guid Id,string Name,string Description,string DefaultBranch,bool IsPrivate,int BranchCount,int OpenPullRequests,DateTime UpdatedAt);
public sealed record BranchDto(Guid Id,string Name,bool IsProtected,DateTime UpdatedAt);
public sealed record CommitDto(Guid Id,string Sha,string Message,string AuthorName,string BranchName,DateTime CommittedAt,int Additions,int Deletions);
public sealed record PullRequestDto(Guid Id,int Number,string Title,string Description,string SourceBranch,string TargetBranch,string AuthorName,PullRequestStatus Status,ReviewState ReviewState,int ChangedFiles,int Additions,int Deletions,DateTime CreatedAt);
public sealed record RepositoryDetails(RepositorySummary Repository,IReadOnlyList<BranchDto> Branches,IReadOnlyList<CommitDto> Commits,IReadOnlyList<PullRequestDto> PullRequests);
public sealed record CreateRepositoryRequest(string Name,string Description,string DefaultBranch="main");
public sealed record CreateBranchRequest(string Name,bool IsProtected=false);
public sealed record CreateCommitRequest(string Message,string AuthorName,string AuthorEmail,string BranchName,int Additions,int Deletions);
public sealed record CreatePullRequestRequest(string Title,string Description,string SourceBranch,string TargetBranch,string AuthorName,int ChangedFiles,int Additions,int Deletions,bool IsDraft=false);
public sealed record ReviewPullRequestRequest(ReviewState State);
public sealed record RepositoryTreeNode(string Name,string Path,string Type,string? Language,long? Size,IReadOnlyList<RepositoryTreeNode>? Children);
public sealed record RepositoryFileDto(string Path,string Language,string Content,long Size,string LastCommitSha,string LastCommitMessage,DateTime UpdatedAt);
public sealed record DiffLineDto(string Type,int? OldLine,int? NewLine,string Content);
public sealed record FileDiffDto(string Path,string OldPath,string Language,int Additions,int Deletions,IReadOnlyList<DiffLineDto> Lines);

public interface IRepositoryService
{
    Task<IReadOnlyList<RepositorySummary>> GetByWorkspaceAsync(Guid workspaceId,Guid userId,CancellationToken ct);
    Task<RepositoryDetails?> GetAsync(Guid repositoryId,Guid userId,CancellationToken ct);
    Task<RepositorySummary?> CreateAsync(Guid workspaceId,Guid userId,CreateRepositoryRequest request,CancellationToken ct);
    Task<BranchDto?> CreateBranchAsync(Guid repositoryId,Guid userId,CreateBranchRequest request,CancellationToken ct);
    Task<CommitDto?> CreateCommitAsync(Guid repositoryId,Guid userId,CreateCommitRequest request,CancellationToken ct);
    Task<PullRequestDto?> CreatePullRequestAsync(Guid repositoryId,Guid userId,CreatePullRequestRequest request,CancellationToken ct);
    Task<bool> ReviewAsync(Guid pullRequestId,Guid userId,ReviewPullRequestRequest request,CancellationToken ct);
    Task<bool> MergeAsync(Guid pullRequestId,Guid userId,CancellationToken ct);
    Task<IReadOnlyList<RepositoryTreeNode>?> GetTreeAsync(Guid repositoryId,Guid userId,string branch,CancellationToken ct);
    Task<RepositoryFileDto?> GetFileAsync(Guid repositoryId,Guid userId,string branch,string path,CancellationToken ct);
    Task<FileDiffDto?> GetDiffAsync(Guid repositoryId,Guid userId,string from,string to,string path,CancellationToken ct);
}
