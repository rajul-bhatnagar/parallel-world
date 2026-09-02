# Parallel World

Parallel World is a private, single-player AI social-life simulation game. Each account owns isolated game worlds containing one human-controlled player and AI-controlled characters; real users never interact with or influence one another.

## Engineering principle

Simulation rules decide what happens, PostgreSQL records what happened, and AI decides how characters express it.

## Repository governance

Implementation is governed by the authoritative documents in this repository:

- `AGENTS.md` defines project-wide engineering instructions.
- `docs/product/PRODUCT.md` defines product scope and MVP boundaries.
- `docs/game-design/GAME_RULES.md` defines simulation behaviour.
- `docs/architecture/` defines architecture, data, API, and security rules.
- `docs/development/` defines testing and records accepted decisions.
- `docs/milestones/` defines milestone acceptance criteria.

The files under `codex/` are ordered working prompts, templates, and references. They instruct development work but do not replace the authoritative documents above.

If repository guidance conflicts with an accepted source-of-truth document, stop and resolve the conflict before implementation. Do not silently invent a product rule, game mechanic, or architecture decision.

## Application areas

- `backend/` — ASP.NET Core modular-monolith backend
- `mobile/` — Flutter mobile application
- `infrastructure/` — local and production infrastructure

M01 adds buildable backend and Android application shells. M02 adds the production-shaped backend host and configurable PostgreSQL connectivity. M03 adds proof-bound guest sessions and the first isolated private world. M04 adds the Android client foundation for secure guest bootstrap, session recovery, owned-world loading, and an explicitly offline cached view; later gameplay remains milestone-gated.

## Pinned toolchain

- .NET SDK 10.0.400 with projects targeting `net10.0`
- Flutter 3.47.1 stable with bundled Dart 3.13.1
- Android API 24 minimum
- Android is the initial platform; iOS remains deferred

`global.json` pins .NET exactly and `mobile/parallel_world_app/.fvmrc` pins Flutter exactly. Preview, beta, release-candidate, development, master/main, and nightly SDKs are not supported.

## Local setup

1. Install the .NET SDK version from `global.json`.
2. Install Flutter 3.47.1 stable directly or through FVM, and confirm `flutter --version` reports Dart 3.13.1.
3. Install the Android SDK and accept its licenses. The generated Android application supports API 24 and newer.
4. Provide an isolated PostgreSQL connection through `ConnectionStrings__Default`. For example, start PostgreSQL and configure the current shell:

   Use a generated local-only PostgreSQL password containing only ASCII letters, digits, `.`, `_`, and `-`. Compose injects this value into an Npgsql connection string, so connection-string delimiters such as `;` are intentionally excluded.

   ```powershell
   $env:POSTGRES_PASSWORD = '<local-only-password>'
   docker compose --file infrastructure/docker/compose.yml up --detach postgres
   $env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=parallel_world;Username=parallel_world;Password=$env:POSTGRES_PASSWORD"
   ```

5. Generate an uncommitted local RSA key and expose the M03 signing configuration in the current PowerShell session:

   ```powershell
   $jwtKey = [System.Security.Cryptography.RSA]::Create(2048)
   $env:JWT_CURRENT_KEY_ID = 'local-development-key'
   $env:JWT_CURRENT_PRIVATE_KEY_PEM = $jwtKey.ExportPkcs8PrivateKeyPem()
   $env:JWT_CURRENT_PUBLIC_KEY_PEM = $jwtKey.ExportSubjectPublicKeyInfoPem()
   $env:Authentication__CurrentKeyId = $env:JWT_CURRENT_KEY_ID
   $env:Authentication__CurrentPrivateKeyPem = $env:JWT_CURRENT_PRIVATE_KEY_PEM
   $env:Authentication__CurrentPublicKeyPem = $env:JWT_CURRENT_PUBLIC_KEY_PEM
   ```

   Keep these values outside the repository. A previous public key may additionally be supplied through `Authentication__PreviousKeyId` and `Authentication__PreviousPublicKeyPem` during a controlled rotation.

6. From the repository root, restore tools and dependencies, apply the M03 migration, and verify the backend:

   ```powershell
   dotnet tool restore
   dotnet restore backend/ParallelWorld.sln --configfile NuGet.Config
   dotnet tool run dotnet-ef database update --project backend/src/ParallelWorld.Infrastructure/ParallelWorld.Infrastructure.csproj --startup-project backend/src/ParallelWorld.Infrastructure/ParallelWorld.Infrastructure.csproj
   dotnet format backend/ParallelWorld.sln --verify-no-changes --no-restore
   dotnet build backend/ParallelWorld.sln --configuration Release --no-restore
   $env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=parallel_world_tests;Username=parallel_world;Password=$env:POSTGRES_PASSWORD"
   dotnet test backend/ParallelWorld.sln --configuration Release --no-build
   ```

   PostgreSQL integration tests require `ConnectionStrings__Default` to name the test-specific
   administrative base `parallel_world_tests` (the base database need not already exist). Each
   test fixture connects through PostgreSQL's administrative database, then creates and later drops
   only a generated database named
   `parallel_world_test_<32 lowercase hex characters>`. A hard guard rejects every other database
   name before destructive lifecycle operations, so the normal `parallel_world` database is never
   an integration-test deletion target. The configured PostgreSQL role must be allowed to create
   and drop these temporary test databases.

7. Restore and verify the Flutter application. `API_BASE_URL` is a required compile-time value; `10.0.2.2` is the Android emulator alias for the development host:

   ```bash
   cd mobile/parallel_world_app
   flutter pub get
   dart format --output=none --set-exit-if-changed .
   flutter analyze
   flutter test
   flutter run --dart-define=API_BASE_URL=http://10.0.2.2:8080 --dart-define=APP_ENVIRONMENT=local
   ```

The API exposes M03 guest session endpoints under `/api/v1/auth` and owned-world endpoints under `/api/v1/worlds`, alongside `/health/live`, `/health/ready`, and Development-only `/openapi/v1.json`. It requires the PostgreSQL connection and current RSA signing key configuration at startup. Store connection strings and private keys in local environment variables or a deployment secret manager; never commit them.

For local PostgreSQL and the containerized API:

```powershell
$env:POSTGRES_PASSWORD = '<local-only-password>'
docker compose --file infrastructure/docker/compose.yml config --quiet
docker compose --file infrastructure/docker/compose.yml up --build
```

For direct local API execution against that PostgreSQL instance:

```powershell
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=parallel_world;Username=parallel_world;Password=$env:POSTGRES_PASSWORD"
dotnet run --project backend/src/ParallelWorld.Api/ParallelWorld.Api.csproj
```

Stop the containerized services with `docker compose --file infrastructure/docker/compose.yml down`.

## Bootstrap and mobile-foundation dependencies

- Backend test projects use `Microsoft.NET.Test.Sdk`, xUnit, the Visual Studio xUnit runner, and `coverlet.collector`, supplied by the .NET 10 xUnit template, for test discovery and future coverage collection.
- `flutter_riverpod` 3.4.2 and `go_router` 18.0.0 provide explicit client state and navigation boundaries.
- Dio 5.11.0 provides the HTTP pipeline, including sanitized request diagnostics and single-flight authenticated retry handling.
- `flutter_secure_storage` 10.0.0 stores session and bootstrap-recovery secrets in platform-protected storage.
- Drift 2.34.3 with `drift_flutter` 0.3.1 stores only the user-scoped cached world projection. `drift_dev` 2.34.5 and `build_runner` 2.15.3 generate the checked-in database code.
- `flutter_test` supplies unit and widget testing, and `flutter_lints` 6.0.0 supplies the configured static-analysis rules.

## Backend foundation dependencies

- EF Core 10.0.11 and `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 provide the PostgreSQL model and migration. The API pins `Microsoft.EntityFrameworkCore.Relational` 10.0.11 directly so its runtime graph matches the accepted EF patch level.
- `Microsoft.EntityFrameworkCore.Design` 10.0.11 and repository-local `dotnet-ef` 10.0.11 provide migration generation and verification.
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.11 provides allowlisted RS256 access-token validation.
- `Microsoft.AspNetCore.OpenApi` 10.0.11 provides framework-native OpenAPI generation.
- `Serilog.AspNetCore` 10.0.0 provides structured console and request logging; sensitive property names are redacted before sinks.
- `Microsoft.AspNetCore.Mvc.Testing` 10.0.11 hosts the real API pipeline in integration tests.

## Development workflow

1. Read `AGENTS.md` and the relevant source-of-truth documents.
2. Follow `codex/reference/start-here/01_PROMPT_ORDER.md` in sequence.
3. Review and approve planning outputs before implementation.
4. Update the persistent `dev` branch from the approved repository state, confirm a clean working tree, and perform milestone work on `dev`.
5. Implement only the current milestone or explicitly requested task.
6. Run all applicable checks in `codex/reference/VERIFICATION_COMMANDS.md` and the current milestone checklist.
7. Inspect the diff, record unresolved risks, and commit a self-contained change.
8. Push `dev`. A pull request is not required after every milestone.
9. When `dev` reaches an approved stable checkpoint, open a pull request from `dev` into `main`, require all applicable CI checks to succeed, and review the pull request, including the required independent Codex diff review.
10. Merge the reviewed `dev`-to-`main` pull request only after the applicable CI and review gates pass.

A milestone is complete on `dev` when its acceptance criteria and verification pass, its diff is reviewed, and its milestone-specific commit is pushed to `dev`. Promotion to stable/release-ready `main` is a separate checkpoint: it requires a reviewed `dev`-to-`main` pull request with applicable CI. A reviewed local merge is not an accepted substitute for that promotion pull request. Direct feature development on `main` is not allowed. Branch and commit naming rules are documented in `codex/reference/NAMING_CONVENTIONS.md`.
