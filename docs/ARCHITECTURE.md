# Architecture

DevOpsHub uses a modular monolith with Clean Architecture boundaries:

- Domain: entities, enums and business rules
- Application: use cases, contracts and validation
- Infrastructure: EF Core, Identity, JWT, persistence and external adapters
- API: REST endpoints, SignalR hubs, middleware and composition root
- Web: React + TypeScript client

Runtime flow: Nginx serves the SPA and proxies `/api` and `/hubs` to ASP.NET Core. SQLite data and logs persist in named Docker volumes.
