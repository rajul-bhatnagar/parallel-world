# Implementation Prompt — Rule-Based Simulation

```text
Read AGENTS.md, docs/milestones/M08_SIMULATION.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Rule-Based Simulation

Scope:
Implement SimulationRun, SimulationAction, deterministic random provider, world clock, activity selector, decision creation/execution separation, template text generator, create-post/reply/like/follow actions, development trigger, idempotency and transaction tests. No external AI, messaging, memory, or dating.

Explicit exclusions:
- No external AI, messaging, memory, romance, catch-up, or deferred events/trends.

Tests:
- Test deterministic seeds/order, interval uniqueness, idempotency, concurrency, transaction boundaries, fallback wording, and cross-world rejection.

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
