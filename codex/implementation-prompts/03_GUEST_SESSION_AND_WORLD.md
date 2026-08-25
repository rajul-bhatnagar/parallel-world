# Implementation Prompt — Guest Session and Private World

```text
Read AGENTS.md, docs/milestones/M03_SESSION_WORLD.md, docs/product/PRODUCT.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Guest Session and Private World

Scope:
Implement transactional User, DeviceInstallation, RefreshToken, GameWorld, player Actor, PlayerProfile, WorldSettings, WorldSimulationState, ownership resolution, EF configurations/migration, guest session endpoint, token issuance/rotation, world creation/current-world endpoint, and isolation integration tests. No character Actors, Character records, or posts.

Explicit exclusions:
- No AI characters, posts, feed, relationships, or later registered authentication.

Tests:
- Test transactional User -> World -> player Actor -> PlayerProfile -> WorldSettings -> WorldSimulationState creation, retry/idempotency, token rotation, and two-user/two-world isolation.

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
