using System.Diagnostics;
using DevOpsHub.Api.Observability;
using Microsoft.AspNetCore.Routing.Patterns;

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

            if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                DevOpsHubMetrics.HttpServerErrors
                    .WithLabels(method, route, statusCode)
                    .Inc();
            }
        }
    }

    private static string ResolveRoute(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint is RouteEndpoint routeEndpoint)
        {
            var rawText = routeEndpoint.RoutePattern.RawText;
            if (!string.IsNullOrWhiteSpace(rawText))
                return Limit(rawText);
        }

        return Limit(context.Request.Path.Value ?? "unknown");
    }

    private static string Limit(string value) =>
        value.Length <= 160 ? value : value[..160];
}
