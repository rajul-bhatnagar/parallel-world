# Implementation Prompt — Repository Bootstrap

```text
Read AGENTS.md, README.md, docs/product/PRODUCT.md, docs/architecture/ARCHITECTURE.md, docs/development/DECISIONS.md, docs/development/DEVELOPMENT_PLAN.md, docs/milestones/M01_REPOSITORY.md, codex/reference/NAMING_CONVENTIONS.md, codex/reference/DEFINITION_OF_DONE.md, and codex/reference/VERIFICATION_COMMANDS.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Repository Bootstrap

Scope:
Confirm the current branch is exactly `dev`; stop and report the mismatch rather than implementing on `main` or another branch unless the user explicitly selected an optional short-lived branch for isolated or risky work. Use .NET 10 LTS targeting `net10.0` and the accepted stable Flutter 3.47 line with bundled Dart 3.13. During M01, select and pin the current exact stable compatible patch versions within those accepted lines; preview, beta, RC, development-channel, master, and nightly toolchains are prohibited. Target Android first with Android API 24 minimum and keep iOS deferred.

Create the .NET solution and projects `ParallelWorld.Api`, `ParallelWorld.Application`, `ParallelWorld.Domain`, `ParallelWorld.Infrastructure`, `ParallelWorld.Simulation`, and `ParallelWorld.AI`, with namespaces following `ParallelWorld.<Project>` and `ParallelWorld.<Project>.<Area>`. Create the Flutter app/package as `parallel_world_app`. Use PascalCase C# identifiers and preserve the accepted future PostgreSQL convention of unquoted `snake_case` physical identifiers without creating a schema.

Create the repository folders, `.gitignore`, `.editorconfig`, README setup instructions, Docker development folder, and a working initial GitHub Actions workflow containing every check applicable at M01. Include backend restore, configured format check, build, and test execution plus Flutter `flutter pub get`, format check, `flutter analyze`, and `flutter test`. Do not add domain entities, authentication, migrations, schema, or PostgreSQL integration jobs. Verify the created shells and workflow configuration.

Explicit exclusions:
- No domain entities, authentication, database schema, migrations, PostgreSQL integration job, iOS delivery requirement, or future milestone feature.
- Do not push, open a pull request, or merge unless the user explicitly asks for those Git operations. Normal M01 completion is a milestone-specific commit pushed to `dev`; stable promotion is a separate `dev`-to-`main` pull request at an approved checkpoint.

Tests:
- Verify only created shells/tooling: backend restore/configured format check/build/test execution and Flutter pub get/format/analyze/tests, locally and through the working initial GitHub Actions workflow.
- Report PostgreSQL migrations, PostgreSQL integration tests, and schema verification as `Not applicable — M01 creates no database schema`.

Before editing:
1. List relevant existing files.
2. Provide a short plan.
3. State assumptions and risks.

Requirements:
- Implement only this task.
- Do not implement future milestones.
- Enforce user/world ownership where applicable.
- Add migrations only for schema changes introduced by this milestone; otherwise report Not applicable.
- Add or update automated tests.
- Do not add secrets.
- Explain any package added.
- Update documentation when behaviour changes.
- M01 is complete on `dev` when its acceptance criteria and verification pass, its diff is reviewed with no unresolved Critical/High finding, and its milestone-specific commit is pushed to `dev`. A pull request is not required solely for M01 completion. Promotion to stable/release-ready `main` requires an approved-checkpoint pull request from `dev`, applicable CI, review, and merge through that pull request; a reviewed local merge is not an accepted alternative.

Verification:
- Run only milestone-applicable formatting, builds/analyzers, tests, migration checks, and manual checks.
- Do not invent paths, projects, tools, or checks that an earlier milestone has not created.
- Report every command/check as Passed, Failed, Unavailable, or Not applicable — with reason.

Completion report:
- Summary
- Changed files
- Important decisions
- Tests and results
- Manual verification
- Remaining risks
- Suggested milestone commit message: `feat(m01): bootstrap repository`
- Confirmation that no application feature, schema, migration, PostgreSQL integration job, or iOS delivery requirement was added
- The remaining user-controlled workflow: review the implementation and diff, then explicitly authorize or perform the milestone-specific commit and push to `dev`; at an approved stable checkpoint, separately authorize or perform the `dev`-to-`main` pull request, applicable CI review, pull-request review, and merge
```
