# Apply

Copy the contents of this patch into the repository root and preserve paths.

Then run:

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

Production logging uses ASP.NET Core `ILogger` with Serilog JSON sinks rather than Pino/Winston because the backend is ASP.NET Core, not Express/Fastify.
