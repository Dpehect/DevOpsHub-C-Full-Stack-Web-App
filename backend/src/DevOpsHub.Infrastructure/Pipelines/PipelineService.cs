using DevOpsHub.Application.Pipelines;
using DevOpsHub.Domain.Pipelines;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Pipelines;

public sealed class PipelineService(AppDbContext db) : IPipelineService
{
    public async Task<PipelineDashboardDto> GetDashboardAsync(CancellationToken ct)
    {
        await EnsureSeededAsync(ct);
        var definitions = await db.PipelineDefinitions.Include(x => x.Runs).OrderBy(x => x.Name).ToListAsync(ct);
        var runs = await db.PipelineRuns.Include(x => x.Stages).Include(x => x.Deployments).OrderByDescending(x => x.QueuedAt).Take(12).ToListAsync(ct);
        var deployments = await db.Deployments.OrderByDescending(x => x.DeployedAt).Take(8).ToListAsync(ct);
        var completed = runs.Where(x => x.Status is PipelineStatus.Succeeded or PipelineStatus.Failed).ToList();
        return new(
            definitions.Select(Map).ToList(), runs.Select(Map).ToList(), deployments.Select(Map).ToList(),
            completed.Count == 0 ? 0 : Math.Round(completed.Count(x => x.Status == PipelineStatus.Succeeded) * 100d / completed.Count, 1),
            completed.Count == 0 ? 0 : (int)completed.Average(Duration),
            deployments.Count(x => x.DeployedAt >= DateTime.UtcNow.AddDays(-7)));
    }

    public async Task<PipelineRunDto?> GetRunAsync(Guid id, CancellationToken ct) =>
        (await db.PipelineRuns.Include(x => x.Stages).Include(x => x.Deployments).FirstOrDefaultAsync(x => x.Id == id, ct)) is { } run ? Map(run) : null;

    public async Task<PipelineDefinitionDto> CreateAsync(CreatePipelineRequest r, CancellationToken ct)
    {
        var entity = new PipelineDefinition { RepositoryId = r.RepositoryId, Name = r.Name.Trim(), Branch = r.Branch.Trim(), UpdatedAt = DateTime.UtcNow };
        db.PipelineDefinitions.Add(entity); await db.SaveChangesAsync(ct); return Map(entity);
    }

    public async Task<PipelineRunDto> RunAsync(Guid pipelineId, RunPipelineRequest r, string user, CancellationToken ct)
    {
        var pipeline = await db.PipelineDefinitions.Include(x => x.Runs).FirstOrDefaultAsync(x => x.Id == pipelineId, ct) ?? throw new KeyNotFoundException("Pipeline not found.");
        var run = BuildRun(pipeline, pipeline.Runs.Count + 1, r, user);
        db.PipelineRuns.Add(run); pipeline.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); return Map(run);
    }

    public async Task<PipelineRunDto?> RetryAsync(Guid runId, string user, CancellationToken ct)
    {
        var source = await db.PipelineRuns.Include(x => x.PipelineDefinition).ThenInclude(x => x.Runs).FirstOrDefaultAsync(x => x.Id == runId, ct);
        if (source is null) return null;
        var request = new RunPipelineRequest(PipelineTrigger.Manual, source.Branch, source.CommitSha, $"Retry: {source.CommitMessage}");
        var retry = BuildRun(source.PipelineDefinition, source.PipelineDefinition.Runs.Count + 1, request, user);
        db.PipelineRuns.Add(retry); await db.SaveChangesAsync(ct); return Map(retry);
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct)
    {
        var run = await db.PipelineRuns.Include(x => x.Stages).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (run is null || run.Status is PipelineStatus.Succeeded or PipelineStatus.Failed) return false;
        run.Status = PipelineStatus.Cancelled; run.CompletedAt = DateTime.UtcNow;
        foreach (var stage in run.Stages.Where(x => x.Status is StageStatus.Pending or StageStatus.Running)) stage.Status = StageStatus.Skipped;
        await db.SaveChangesAsync(ct); return true;
    }

    private static PipelineRun BuildRun(PipelineDefinition p, int number, RunPipelineRequest r, string user)
    {
        var failed = r.CommitMessage.Contains("fail", StringComparison.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;
        var stages = new[]
        {
            Stage("Checkout",1,18,"Repository checked out and cache restored."), Stage("Restore",2,24,"NuGet and npm dependencies restored."),
            Stage("Build",3,46,"Backend and frontend compiled successfully."), Stage("Test",4,39, failed ? "2 integration tests failed." : "128 tests passed. Coverage 84%.", failed),
            Stage("Security scan",5,21,"No critical vulnerabilities detected.", failed, true), Stage("Package",6,17,"Container image assembled.", failed, true)
        };
        var status = failed ? PipelineStatus.Failed : PipelineStatus.Succeeded;
        var run = new PipelineRun { PipelineDefinitionId = p.Id, Number = number, Status = status, Trigger = r.Trigger, Branch = r.Branch, CommitSha = r.CommitSha, CommitMessage = r.CommitMessage, TriggeredBy = user, QueuedAt = now.AddMinutes(-4), StartedAt = now.AddMinutes(-3), CompletedAt = now };
        foreach (var s in stages) run.Stages.Add(s);
        if (!failed) run.Deployments.Add(new Deployment { Environment = DeploymentEnvironment.Staging, Status = PipelineStatus.Succeeded, Version = $"0.7.{number}", Url = $"https://staging-{number}.devopshub.local", DeployedAt = now, DeployedBy = user });
        return run;
    }

    private static PipelineStage Stage(string name,int order,int seconds,string log,bool failed=false,bool skip=false) => new() { Name=name, Order=order, DurationSeconds=seconds, Log=log, Status=skip?StageStatus.Skipped:failed&&name=="Test"?StageStatus.Failed:StageStatus.Succeeded };
    private static int Duration(PipelineRun x) => x.StartedAt.HasValue && x.CompletedAt.HasValue ? (int)(x.CompletedAt.Value-x.StartedAt.Value).TotalSeconds : x.Stages.Sum(s=>s.DurationSeconds);
    private static PipelineDefinitionDto Map(PipelineDefinition x) { var done=x.Runs.Where(r=>r.Status is PipelineStatus.Succeeded or PipelineStatus.Failed).ToList(); return new(x.Id,x.RepositoryId,x.Name,x.Branch,x.IsActive,x.Runs.Count,done.Count==0?0:Math.Round(done.Count(r=>r.Status==PipelineStatus.Succeeded)*100d/done.Count,1),x.UpdatedAt); }
    private static PipelineRunDto Map(PipelineRun x) => new(x.Id,x.PipelineDefinitionId,x.Number,x.Status,x.Trigger,x.Branch,x.CommitSha,x.CommitMessage,x.TriggeredBy,x.QueuedAt,x.StartedAt,x.CompletedAt,x.Stages.OrderBy(s=>s.Order).Select(Map).ToList(),x.Deployments.Select(Map).ToList());
    private static PipelineStageDto Map(PipelineStage x)=>new(x.Id,x.Name,x.Order,x.Status,x.DurationSeconds,x.Log);
    private static DeploymentDto Map(Deployment x)=>new(x.Id,x.Environment,x.Status,x.Version,x.Url,x.DeployedAt,x.DeployedBy);

    private async Task EnsureSeededAsync(CancellationToken ct)
    {
        if (await db.PipelineDefinitions.AnyAsync(ct)) return;
        var repo = await db.Repositories.FirstOrDefaultAsync(ct);
        var repoId = repo?.Id ?? Guid.NewGuid();
        var p1 = new PipelineDefinition { RepositoryId=repoId, Name="DevOpsHub CI", Branch="main", UpdatedAt=DateTime.UtcNow };
        var p2 = new PipelineDefinition { RepositoryId=repoId, Name="Pull Request Validation", Branch="pull/*", UpdatedAt=DateTime.UtcNow.AddHours(-2) };
        db.PipelineDefinitions.AddRange(p1,p2); await db.SaveChangesAsync(ct);
        var samples = new[]
        {
            BuildRun(p1,148,new(PipelineTrigger.Push,"main","a8d31f2","feat: pipeline center and deployment history"),"Alex Morgan"),
            BuildRun(p2,147,new(PipelineTrigger.PullRequest,"feature/repository-center","c74e9b1","fix: fail flaky integration suite"),"Maya Chen"),
            BuildRun(p1,146,new(PipelineTrigger.Push,"main","4e201cd","test: expand workspace authorization coverage"),"Jordan Lee"),
            BuildRun(p1,145,new(PipelineTrigger.Manual,"main","018fcb4","release: stabilize project board queries"),"Sam Rivera")
        };
        db.PipelineRuns.AddRange(samples); await db.SaveChangesAsync(ct);
    }
}
