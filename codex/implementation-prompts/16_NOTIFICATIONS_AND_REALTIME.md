# Implementation Prompt — Notifications and Realtime

```text
Read AGENTS.md, docs/milestones/M16_NOTIFICATIONS.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Notifications and Realtime

Scope:
Implement persisted Reply, PrivateMessage, and CatchUpSummary in-app notification intents, deduplication, read/unread state, HTTP count/read and a bounded cursor-paginated minimal list for resolving badge/deep-link indicators, HTTP synchronization/refetch, and matching Flutter states. SignalR is included only if an accepted decision activates it; otherwise HTTP remains the baseline. Do not implement Follow/DatingInvitation indicators, rich history/filtering/search, or push/FCM.

Explicit exclusions:
- No push/FCM. No SignalR unless separately accepted and recorded. No rich/deferred notification categories.

Tests:
- Test persisted in-app intent, deduplication, unread/read, HTTP list/count/read/refetch, ownership/privacy, and Flutter states. Test SignalR only if activated.

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
