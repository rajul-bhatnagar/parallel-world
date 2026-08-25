# M02 — Backend foundation

## Goal
Create a production-shaped ASP.NET Core host without game entities.

## User-visible result
Operational health response; no gameplay.

## Dependencies
M01.

## Scope
- **Backend:** Startup/composition, validated configuration, ProblemDetails/global exception handling, Serilog redaction, correlation IDs, OpenAPI, liveness/readiness, PostgreSQL DbContext registration, approved transaction abstraction.
- **Database:** Configurable PostgreSQL connectivity; no gameplay schema. A migration is Not applicable unless a reviewed foundation schema is introduced.
- **Flutter:** None beyond any safe development base-URL compatibility check.
- **Infrastructure:** API Dockerfile and local PostgreSQL Compose.

## Explicit exclusions
Users, worlds, actors, feed, simulation, AI/provider behavior.

## Test scope
Startup, health, sanitized ProblemDetails, missing-config failure, project dependency rules, PostgreSQL connectivity.

## Security and ownership considerations
Security defaults, exception boundary, dependency direction, no game entities. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
API starts, health is safe, PostgreSQL is configurable, logs/errors reveal no secrets, architecture tests pass.

## Required verification
Backend restore/format/build/test, Compose config, local health and configuration-failure checks. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Start/stop API/PostgreSQL; inspect health, correlation ID, and redacted failure.

## Exit criteria
Production-shaped host verified with no feature scope.
