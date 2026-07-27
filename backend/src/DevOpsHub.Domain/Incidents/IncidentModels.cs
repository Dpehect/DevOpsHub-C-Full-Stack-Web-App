namespace DevOpsHub.Domain.Incidents;

public enum IncidentSeverity { Sev1, Sev2, Sev3, Sev4 }
public enum IncidentStatus { Investigating, Identified, Monitoring, Resolved }
public enum ServiceStatus { Operational, Degraded, PartialOutage, MajorOutage, Maintenance }

public sealed class MonitoredService : Entity
{
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; } = ServiceStatus.Operational;
    public decimal AvailabilityPercent { get; set; } = 100;
    public int TargetSlaMinutes { get; set; } = 60;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<Incident> Incidents { get; set; } = [];
}

public sealed class Incident : Entity
{
    public Guid WorkspaceId { get; set; }
    public Guid MonitoredServiceId { get; set; }
    public MonitoredService MonitoredService { get; set; } = null!;
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Investigating;
    public string Commander { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime SlaDueAtUtc { get; set; }
    public List<IncidentUpdate> Updates { get; set; } = [];
    public List<PostmortemAction> Actions { get; set; } = [];
}

public sealed class IncidentUpdate : Entity
{
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;
    public IncidentStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class PostmortemAction : Entity
{
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime DueAtUtc { get; set; }
    public bool IsCompleted { get; set; }
}
