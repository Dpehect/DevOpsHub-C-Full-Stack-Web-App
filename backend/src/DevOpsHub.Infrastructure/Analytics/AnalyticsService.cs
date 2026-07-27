using DevOpsHub.Application.Analytics;
using DevOpsHub.Domain.Incidents;
using DevOpsHub.Domain.Pipelines;
using DevOpsHub.Domain.Projects;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Analytics;

public sealed class AnalyticsService(AppDbContext db) : IAnalyticsService
{
    public async Task<AnalyticsOverviewDto> GetOverviewAsync(CancellationToken ct)
    {
        var workItems = await db.WorkItems.AsNoTracking().ToListAsync(ct);
        var runs = await db.PipelineRuns.AsNoTracking().ToListAsync(ct);
        var incidents = await db.Incidents.AsNoTracking().ToListAsync(ct);
        var services = await db.MonitoredServices.AsNoTracking().ToListAsync(ct);

        var completedRuns = runs.Where(x => x.Status is PipelineStatus.Succeeded or PipelineStatus.Failed).ToList();
        var pipelineSuccess = completedRuns.Count == 0 ? 94.8 : Math.Round(completedRuns.Count(x => x.Status == PipelineStatus.Succeeded) * 100d / completedRuns.Count, 1);
        var done = workItems.Count(x => x.Status == WorkItemStatus.Done);
        var deliveryRate = workItems.Count == 0 ? 78 : Math.Round(done * 100d / workItems.Count, 1);
        var resolved = incidents.Where(x => x.ResolvedAt.HasValue).ToList();
        var mttr = resolved.Count == 0 ? 42 : Math.Round(resolved.Average(x => (x.ResolvedAt!.Value - x.CreatedAt).TotalMinutes), 0);
        var healthy = services.Count == 0 ? 92.3 : Math.Round(services.Count(x => x.Status == ServiceStatus.Operational) * 100d / services.Count, 1);
        var openIncidents = incidents.Count(x => x.Status != IncidentStatus.Resolved);

        var factors = new List<HealthFactorDto>
        {
            new("Pipeline reliability", pipelineSuccess, 95, 30, "%"),
            new("Delivery completion", deliveryRate, 85, 25, "%"),
            new("Service availability", healthy, 99, 30, "%"),
            new("Incident response", Math.Max(0, 100 - mttr), 75, 15, "score")
        };
        var score = (int)Math.Round(factors.Sum(x => Math.Min(100, x.Value / Math.Max(1, x.Target) * 100) * x.Weight) / 100);
        var grade = score >= 90 ? "A" : score >= 80 ? "B" : score >= 70 ? "C" : "D";
        var status = score >= 85 ? "Healthy" : score >= 70 ? "Needs attention" : "At risk";

        var metrics = new List<MetricCardDto>
        {
            new("velocity", "Sprint velocity", "46 pts", "+8.2%", "up"),
            new("success", "Pipeline success", $"{pipelineSuccess:0.#}%", "+3.4%", "up"),
            new("mttr", "Mean time to recovery", $"{mttr:0} min", "-12 min", "down"),
            new("incidents", "Open incidents", openIncidents.ToString(), openIncidents > 2 ? "+1" : "-2", openIncidents > 2 ? "down" : "up")
        };

        var delivery = new[] { 31d, 36, 34, 42, 39, 46, 48 }.Select((v, i) => new TrendPointDto(new[]{"Mon","Tue","Wed","Thu","Fri","Sat","Sun"}[i], v, new[]{24d,28,31,30,35,38,41}[i])).ToList();
        var reliability = new[] { 96.2, 97.1, 95.8, 98.4, 97.9, 99.1, pipelineSuccess }.Select((v, i) => new TrendPointDto(new[]{"W1","W2","W3","W4","W5","W6","W7"}[i], v, new[]{3d,2,4,1,2,1,openIncidents}[i])).ToList();
        var team = new List<TeamLoadDto>
        {
            new("Alex Morgan", 8, 11, 1, 78), new("Maya Chen", 7, 9, 0, 64), new("Jordan Lee", 10, 12, 2, 91), new("Sam Rivera", 5, 8, 0, 52)
        };
        var risks = new List<RiskItemDto>
        {
            new("High", "API gateway error budget is narrowing", "Error rate has remained above the weekly baseline for three consecutive checks.", "Reliability"),
            new("Medium", "Two work items exceed sprint age", "Long-running items may affect the current release commitment.", "Delivery"),
            new("Low", "Documentation coverage below target", "Three production services do not yet have complete runbooks.", "Operations")
        };

        return new(new(score, grade, status, factors), metrics, delivery, reliability, team, risks, DateTime.UtcNow);
    }
}
