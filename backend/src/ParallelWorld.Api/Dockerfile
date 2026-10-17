FROM mcr.microsoft.com/dotnet/sdk:10.0.402-noble AS build
WORKDIR /source

COPY global.json NuGet.Config ./
COPY .config/dotnet-tools.json .config/
COPY backend/ParallelWorld.sln backend/
COPY backend/src/ParallelWorld.Api/ParallelWorld.Api.csproj backend/src/ParallelWorld.Api/
COPY backend/src/ParallelWorld.Application/ParallelWorld.Application.csproj backend/src/ParallelWorld.Application/
COPY backend/src/ParallelWorld.Domain/ParallelWorld.Domain.csproj backend/src/ParallelWorld.Domain/
COPY backend/src/ParallelWorld.Infrastructure/ParallelWorld.Infrastructure.csproj backend/src/ParallelWorld.Infrastructure/
COPY backend/src/ParallelWorld.Simulation/ParallelWorld.Simulation.csproj backend/src/ParallelWorld.Simulation/
COPY backend/src/ParallelWorld.AI/ParallelWorld.AI.csproj backend/src/ParallelWorld.AI/

RUN dotnet restore backend/src/ParallelWorld.Api/ParallelWorld.Api.csproj --configfile NuGet.Config
RUN dotnet tool restore

COPY backend/ backend/

RUN dotnet publish backend/src/ParallelWorld.Api/ParallelWorld.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

RUN ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__Default="Host=localhost;Database=parallel_world_bundle;Username=postgres" \
    dotnet tool run dotnet-ef migrations bundle \
    --project backend/src/ParallelWorld.Infrastructure/ParallelWorld.Infrastructure.csproj \
    --startup-project backend/src/ParallelWorld.Infrastructure/ParallelWorld.Infrastructure.csproj \
    --output /app/migrate

FROM mcr.microsoft.com/dotnet/aspnet:10.0.13-noble AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get upgrade -y \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
COPY --from=build --chmod=755 /app/migrate /app/migrate

RUN usermod --append --groups 1000 app

USER $APP_UID
ENTRYPOINT ["dotnet", "ParallelWorld.Api.dll"]
