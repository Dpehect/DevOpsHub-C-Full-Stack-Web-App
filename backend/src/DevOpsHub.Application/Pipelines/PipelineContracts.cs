using DevOpsHub.Domain.Pipelines;

namespace DevOpsHub.Application.Pipelines;

public sealed record PipelineDefinitionDto(Guid Id, Guid RepositoryId, string Name, string Branch, bool IsActive, int TotalRuns, double SuccessRate, DateTime UpdatedAt);
public sealed record PipelineStageDto(Guid Id, string Name, int Order, StageStatus Status, int DurationSeconds, string Log);
public sealed record DeploymentDto(Guid Id, DeploymentEnvironment Environment, PipelineStatus Status, string Version, string Url, DateTime DeployedAt, string DeployedBy);
public sealed record PipelineRunDto(Guid Id, Guid PipelineDefinitionId, int Number, PipelineStatus Status, PipelineTrigger Trigger, string Branch, string CommitSha, string CommitMessage, string TriggeredBy, DateTime QueuedAt, DateTime? StartedAt, DateTime? CompletedAt, IReadOnlyCollection<PipelineStageDto> Stages, IReadOnlyCollection<DeploymentDto> Deployments);
public sealed record PipelineDashboardDto(IReadOnlyCollection<PipelineDefinitionDto> Pipelines, IReadOnlyCollection<PipelineRunDto> RecentRuns, IReadOnlyCollection<DeploymentDto> RecentDeployments, double SuccessRate, int AverageDurationSeconds, int DeploymentsThisWeek);
public sealed record CreatePipelineRequest(Guid RepositoryId, string Name, string Branch);
public sealed record RunPipelineRequest(PipelineTrigger Trigger, string Branch, string CommitSha, string CommitMessage);

public interface IPipelineService
{
    Task<PipelineDashboardDto> GetDashboardAsync(CancellationToken cancellationToken);
    Task<PipelineRunDto?> GetRunAsync(Guid runId, CancellationToken cancellationToken);
    Task<PipelineDefinitionDto> CreateAsync(CreatePipelineRequest request, CancellationToken cancellationToken);
    Task<PipelineRunDto> RunAsync(Guid pipelineId, RunPipelineRequest request, string triggeredBy, CancellationToken cancellationToken);
    Task<PipelineRunDto?> RetryAsync(Guid runId, string triggeredBy, CancellationToken cancellationToken);
    Task<bool> CancelAsync(Guid runId, CancellationToken cancellationToken);
}
