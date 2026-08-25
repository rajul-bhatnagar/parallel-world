# Implementation Prompt — Backend Foundation

```text
Read AGENTS.md, docs/milestones/M02_BACKEND_FOUNDATION.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Backend Foundation

Scope:
Implement API startup, dependency registration, validated configuration, ProblemDetails/global exception handling, Serilog, health checks, OpenAPI, PostgreSQL DbContext registration, Dockerfile, and local PostgreSQL compose. Add startup, health, and error-shape tests. No game entities.

Explicit exclusions:
- No game entities, feature endpoints, authentication implementation, or Flutter feature work.

Tests:
- Add startup, health, configuration, and ProblemDetails tests; run applicable backend checks.

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
- Suggested commit message
```
