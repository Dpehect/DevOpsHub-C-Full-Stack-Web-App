# DevOpsHub Production Hardening V5

This patch contains:

- Global FluentValidation request validation for ASP.NET Core controllers
- Strict Zod request and response schemas on the React client
- ASP.NET Core rate limiting and security headers
- Serilog compact JSON logging
- ProblemDetails-based global exception handling
- Database startup retry
- Multi-stage Alpine Docker image
- `/healthz`, `/health/live`, and `/health/ready`
- Docker HEALTHCHECK and root `.dockerignore`
- GitHub Actions backend, frontend, native, and container jobs
- TanStack Query hooks
- Application and page-level Error Boundaries
- Skeleton and async-state components
- Husky and lint-staged
- Native safe string helpers and hardened native CI compilation

The backend is ASP.NET Core. Express-specific packages are intentionally replaced
with their production-grade ASP.NET Core equivalents.
