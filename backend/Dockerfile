FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore DevOpsHub.sln
RUN dotnet publish src/DevOpsHub.Api/DevOpsHub.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
RUN mkdir -p /app/data /app/logs
EXPOSE 8080
ENTRYPOINT ["dotnet", "DevOpsHub.Api.dll"]
