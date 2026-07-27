# Security Policy

## Supported versions

Only the latest `main` branch is supported.

## Secret handling

Production secrets must never be committed. Configure:

- `JWT_SIGNING_KEY`
- `POSTGRES_PASSWORD`
- `FRONTEND_ORIGIN`

through the deployment platform's secret manager.

## Reporting

Report vulnerabilities through a private GitHub security advisory. Do not open a public issue for exploitable findings.

## Operational requirements

- HTTPS is mandatory.
- Rotate JWT signing keys after exposure.
- Disable development seed accounts in production.
- Review audit logs after role and credential changes.
