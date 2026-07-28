# Apply

Copy all files into the repository root, preserving paths.

```bash
cd frontend
npm install
npm run check
npm run build

cd ../backend
dotnet restore DevOpsHub.sln
dotnet build DevOpsHub.sln -c Release
dotnet test DevOpsHub.sln -c Release
```

The project backend is ASP.NET Core. Therefore FluentValidation, ASP.NET Core rate limiting,
security-header middleware and Serilog are used instead of Express/Fastify, Zod/Joi,
express-rate-limit, Helmet and Pino/Winston.
