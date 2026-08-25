# Implementation Prompt — Character Catalogue

```text
Read AGENTS.md, docs/milestones/M05_CHARACTERS.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Character Catalogue

Scope:
Reuse the M03 Actor abstraction and implement character Actors, Character records, traits, interests, opinions, deterministic seed data for ten characters, list/detail APIs, Flutter list/profile screens, cache, ownership checks, migrations, and tests. No AI generation or relationships.

Explicit exclusions:
- Do not introduce Actor; reuse M03 Actor. No AI generation, relationships, autonomous simulation, or future catalogue features.

Tests:
- Test deterministic character Actor/Character seed creation, list/detail, same-world constraints, ownership negatives, cache, and UI states.

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
