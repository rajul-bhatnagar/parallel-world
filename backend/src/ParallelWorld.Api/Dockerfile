FROM mcr.microsoft.com/dotnet/sdk:10.0.400-bookworm-slim AS build
WORKDIR /source

COPY global.json NuGet.Config ./
COPY backend/ParallelWorld.sln backend/
COPY backend/src/ParallelWorld.Api/ParallelWorld.Api.csproj backend/src/ParallelWorld.Api/
COPY backend/src/ParallelWorld.Application/ParallelWorld.Application.csproj backend/src/ParallelWorld.Application/
COPY backend/src/ParallelWorld.Domain/ParallelWorld.Domain.csproj backend/src/ParallelWorld.Domain/
COPY backend/src/ParallelWorld.Infrastructure/ParallelWorld.Infrastructure.csproj backend/src/ParallelWorld.Infrastructure/
COPY backend/src/ParallelWorld.Simulation/ParallelWorld.Simulation.csproj backend/src/ParallelWorld.Simulation/
COPY backend/src/ParallelWorld.AI/ParallelWorld.AI.csproj backend/src/ParallelWorld.AI/

RUN dotnet restore backend/src/ParallelWorld.Api/ParallelWorld.Api.csproj --configfile NuGet.Config

COPY backend/ backend/

RUN dotnet publish backend/src/ParallelWorld.Api/ParallelWorld.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0.11-bookworm-slim AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "ParallelWorld.Api.dll"]
