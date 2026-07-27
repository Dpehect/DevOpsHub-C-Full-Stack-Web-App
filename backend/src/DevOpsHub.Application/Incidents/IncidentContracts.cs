using DevOpsHub.Domain.Incidents;

namespace DevOpsHub.Application.Incidents;

public sealed record CreateIncidentRequest(Guid WorkspaceId, Guid ServiceId, string Title, string Summary, IncidentSeverity Severity, string Commander);
public sealed record AddIncidentUpdateRequest(IncidentStatus Status, string Message, string Author);
public sealed record CreateServiceRequest(Guid WorkspaceId, string Name, string Description, int TargetSlaMinutes);
public sealed record IncidentUpdateDto(Guid Id, IncidentStatus Status, string Message, string Author, DateTime CreatedAtUtc);
public sealed record IncidentDto(Guid Id, int Number, string Title, string Summary, IncidentSeverity Severity, IncidentStatus Status, string Commander, DateTime StartedAtUtc, DateTime? ResolvedAtUtc, DateTime SlaDueAtUtc, bool IsSlaBreached, string ServiceName, IReadOnlyList<IncidentUpdateDto> Updates);
public sealed record ServiceDto(Guid Id, string Name, string Slug, string Description, ServiceStatus Status, decimal AvailabilityPercent, int TargetSlaMinutes, int OpenIncidentCount);
public sealed record IncidentDashboardDto(int ActiveIncidents, int Sev1Count, decimal MttaMinutes, decimal MttrMinutes, decimal SlaCompliancePercent, IReadOnlyList<ServiceDto> Services, IReadOnlyList<IncidentDto> Incidents);

public interface IIncidentService
{
    Task<IncidentDashboardDto> GetDashboardAsync(Guid workspaceId, CancellationToken ct);
    Task<IncidentDto> CreateAsync(CreateIncidentRequest request, CancellationToken ct);
    Task<IncidentDto?> AddUpdateAsync(Guid incidentId, AddIncidentUpdateRequest request, CancellationToken ct);
    Task<ServiceDto> CreateServiceAsync(CreateServiceRequest request, CancellationToken ct);
}
