FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY DevOpsHub.sln ./
COPY src/DevOpsHub.Api/DevOpsHub.Api.csproj src/DevOpsHub.Api/
COPY src/DevOpsHub.Application/DevOpsHub.Application.csproj src/DevOpsHub.Application/
COPY src/DevOpsHub.Domain/DevOpsHub.Domain.csproj src/DevOpsHub.Domain/
COPY src/DevOpsHub.Infrastructure/DevOpsHub.Infrastructure.csproj src/DevOpsHub.Infrastructure/
COPY tests/DevOpsHub.Tests/DevOpsHub.Tests.csproj tests/DevOpsHub.Tests/

RUN dotnet restore DevOpsHub.sln

FROM restore AS build
COPY . .
RUN dotnet publish src/DevOpsHub.Api/DevOpsHub.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0 \
    COMPlus_EnableDiagnostics=0

RUN mkdir -p /app/data /app/logs \
    && chown -R app:app /app

COPY --from=build --chown=app:app /app/publish .

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "DevOpsHub.Api.dll"]
