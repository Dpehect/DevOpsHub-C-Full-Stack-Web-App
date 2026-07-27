using DevOpsHub.Domain.Pipelines;
namespace DevOpsHub.Tests;
public sealed class PipelineDomainTests
{
 [Fact] public void New_pipeline_is_active()=>Assert.True(new PipelineDefinition().IsActive);
 [Fact] public void New_run_is_queued()=>Assert.Equal(PipelineStatus.Queued,new PipelineRun().Status);
 [Fact] public void Stage_order_is_deterministic()=>Assert.Equal("Checkout",new[]{new PipelineStage{Name="Build",Order=2},new PipelineStage{Name="Checkout",Order=1}}.OrderBy(x=>x.Order).First().Name);
}
