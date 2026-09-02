# Local backend infrastructure

The local stack provides PostgreSQL 18.6 and the containerized ASP.NET Core API. M03 migrations are applied explicitly rather than during API startup.

Set a local-only password in the current shell before using Compose. Do not put it in a committed file. Use a generated value containing only ASCII letters, digits, `.`, `_`, and `-`; connection-string delimiters such as `;` are intentionally excluded because Compose injects the password into the API's Npgsql connection string.

```powershell
$env:POSTGRES_PASSWORD = '<local-only-password>'
$jwtKey = [System.Security.Cryptography.RSA]::Create(2048)
$env:JWT_CURRENT_KEY_ID = 'local-development-key'
$env:JWT_CURRENT_PRIVATE_KEY_PEM = $jwtKey.ExportPkcs8PrivateKeyPem()
$env:JWT_CURRENT_PUBLIC_KEY_PEM = $jwtKey.ExportSubjectPublicKeyInfoPem()
docker compose --file infrastructure/docker/compose.yml config --quiet
docker compose --file infrastructure/docker/compose.yml up --detach postgres
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=parallel_world;Username=parallel_world;Password=$env:POSTGRES_PASSWORD"
dotnet tool restore
dotnet tool run dotnet-ef database update --project backend/src/ParallelWorld.Infrastructure/ParallelWorld.Infrastructure.csproj --startup-project backend/src/ParallelWorld.Infrastructure/ParallelWorld.Infrastructure.csproj
docker compose --file infrastructure/docker/compose.yml up --build
```

The API is available at `http://localhost:8080`. Its operational endpoints are:

- `GET /health/live` for process liveness.
- `GET /health/ready` for PostgreSQL readiness.
- `GET /openapi/v1.json` in the Development environment only.
- `POST /api/v1/auth/guest`, `/refresh`, and authenticated `/logout` for M03 sessions.
- `POST/GET /api/v1/worlds` plus `GET /api/v1/worlds/current` and `GET /api/v1/worlds/{worldId}` for the isolated M03 world.

Stop the services with:

```powershell
docker compose --file infrastructure/docker/compose.yml down
```
