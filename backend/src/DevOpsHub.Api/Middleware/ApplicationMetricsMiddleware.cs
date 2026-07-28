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

            var route = ResolveRoute(context);
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode.ToString();

            DevOpsHubMetrics.HttpRequests
                .WithLabels(method, route, statusCode)
                .Inc();

            DevOpsHubMetrics.HttpRequestDuration
                .WithLabels(method, route, statusCode)
                .Observe(stopwatch.Elapsed.TotalSeconds);

            if (context.Response.StatusCode >= 500)
            {
                DevOpsHubMetrics.HttpServerErrors
                    .WithLabels(method, route, statusCode)
                    .Inc();
            }
        }
    }

    private static string ResolveRoute(HttpContext context)
    {
        var routePattern = context
            .GetEndpoint()?
            .Metadata
            .GetMetadata<Microsoft.AspNetCore.Routing.RouteNameMetadata>()?
            .RouteName;

        var displayName = context.GetEndpoint()?.DisplayName;
        var route = routePattern
            ?? displayName
            ?? context.Request.Path.Value
            ?? "unknown";

        return route.Length <= 160 ? route : route[..160];
    }
}
