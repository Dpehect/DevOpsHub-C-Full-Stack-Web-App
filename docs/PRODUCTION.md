# Production Runbook

## Required configuration

Copy `.env.example` to `.env` and replace every placeholder with strong values.

## Database

Production uses PostgreSQL. Run EF Core migrations as a dedicated deployment step before starting new application instances.

Recommended command:

```bash
dotnet ef database update \
  --project backend/src/DevOpsHub.Infrastructure \
  --startup-project backend/src/DevOpsHub.Api
```

Do not run migrations automatically from every application instance.

## Health endpoints

- `/health/live`: process liveness
- `/health/ready`: database readiness

## Logging

Use structured console logs as the primary sink. Forward stdout/stderr to the platform logging service. Redact authorization headers, refresh tokens, access-token query parameters and credentials.

## Backup

- Daily PostgreSQL backups
- Weekly restore verification
- Documented RPO/RTO
- Encrypted off-site backup retention

## Deployment order

1. Build and scan images.
2. Apply database migrations.
3. Deploy API instances.
4. Wait for readiness.
5. Deploy frontend.
6. Run smoke tests.
7. Roll back if readiness or smoke checks fail.
