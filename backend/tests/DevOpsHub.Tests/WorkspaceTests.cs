using DevOpsHub.Domain.Workspaces;

namespace DevOpsHub.Tests;

public sealed class WorkspaceTests
{
    [Fact]
    public void Workspace_Normalizes_Slug()
    {
        var workspace = new Workspace("Platform Team", "PLATFORM-TEAM", Guid.NewGuid());
        Assert.Equal("platform-team", workspace.Slug);
    }

    [Fact]
    public void Owner_Role_Has_Highest_Authority()
    {
        Assert.True(WorkspaceRole.Owner > WorkspaceRole.Admin);
        Assert.True(WorkspaceRole.Admin > WorkspaceRole.Member);
    }

    [Fact]
    public void Invitation_Is_Active_When_Created()
    {
        var invitation = new WorkspaceInvitation(Guid.NewGuid(), "dev@example.com", WorkspaceRole.Member, Guid.NewGuid(), "token");
        Assert.True(invitation.IsActive);
    }
}
