# DevOpsHub Production Hardening V4

Copy the archive contents into the repository root and preserve paths.

The backend is ASP.NET Core, so the production equivalents are:

- FluentValidation instead of backend Zod/Joi
- ASP.NET Core Rate Limiting instead of express-rate-limit
- SecurityHeadersMiddleware instead of Helmet
- Serilog compact JSON instead of Pino/Winston
- IExceptionHandler and ProblemDetails for centralized async error handling

Frontend input and response validation uses Zod.
