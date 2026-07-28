# Enterprise Final Integration

## Endpoints

- Swagger UI: `/api-docs`
- OpenAPI JSON: `/api-docs/v1/openapi.json`
- Prometheus: `/metrics`
- Health: `/healthz`
- Grafana: `http://localhost:3001`
- Prometheus UI: `http://localhost:9090`

## Production defaults

Swagger UI is disabled by default. Enable it only behind a VPN, private ingress
or authenticated gateway:

```env
ENABLE_SWAGGER_UI=true
```

Grafana requires an explicit admin password through `.env`.

## Collected metrics

- `devopshub_http_requests_total`
- `devopshub_http_5xx_total`
- `devopshub_http_request_duration_seconds`
- `devopshub_http_requests_active`
- `process_working_set_bytes`
- `process_cpu_seconds_total`
- .NET runtime event counter metrics

## Validation

```bash
docker compose config
docker compose up --build

curl -f http://localhost:3000/api/system
curl -f http://localhost:9090/-/ready
curl -f http://localhost:3001/api/health
```
