# Implementation Prompt — AI Text Generation

```text
Read AGENTS.md, docs/milestones/M09_AI.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: AI Text Generation

Scope:
Implement IAiTextGenerator, request/result models, prompt builder, template fallback, one external provider adapter, timeout/retry/output validation/duplicate detection, budget controls, safe logging, generation diagnostics, fake-provider tests and stub integration tests. AI may write wording only.

Explicit exclusions:
- No provider/model choice until recorded before M09; AI must not choose or mutate mechanics; no future AI features.

Tests:
- Use fake/stub providers for automated tests covering validation, retry classification, fallback, budgets, duplicate detection, redaction, and mechanical invariance.

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
