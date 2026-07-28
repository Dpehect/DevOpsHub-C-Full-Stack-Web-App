# Enterprise Final Check

## OpenAPI

- UI: `/api-docs`
- Document: `/api-docs/v1/openapi.json`
- Bearer JWT is attached only to authorized operations.
- Standard 400, 429 and 500 responses are added globally.
- 401 and 403 responses are added to protected operations.
- Swagger UI remains disabled by default in production.

Enable only behind private network controls:

```env
ENABLE_SWAGGER_UI=true
```

## Prometheus

- Metrics endpoint: `/metrics`
- HTTP request count: `devopshub_http_requests_total`
- HTTP latency: `devopshub_http_request_duration_seconds`
- HTTP 5xx count: `devopshub_http_5xx_total`
- Active requests: `devopshub_http_requests_active`
- Process CPU/RAM and .NET runtime metrics are exported by prometheus-net default collectors.

## Docker

The Prometheus target is `api:8080`, matching the Docker Compose service name
and ASP.NET Core container port.

## Native modules

The current application has no runtime C/native API. `/api/native/status`
documents this explicitly. Native helper functions remain protected by bounded
`snprintf`/`vsnprintf`, explicit null checks and sanitizer-enabled CI compilation.
