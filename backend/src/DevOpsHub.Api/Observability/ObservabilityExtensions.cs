using DevOpsHub.Api.Middleware;
using Prometheus;

namespace DevOpsHub.Api.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddEnterpriseObservability(
        this IServiceCollection services)
    {
        Metrics.SuppressDefaultMetrics(new SuppressDefaultMetricOptions
        {
            SuppressEventCounters = false,
            SuppressProcessMetrics = false,
            SuppressDebugMetrics = true
        });

        return services;
    }

    public static WebApplication UseEnterpriseObservability(
        this WebApplication app)
    {
        app.UseHttpMetrics(options =>
        {
            options.AddCustomLabel(
                "host",
                context => context.Request.Host.Host);
        });

        app.UseMiddleware<ApplicationMetricsMiddleware>();

        app.MapMetrics("/metrics")
            .AllowAnonymous();

        return app;
    }
}
