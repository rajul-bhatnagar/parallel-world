# M03 — Guest session and private world

## Goal
Deliver the first owned private-world flow.

## User-visible result
Online guest creates/restores a session, creates one exposed world, and retrieves it.

## Dependencies
M02 and resolved token-format/lifetime decisions required for implementation.

## Scope
- **Backend:** Guest create/replay, minimal access token, rotating hashed refresh token, logout basics, world create/list/current/get, reusable world ownership policy.
- **Database:** Users, DeviceInstallations, RefreshTokens, GameWorlds, WorldSettings, WorldSimulationState (`world_simulation_states`), player Actor/Profile; composite ownership constraints and migration. No character Actor or Character row is created.
- **Flutter:** None beyond contract fixtures; M04 owns UI/session implementation.
- **Infrastructure:** Local/CI PostgreSQL migration execution; protected signing configuration names.

## Explicit exclusions
Registration/password/recovery, characters, feed, multiple exposed worlds, real-user interaction.

## Test scope
Guest first/repeat/conflicting key, refresh/concurrency/reuse baseline, world create/replay, one player, two-user denial, cross-world constraints, clean migration.

## Security and ownership considerations
Token storage/rotation, `WorldId`, owner queries, same-world FKs, idempotency. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Guest and owned world persist; foreign user gets ownership-safe denial; retries create one effect; migration applies cleanly.

## Required verification
Backend unit/API/PostgreSQL/migration/security tests and log-redaction check. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
First guest, repeat launch token refresh, world create/current, foreign-ID attempt, logout.

## Exit criteria
Isolated guest world demonstrated and M03 tests pass.
