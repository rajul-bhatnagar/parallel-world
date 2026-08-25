# Implementation Prompt — Reactions, Replies and Follows

```text
Read AGENTS.md, docs/milestones/M07_SOCIAL_ACTIONS.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Reactions, Replies and Follows

Scope:
Implement reactions/likes, reply creation/thread view, directional follows, cached counters with source rows, idempotent client actions, APIs, Flutter interactions, migrations, ownership validation, and tests. Exclude AI and trends.

Explicit exclusions:
- MVP like/reply/follow only; no reposts, other reaction types, mentions, trends, or AI actions.

Tests:
- Test idempotency, source rows/counters, same-world constraints, ownership negatives, API contracts, and Flutter reconciliation.

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
