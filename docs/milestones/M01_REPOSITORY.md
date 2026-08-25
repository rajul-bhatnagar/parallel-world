# M01 — Repository bootstrap

## Goal
Create buildable backend and Flutter shells without product behavior.

## User-visible result
None beyond a launchable empty shell.

## Dependencies
Approved planning documents; no implementation milestone.

## Required branch and workflow
- Update `dev` from the approved repository state and confirm a clean working tree.
- Implement M01 on the persistent `dev` development and integration branch.
- After implementation and all applicable M01 verification, inspect the diff, create the milestone-specific commit `feat(m01): bootstrap repository`, and push `dev`.
- A pull request is not required solely to complete M01 on `dev`.
- When `dev` reaches an approved stable checkpoint, promote it to `main` only through a reviewed `dev`-to-`main` pull request with applicable CI. A reviewed local merge is not an accepted substitute.

## Scope
- **Toolchain/platform:** .NET 10 LTS targeting `net10.0`; stable Flutter 3.47 with its bundled Dart 3.13 line; Android-first with Android API 24 minimum; iOS deferred. Select and pin exact stable compatible patch versions within these accepted lines during M01. Preview, beta, RC, dev, master, and nightly toolchains are prohibited.
- **Backend:** `ParallelWorld.Api`, `ParallelWorld.Application`, `ParallelWorld.Domain`, `ParallelWorld.Infrastructure`, `ParallelWorld.Simulation`, and `ParallelWorld.AI`, plus UnitTests, IntegrationTests, ArchitectureTests, and narrow TestUtilities projects; C# namespaces use `ParallelWorld.<Project>` and `ParallelWorld.<Project>.<Area>`; references match `ARCHITECTURE.md`.
- **Database:** None; no entities, schema, or migration. Future PostgreSQL physical identifiers use unquoted `snake_case`; C# identifiers remain PascalCase.
- **Flutter:** Application/package shell named `parallel_world_app`, Android API 24 minimum, lint/format configuration, and no feature implementation. iOS is deferred.
- **Infrastructure:** `.editorconfig`, `.gitignore`, README setup, a working initial GitHub Actions workflow containing all applicable M01 checks, and Docker directory skeleton only.

## Explicit exclusions
Entities, endpoints, auth, characters, feed, simulation, AI integration, production deployment.

## Test scope
Empty test discovery, build/reference architecture checks, Flutter baseline test.

## Security and ownership considerations
Project references, dependency footprint, empty feature scope. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
The exact stable compatible patches are pinned within the accepted .NET 10/`net10.0` and Flutter 3.47/Dart 3.13 lines; the Android shell targets API 24 minimum, remains Android-first, and adds no iOS requirement. Project, namespace, Flutter-package, and future PostgreSQL naming follow the required conventions. Backend restore/configured format check/build/test execution and Flutter pub get/format/analyze/tests succeed locally and in the working initial GitHub Actions workflow. No domain entity, database schema, migration, auth code, or product feature exists.

## Required verification
`dotnet restore`, configured backend format check, `dotnet build`, `dotnet test`; `flutter pub get`, Dart format check, `flutter analyze`, `flutter test`; workflow configuration syntax. PostgreSQL migrations, PostgreSQL integration tests, and schema verification are **Not applicable — M01 creates no database schema**. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Clone/setup instructions work on a clean machine or clean workspace.

## Exit criteria
All acceptance criteria and applicable local/CI checks pass; the diff has no unresolved Critical/High finding; and the milestone-specific commit is pushed to `dev`. That completes M01 on `dev`. Stable/release-ready promotion to `main` is separate and occurs only at an approved checkpoint through a reviewed `dev`-to-`main` pull request with applicable CI; a reviewed local merge is insufficient.
