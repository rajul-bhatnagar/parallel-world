# Implementation Prompt — World Events and Trends

```text
Read AGENTS.md, docs/milestones/M14_EVENTS_TRENDS.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: World Events and Trends

Gate: Version 1/deferred. Skip this numbered prompt on the core MVP path unless an accepted decision explicitly activates M14. M15 does not depend on it.

Scope:
Only after activation, implement the approved Version 1 fictional WorldEvents, trend scoring/snapshots, character reaction decisions, approved APIs/UI, scheduled template/rule generation, migrations, and deterministic tests. Seeded MVP topics are separate and do not activate this milestone.

Explicit exclusions:
- Do not execute on the core MVP path. No real-world news. Seeded MVP topics are not authorization for this full Version 1 feature.

Tests:
- Before any implementation, verify an accepted activation decision. If activated, add deterministic lifecycle/scoring, ownership, migration, API, and UI tests appropriate to the approved scope.

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
