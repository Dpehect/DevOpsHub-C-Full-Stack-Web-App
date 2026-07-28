using System.Diagnostics;
using DevOpsHub.Api.Observability;

namespace DevOpsHub.Api.Middleware;

public sealed class ApplicationMetricsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        DevOpsHubMetrics.ActiveHttpRequests.Inc();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            DevOpsHubMetrics.ActiveHttpRequests.Dec();

            var route =
                context.GetEndpoint()?.DisplayName
                ?? context.Request.Path.Value
                ?? "unknown";

            DevOpsHubMetrics.ApplicationRequestDuration
                .WithLabels(
                    context.Request.Method,
                    NormalizeRoute(route),
                    context.Response.StatusCode.ToString())
                .Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }

    private static string NormalizeRoute(string route)
    {
        if (route.Length <= 160)
            return route;

        return route[..160];
    }
}
