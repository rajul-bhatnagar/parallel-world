# Implementation Prompt — Social Feed

```text
Read AGENTS.md, docs/milestones/M06_FEED.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Social Feed

Scope:
Implement Posts with player/character/system authors, replies via ParentPostId, cursor feed API, player compose API, seed posts, Flutter feed/compose/pagination/cache/error states, migration and tests. Exclude reactions/reposts/hashtags.

Explicit exclusions:
- No reactions, reposts, hashtags, mentions, full trends, or unapproved ranking choice.

Tests:
- Test cursor feed, authorship/ownership, same-world parent references, idempotent compose, cache/pagination, and UI states.

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
