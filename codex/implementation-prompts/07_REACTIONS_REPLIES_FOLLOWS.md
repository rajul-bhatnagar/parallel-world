# Implementation Prompt — Reactions, Replies and Follows

```text
Read AGENTS.md, docs/milestones/M07_SOCIAL_ACTIONS.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Reactions, Replies and Follows

Scope:
Implement reactions/likes, reply creation/thread view, directional follows, cached counters with source rows, idempotent client actions, APIs, Flutter interactions, migrations, ownership validation, and tests. Exclude AI and trends.

Accepted reply-read contract (ADR-016):
- `GET /api/v1/worlds/{worldId}/posts/{postId}/replies` returns only direct child replies of that post/reply in `createdAtUtc ASC, id ASC` order through the standard cursor collection envelope.
- Its opaque `(createdAtUtc, id)` cursor seeks with `createdAtUtc > cursor.createdAtUtc`, or equal timestamp and `id > cursor.id`, and is bound to the world, parent, and applicable filters.
- `GET /api/v1/worlds/{worldId}/posts/{postId}` returns one Post/detail resource and does not embed the paginated replies collection.
- Thread traversal is explicit and parent-scoped; do not recursively nest, flatten, or preload the whole depth-two tree.
- `MAX_REPLY_DEPTH = 2` means root depth 0, reply depth 1, and reply-to-reply depth 2. Reject depth-3 creation; reading a depth-2 reply returns an empty collection.

Explicit exclusions:
- MVP like/reply/follow only; no reposts, other reaction types, mentions, trends, or AI actions.

Tests:
- Test idempotency, source rows/counters, same-world constraints, ownership negatives, API contracts, and Flutter reconciliation.
- Test direct-child-only reply reads, ascending order and equal-time `id ASC`, deterministic repetition, cursor continuation without duplicates, invalid/tampered/wrong-world/wrong-parent cursors, depths 0-2, depth-3 rejection, empty children at depth 2, and no embedded reply collection in Post detail.

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
