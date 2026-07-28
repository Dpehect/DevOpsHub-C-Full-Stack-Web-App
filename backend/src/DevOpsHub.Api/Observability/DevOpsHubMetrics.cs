using Prometheus;

namespace DevOpsHub.Api.Observability;

public static class DevOpsHubMetrics
{
    public static readonly Gauge ActiveHttpRequests = Metrics.CreateGauge(
        "devopshub_http_requests_active",
        "Number of HTTP requests currently being processed.");

    public static readonly Counter HttpRequests = Metrics.CreateCounter(
        "devopshub_http_requests_total",
        "Total HTTP requests processed by the application.",
        new CounterConfiguration
        {
            LabelNames = ["method", "route", "status_code"]
        });

    public static readonly Counter HttpServerErrors = Metrics.CreateCounter(
        "devopshub_http_5xx_total",
        "Total HTTP responses with a 5xx status code.",
        new CounterConfiguration
        {
            LabelNames = ["method", "route", "status_code"]
        });

    public static readonly Histogram HttpRequestDuration = Metrics.CreateHistogram(
        "devopshub_http_request_duration_seconds",
        "HTTP request duration in seconds.",
        new HistogramConfiguration
        {
            LabelNames = ["method", "route", "status_code"],
            Buckets = Histogram.ExponentialBuckets(
                start: 0.005,
                factor: 2,
                count: 14)
        });

    public static readonly Counter PipelineTriggers = Metrics.CreateCounter(
        "devopshub_pipeline_triggers_total",
        "Total number of pipeline trigger attempts.",
        new CounterConfiguration
        {
            LabelNames = ["result"]
        });

    public static readonly Counter IncidentOperations = Metrics.CreateCounter(
        "devopshub_incident_operations_total",
        "Total number of incident operations.",
        new CounterConfiguration
        {
            LabelNames = ["operation", "result"]
        });
}
