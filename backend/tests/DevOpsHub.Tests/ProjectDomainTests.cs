using DevOpsHub.Domain.Projects;
using Xunit;
namespace DevOpsHub.Tests;
public sealed class ProjectDomainTests
{
 [Fact] public void Project_key_is_normalized(){var x=new Project(Guid.NewGuid(),"Platform","doh",null,Guid.NewGuid());Assert.Equal("DOH",x.Key);}
 [Fact] public void Sprint_rejects_invalid_range(){Assert.Throws<ArgumentException>(()=>new Sprint(Guid.NewGuid(),"S1",new DateOnly(2026,8,2),new DateOnly(2026,8,1),null));}
 [Fact] public void Work_item_can_move_columns(){var x=new WorkItem(Guid.NewGuid(),1,"Build API",WorkItemType.Story,WorkItemPriority.High,Guid.NewGuid());x.Move(WorkItemStatus.InProgress,2,null);Assert.Equal(WorkItemStatus.InProgress,x.Status);Assert.Equal(2,x.Position);}
}
