using DevOpsHub.Application.Incidents;
using DevOpsHub.Domain.Incidents;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsHub.Infrastructure.Incidents;

public sealed class IncidentService(AppDbContext db) : IIncidentService
{
    public async Task<IncidentDashboardDto> GetDashboardAsync(Guid workspaceId, CancellationToken ct)
    {
        await EnsureSeedAsync(workspaceId, ct);
        var services = await db.MonitoredServices.Include(x => x.Incidents).Where(x => x.WorkspaceId == workspaceId).OrderBy(x => x.Name).ToListAsync(ct);
        var incidents = await db.Incidents.Include(x => x.MonitoredService).Include(x => x.Updates).Where(x => x.WorkspaceId == workspaceId).OrderByDescending(x => x.StartedAtUtc).ToListAsync(ct);
        var resolved = incidents.Where(x => x.ResolvedAtUtc.HasValue).ToList();
        var mttr = resolved.Count == 0 ? 0 : (decimal)resolved.Average(x => (x.ResolvedAtUtc!.Value - x.StartedAtUtc).TotalMinutes);
        var compliance = resolved.Count == 0 ? 100 : resolved.Count(x => x.ResolvedAtUtc <= x.SlaDueAtUtc) * 100m / resolved.Count;
        return new(incidents.Count(x => x.Status != IncidentStatus.Resolved), incidents.Count(x => x.Status != IncidentStatus.Resolved && x.Severity == IncidentSeverity.Sev1), 7.4m, Math.Round(mttr, 1), Math.Round(compliance, 1), services.Select(MapService).ToList(), incidents.Select(MapIncident).ToList());
    }

    public async Task<IncidentDto> CreateAsync(CreateIncidentRequest request, CancellationToken ct)
    {
        var service = await db.MonitoredServices.FirstAsync(x => x.Id == request.ServiceId && x.WorkspaceId == request.WorkspaceId, ct);
        var number = (await db.Incidents.Where(x => x.WorkspaceId == request.WorkspaceId).MaxAsync(x => (int?)x.Number, ct) ?? 0) + 1;
        var incident = new Incident { WorkspaceId=request.WorkspaceId, MonitoredServiceId=service.Id, Number=number, Title=request.Title.Trim(), Summary=request.Summary.Trim(), Severity=request.Severity, Commander=request.Commander.Trim(), SlaDueAtUtc=DateTime.UtcNow.AddMinutes(service.TargetSlaMinutes) };
        incident.Updates.Add(new IncidentUpdate { Status=IncidentStatus.Investigating, Message="Incident declared and response team mobilized.", Author=incident.Commander });
        service.Status = request.Severity == IncidentSeverity.Sev1 ? ServiceStatus.MajorOutage : ServiceStatus.Degraded;
        db.Incidents.Add(incident); await db.SaveChangesAsync(ct); incident.MonitoredService=service; return MapIncident(incident);
    }

    public async Task<IncidentDto?> AddUpdateAsync(Guid incidentId, AddIncidentUpdateRequest request, CancellationToken ct)
    {
        var incident = await db.Incidents.Include(x => x.MonitoredService).Include(x => x.Updates).FirstOrDefaultAsync(x => x.Id == incidentId, ct); if (incident is null) return null;
        incident.Status=request.Status; incident.Updates.Add(new IncidentUpdate { Status=request.Status, Message=request.Message.Trim(), Author=request.Author.Trim() });
        if(request.Status==IncidentStatus.Resolved){ incident.ResolvedAtUtc=DateTime.UtcNow; incident.MonitoredService.Status=ServiceStatus.Operational; }
        await db.SaveChangesAsync(ct); return MapIncident(incident);
    }

    public async Task<ServiceDto> CreateServiceAsync(CreateServiceRequest request, CancellationToken ct)
    {
        var service = new MonitoredService { WorkspaceId=request.WorkspaceId, Name=request.Name.Trim(), Slug=request.Name.Trim().ToLowerInvariant().Replace(' ','-'), Description=request.Description.Trim(), TargetSlaMinutes=Math.Clamp(request.TargetSlaMinutes,15,1440) };
        db.MonitoredServices.Add(service); await db.SaveChangesAsync(ct); return MapService(service);
    }

    private async Task EnsureSeedAsync(Guid workspaceId, CancellationToken ct)
    {
        if(await db.MonitoredServices.AnyAsync(x=>x.WorkspaceId==workspaceId,ct)) return;
        var api = new MonitoredService { WorkspaceId=workspaceId, Name="Public API", Slug="public-api", Description="Customer-facing REST API", Status=ServiceStatus.Degraded, AvailabilityPercent=99.94m, TargetSlaMinutes=45 };
        var auth = new MonitoredService { WorkspaceId=workspaceId, Name="Identity", Slug="identity", Description="Authentication and token service", Status=ServiceStatus.Operational, AvailabilityPercent=99.99m, TargetSlaMinutes=30 };
        var web = new MonitoredService { WorkspaceId=workspaceId, Name="Web Application", Slug="web-app", Description="Primary customer dashboard", Status=ServiceStatus.Operational, AvailabilityPercent=99.97m, TargetSlaMinutes=60 };
        db.MonitoredServices.AddRange(api,auth,web); await db.SaveChangesAsync(ct);
        var incident = new Incident { WorkspaceId=workspaceId, MonitoredServiceId=api.Id, Number=1042, Title="Elevated API latency in EU region", Summary="P95 latency exceeded 2.5 seconds for write endpoints.", Severity=IncidentSeverity.Sev2, Status=IncidentStatus.Identified, Commander="Elif Kaya", StartedAtUtc=DateTime.UtcNow.AddMinutes(-38), SlaDueAtUtc=DateTime.UtcNow.AddMinutes(7) };
        incident.Updates.Add(new IncidentUpdate { Status=IncidentStatus.Investigating, Message="On-call acknowledged elevated latency alerts.", Author="Elif Kaya", CreatedAtUtc=DateTime.UtcNow.AddMinutes(-38) });
        incident.Updates.Add(new IncidentUpdate { Status=IncidentStatus.Identified, Message="Connection pool saturation identified. Capacity increased while root cause is investigated.", Author="Mert Demir", CreatedAtUtc=DateTime.UtcNow.AddMinutes(-16) });
        db.Incidents.Add(incident); await db.SaveChangesAsync(ct);
    }

    private static ServiceDto MapService(MonitoredService x)=>new(x.Id,x.Name,x.Slug,x.Description,x.Status,x.AvailabilityPercent,x.TargetSlaMinutes,x.Incidents.Count(i=>i.Status!=IncidentStatus.Resolved));
    private static IncidentDto MapIncident(Incident x)=>new(x.Id,x.Number,x.Title,x.Summary,x.Severity,x.Status,x.Commander,x.StartedAtUtc,x.ResolvedAtUtc,x.SlaDueAtUtc,(x.ResolvedAtUtc??DateTime.UtcNow)>x.SlaDueAtUtc,x.MonitoredService.Name,x.Updates.OrderByDescending(u=>u.CreatedAtUtc).Select(u=>new IncidentUpdateDto(u.Id,u.Status,u.Message,u.Author,u.CreatedAtUtc)).ToList());
}
