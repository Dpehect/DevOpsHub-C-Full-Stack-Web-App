using DevOpsHub.Domain.Repositories;
using Xunit;

namespace DevOpsHub.Tests;

public sealed class RepositoryDomainTests
{
    [Fact]
    public void Approved_pull_request_can_be_merged()
    {
        var pr = new PullRequest(Guid.NewGuid(), 1, "Repository center", "Details", "feature/repositories", "main", "Demo User");
        pr.Approve();
        pr.Merge();
        Assert.Equal(PullRequestStatus.Merged, pr.Status);
        Assert.Equal(ReviewState.Approved, pr.ReviewState);
    }
}
