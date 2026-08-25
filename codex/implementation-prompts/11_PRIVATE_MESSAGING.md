# Implementation Prompt — Private Messaging

```text
Read AGENTS.md, docs/milestones/M11_MESSAGES.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Private Messaging

Scope:
Implement one active direct player-character conversation, participants, persisted messages, cursor pagination, send endpoint, read cursor/state, deterministic reply eligibility/no-response, immediate eligible reply generation through the approved AI abstraction/processing flow, memory hooks without memory persistence yet, Flutter conversation list/chat/pending-failed states/cache, and tests.

Explicit exclusions:
- No simulated delay/scheduling, character-initiated messages, group chat, raw memory APIs, or AI mechanical decisions.

Tests:
- Test persisted player send, deterministic immediate reply eligibility/no-response, idempotency, pagination/read state, ownership/privacy, fallback wording, and Flutter pending/failed/cache states.

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
