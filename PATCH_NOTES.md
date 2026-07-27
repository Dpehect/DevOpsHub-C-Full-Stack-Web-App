# DevOpsHub Production Hardening Patch

This patch closes the highest-risk production gaps:

- removes demo JWT fallback from container configuration
- adds fail-fast JWT option validation
- adds ProblemDetails-based global exception handling
- adds correlation IDs and stricter security headers
- separates liveness and readiness checks
- upgrades production database profile to PostgreSQL
- hardens Docker runtime with non-root user and cache-efficient builds
- adds CI quality gates, tests, linting, audit and image scanning
- pins frontend dependencies and adds TanStack Query/Error Boundary foundations
- adds `.env.example`, security policy and production runbook

## Apply

Copy the patch contents into the repository root, preserving paths. Then run:

```bash
dotnet restore backend/DevOpsHub.sln
dotnet build backend/DevOpsHub.sln -c Release
cd frontend
npm install
npm run typecheck
npm run build
```

Additional service-level authorization, refresh-token concurrency and tenant-isolation tests still require integration with the exact current domain models.
