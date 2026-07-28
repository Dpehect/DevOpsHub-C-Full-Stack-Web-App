# Apply V3

Copy all files into the repository root while preserving paths.

In `Program.cs`:

```csharp
using DevOpsHub.Api.OpenApi;

builder.Services.AddProductionRateLimiting();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
});
```

Remove the inline `AddRateLimiter` block after registering
`AddProductionRateLimiting()`.

Install and validate:

```bash
cd frontend
npm install
npm run check
npm run build

cd ../backend
dotnet restore DevOpsHub.sln
dotnet build DevOpsHub.sln -c Release
dotnet test DevOpsHub.sln -c Release
docker build -t devopshub-api .
```
