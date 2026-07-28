using Prometheus;

namespace DevOpsHub.Api.Observability;

public static class DevOpsHubMetrics
{
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

    public static readonly Gauge ActiveHttpRequests = Metrics.CreateGauge(
        "devopshub_http_requests_active",
        "Number of HTTP requests currently being processed.");

    public static readonly Histogram ApplicationRequestDuration =
        Metrics.CreateHistogram(
            "devopshub_application_request_duration_seconds",
            "Application HTTP request duration in seconds.",
            new HistogramConfiguration
            {
                LabelNames = ["method", "route", "status_code"],
                Buckets = Histogram.ExponentialBuckets(
                    start: 0.005,
                    factor: 2,
                    count: 14)
            });
}
