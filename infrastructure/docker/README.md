# Local backend infrastructure

M02 provides a local PostgreSQL 18.6 service and the containerized ASP.NET Core API. The Compose file creates no tables and applies no migrations.

Set a local-only password in the current shell before using Compose. Do not put it in a committed file. Use a generated value containing only ASCII letters, digits, `.`, `_`, and `-`; connection-string delimiters such as `;` are intentionally excluded because Compose injects the password into the API's Npgsql connection string.

```powershell
$env:POSTGRES_PASSWORD = '<local-only-password>'
docker compose --file infrastructure/docker/compose.yml config --quiet
docker compose --file infrastructure/docker/compose.yml up --build
```

The API is available at `http://localhost:8080`. Its operational endpoints are:

- `GET /health/live` for process liveness.
- `GET /health/ready` for PostgreSQL readiness.
- `GET /openapi/v1.json` in the Development environment only.

Stop the services with:

```powershell
docker compose --file infrastructure/docker/compose.yml down
```
