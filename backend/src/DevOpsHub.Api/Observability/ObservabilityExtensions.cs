using DevOpsHub.Api.Middleware;
using Prometheus;

namespace DevOpsHub.Api.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddEnterpriseObservability(
        this IServiceCollection services)
    {
        Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
        {
            ["application"] = "devopshub-api"
        });

        Metrics.SuppressDefaultMetrics(new SuppressDefaultMetricOptions
        {
            SuppressDebugMetrics = true,
            SuppressEventCounters = false,
            SuppressProcessMetrics = false
        });

        return services;
    }

    public static WebApplication UseEnterpriseObservability(
        this WebApplication app)
    {
        app.UseMiddleware<ApplicationMetricsMiddleware>();

        app.MapMetrics("/metrics")
            .AllowAnonymous()
            .ExcludeFromDescription();

        return app;
    }
}
