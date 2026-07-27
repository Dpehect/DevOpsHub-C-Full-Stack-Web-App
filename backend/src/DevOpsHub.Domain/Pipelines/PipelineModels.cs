namespace DevOpsHub.Domain.Pipelines;

public enum PipelineStatus { Queued, Running, Succeeded, Failed, Cancelled }
public enum PipelineTrigger { Push, PullRequest, Manual, Schedule }
public enum StageStatus { Pending, Running, Succeeded, Failed, Skipped }
public enum DeploymentEnvironment { Development, Staging, Production }

public sealed class PipelineDefinition : Entity
{
    public Guid RepositoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PipelineRun> Runs { get; set; } = new List<PipelineRun>();
}

public sealed class PipelineRun : Entity
{
    public Guid PipelineDefinitionId { get; set; }
    public PipelineDefinition PipelineDefinition { get; set; } = null!;
    public int Number { get; set; }
    public PipelineStatus Status { get; set; } = PipelineStatus.Queued;
    public PipelineTrigger Trigger { get; set; }
    public string Branch { get; set; } = "main";
    public string CommitSha { get; set; } = string.Empty;
    public string CommitMessage { get; set; } = string.Empty;
    public string TriggeredBy { get; set; } = string.Empty;
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<PipelineStage> Stages { get; set; } = new List<PipelineStage>();
    public ICollection<Deployment> Deployments { get; set; } = new List<Deployment>();
}

public sealed class PipelineStage : Entity
{
    public Guid PipelineRunId { get; set; }
    public PipelineRun PipelineRun { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public StageStatus Status { get; set; } = StageStatus.Pending;
    public int DurationSeconds { get; set; }
    public string Log { get; set; } = string.Empty;
}

public sealed class Deployment : Entity
{
    public Guid PipelineRunId { get; set; }
    public PipelineRun PipelineRun { get; set; } = null!;
    public DeploymentEnvironment Environment { get; set; }
    public PipelineStatus Status { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; } = DateTime.UtcNow;
    public string DeployedBy { get; set; } = string.Empty;
}
