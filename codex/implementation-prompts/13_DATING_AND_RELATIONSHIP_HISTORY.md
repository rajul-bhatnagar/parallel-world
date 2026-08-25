# Implementation Prompt — Dating and Relationship History

```text
Read AGENTS.md, docs/milestones/M13_DATING.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Dating and Relationship History

Scope:
Implement MVP attraction inputs, romantic eligibility, date invitation, deterministic accept/reject, Dating, the PRODUCT.md-approved MVP state machine, necessary immutable invitation/outcome history, APIs, Flutter relationship view, and exhaustive transition tests. AI writes dialogue only.

Explicit exclusions:
- No breakup lifecycle, FormerPartner re-entry, reconciliation/cooldowns/cycling, commitment stage beyond Dating, engagement, marriage, separation, divorce, or children/family.

Tests:
- Exhaustively test eligibility, invitation, deterministic accept/reject, Dating, forbidden/deferred transitions, necessary immutable history, idempotency, ownership, and safe UI projection.

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
