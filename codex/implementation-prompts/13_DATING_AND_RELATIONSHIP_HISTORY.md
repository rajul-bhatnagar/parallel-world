# Implementation Prompt — Dating and Relationship History

```text
Read AGENTS.md, docs/milestones/M13_DATING.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Dating and Relationship History

Scope:
Implement ADR-030 exactly: structured world/profile romance availability, ROM-01/02 neutral inputs and reason precedence, strict Actor-level Dating/pending-invitation exclusivity, canonical `CasualDate`, 14-game-day cooldown, 24-game-hour expiry, explicit Player accept/reject, immediate deterministic Character outcome, shared Dating state, bilateral replay-safe Commitment +10, immutable invitation/status history with UTC audit and world-game time, APIs, Flutter relationship view, and exhaustive transition tests. AI writes dialogue only after authoritative outcomes.

Explicit exclusions:
- No breakup/Dating-end lifecycle, FormerPartner re-entry, re-dating, reconciliation/cooldowns/cycling, commitment stage beyond Dating, extra preference modes/date types, engagement, marriage, separation, divorce, or children/family.

Tests:
- Exhaustively test romance enabled/modes/compatibility, exact neutral inputs and reason precedence, thresholds, invitation/cooldown/expiry, deterministic accept/reject, Player and Character targets, strict exclusivity and competing concurrency, `CasualDate`, Dating, bilateral Commitment +10/clamp/one-time replay, forbidden/deferred transitions, immutable history including persistence-level preservation of seeded distinct ended/re-dated episodes without activating those transitions, idempotency, ownership, AI/text/memory invariance, and safe UI projection.

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
