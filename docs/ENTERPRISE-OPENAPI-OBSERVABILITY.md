# Enterprise OpenAPI and Observability

## Endpoints

- Swagger UI: `/api-docs`
- OpenAPI JSON: `/api-docs/v1/openapi.json`
- Prometheus metrics: `/metrics`
- Readiness: `/healthz`
- Liveness: `/health/live`

Swagger UI is disabled by default in production. Enable it with:

```env
ENABLE_SWAGGER_UI=true
```

Expose Swagger only behind VPN, private ingress or an authenticated gateway.

## Prometheus

Start the stack:

```bash
docker compose up --build
```

Prometheus UI:

```text
http://localhost:9090
```

Example queries:

```promql
rate(http_requests_received_total[5m])
histogram_quantile(
  0.95,
  sum by (le) (
    rate(devopshub_application_request_duration_seconds_bucket[5m])
  )
)
devopshub_http_requests_active
process_working_set_bytes
process_cpu_seconds_total
```

## Custom business metrics

Use:

```csharp
DevOpsHubMetrics.PipelineTriggers
    .WithLabels("success")
    .Inc();

DevOpsHubMetrics.IncidentOperations
    .WithLabels("create", "success")
    .Inc();
```

Record failure labels inside exception/error branches.
