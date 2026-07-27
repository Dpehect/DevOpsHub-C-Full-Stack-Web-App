# Apply

Copy these files into the repository root and preserve paths.

Then update `Program.cs`:

```csharp
using DevOpsHub.Api.Extensions;
using DevOpsHub.Api.OpenApi;

builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<OpenApiSecurityTransformer>());

app.MapProductionHealthChecks();
```

Remove the duplicate direct `MapHealthChecks` calls after adding the extension.

Run:

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
