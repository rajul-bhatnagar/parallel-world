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

M01 adds buildable backend and Android application shells only. Product behavior is added one milestone at a time after the planning documents have been reviewed and accepted.

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
4. From the repository root, restore and verify the backend:

   ```bash
   dotnet restore backend/ParallelWorld.sln --configfile NuGet.Config
   dotnet format backend/ParallelWorld.sln --verify-no-changes --no-restore
   dotnet build backend/ParallelWorld.sln --configuration Release --no-restore
   dotnet test backend/ParallelWorld.sln --configuration Release --no-build
   ```

5. Restore and verify the Flutter shell:

   ```bash
   cd mobile/parallel_world_app
   flutter pub get
   dart format --output=none --set-exit-if-changed .
   flutter analyze
   flutter test
   flutter run
   ```

The API project is an intentionally empty executable shell with no endpoints. The Docker directory is also a placeholder: PostgreSQL, Compose, migrations, and containerized backend startup are not part of M01.

## Bootstrap dependencies

- Backend test projects use `Microsoft.NET.Test.Sdk`, xUnit, the Visual Studio xUnit runner, and `coverlet.collector`, supplied by the .NET 10 xUnit template, for test discovery and future coverage collection.
- The Flutter app uses only the Flutter SDK at runtime. `flutter_test` supplies the baseline widget test and `flutter_lints` 6.0.0 supplies the configured static-analysis rules.

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
