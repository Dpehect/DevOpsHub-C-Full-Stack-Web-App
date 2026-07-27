using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace DevOpsHub.Api.Extensions;

public static class HealthEndpointExtensions
{
    public static IEndpointRouteBuilder MapProductionHealthChecks(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return endpoints;
    }
}
