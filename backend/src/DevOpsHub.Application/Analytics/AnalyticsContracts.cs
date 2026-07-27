namespace DevOpsHub.Application.Analytics;

public sealed record AnalyticsOverviewDto(
    ProjectHealthDto ProjectHealth,
    IReadOnlyList<MetricCardDto> Metrics,
    IReadOnlyList<TrendPointDto> DeliveryTrend,
    IReadOnlyList<TrendPointDto> ReliabilityTrend,
    IReadOnlyList<TeamLoadDto> TeamLoad,
    IReadOnlyList<RiskItemDto> Risks,
    DateTime GeneratedAtUtc);

public sealed record ProjectHealthDto(int Score, string Grade, string Status, IReadOnlyList<HealthFactorDto> Factors);
public sealed record HealthFactorDto(string Name, double Value, double Target, int Weight, string Unit);
public sealed record MetricCardDto(string Key, string Label, string Value, string Delta, string Direction);
public sealed record TrendPointDto(string Label, double Value, double SecondaryValue);
public sealed record TeamLoadDto(string Member, int Assigned, int Completed, int Incidents, int CapacityPercent);
public sealed record RiskItemDto(string Severity, string Title, string Description, string Area);

public interface IAnalyticsService
{
    Task<AnalyticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
}
