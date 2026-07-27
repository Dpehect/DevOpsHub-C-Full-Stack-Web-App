# Phase 14 — Tests, Security and Quality Gates

- Protected API controllers require authentication.
- Login/register endpoints use a fixed-window rate limiter.
- Remaining API endpoints use a per-user/IP sliding-window limiter.
- Defensive HTTP headers and restrictive API CSP are applied globally.
- SignalR message size is bounded and detailed errors are disabled outside development.
- Unit tests cover authorization metadata, security headers and input validation.
- CI treats compiler warnings as errors, uploads test coverage and audits vulnerable dependencies.
