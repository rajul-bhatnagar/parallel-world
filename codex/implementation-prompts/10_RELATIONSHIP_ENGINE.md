# Implementation Prompt — Relationship Engine

```text
Read AGENTS.md, docs/milestones/M10_RELATIONSHIPS.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Relationship Engine

Scope:
Implement directional Relationship and immutable RelationshipEvent, trust/familiarity/respect/affection/comfort/rivalry/jealousy, clamping, idempotent deltas, derived friendship/rival/enemy states, APIs and Flutter summary/timeline basics, with deterministic tests. Exclude romance transitions.

Explicit exclusions:
- No romance transitions, dating, secrets/promises, or AI-decided relationship changes.

Tests:
- Test directional values, clamping/caps, immutable events, idempotency, derived states, same-world constraints, safe API projection, and UI.

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
