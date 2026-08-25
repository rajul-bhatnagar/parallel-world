# Implementation Prompt — Flutter Foundation

```text
Read AGENTS.md, docs/milestones/M04_FLUTTER_FOUNDATION.md, docs/product/PRODUCT.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Flutter Foundation

Scope:
Implement environment config, Dio client, access/refresh handling, secure installation ID and token storage, Riverpod session state, splash/bootstrap, retry UI, world bootstrap navigation, and foundational Drift database. Add unit/widget tests. No feed.

Explicit exclusions:
- No feed or later feature UI; Drift remains non-authoritative.

Tests:
- Add unit/widget tests for bootstrap, session refresh, secure storage abstraction, navigation guards, and failure/offline states.

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
