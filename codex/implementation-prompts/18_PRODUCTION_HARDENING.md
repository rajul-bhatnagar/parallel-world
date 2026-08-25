# Implementation Prompt — Production Hardening

```text
Read AGENTS.md, docs/milestones/M18_PRODUCTION.md, docs/product/PRODUCT.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Production Hardening

Scope:
Add deployment configuration, CI gates, migration execution strategy, observability, health/readiness, rate limits, AI budgets, retry/circuit policies, backup/restore documentation, performance indexes, security headers, release build configuration, privacy controls, and production checklist. Do not add microservices.

Explicit exclusions:
- No new product features, microservices, unselected hosting provider, deferred M14/M17 features, push, or mandatory realtime.

Tests:
- Run all applicable release gates for selected milestones, including security, migrations, backup/restore, observability, performance measurements, Flutter release checks, and documentation.

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
