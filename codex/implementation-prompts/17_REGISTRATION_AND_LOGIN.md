# Implementation Prompt — Registration and Login

```text
Read AGENTS.md, docs/milestones/M17_AUTH.md, docs/product/PRODUCT.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Registration and Login

Prerequisite: Resolve and record the accepted registered-authentication and recovery approach in docs/development/DECISIONS.md before implementation.

Scope:
Implement guest account upgrade, registration/login/recovery using the selected accepted approach, refresh rotation, logout/revoke, Flutter auth screens, preservation of the same UserId and all existing worlds/history, and security/integration tests. Single-player isolation remains unchanged.

Explicit exclusions:
- Do not implement until registered authentication and recovery are accepted in DECISIONS.md. Do not assume email/password, password reset, or a provider.

Tests:
- Test the selected method, recovery, enumeration/privacy defenses, transactional same-UserId guest upgrade, preservation of all worlds/history, rollback/concurrency, isolation, and Flutter auth flows.

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
