# M03 — Guest session and private world

## Goal
Deliver the first owned private-world flow.

## User-visible result
Online guest creates/restores a session, creates one exposed world, and retrieves it.

## Dependencies
M02 and accepted ADR-013 M03 access-token/session-family policy.

## Scope
- **Backend:** Guest create/replay; 15-minute RS256 JWT issue/validation; opaque hash-only 30-day refresh rotation; transactional family replay containment; five-active-family policy; current-family logout and all-family revocation operation; M03 auth rate limits; world create/list/current/get; reusable world ownership policy. Public logout-all/session management and registered authentication remain M17.
- **Database:** Users, DeviceInstallations, RefreshTokens with family/device, expiry, consumption, replacement, revocation, and safe audit state, GameWorlds, WorldSettings, WorldSimulationState (`world_simulation_states`), player Actor/Profile; composite ownership constraints and migration. No character Actor or Character row is created.
- **Flutter:** None beyond contract fixtures; M04 owns UI/session implementation.
- **Infrastructure:** Local/CI PostgreSQL migration execution; protected current/previous signing-key configuration with `kid`; no committed private key, vendor-specific secret store, access-token denylist, Redis, or distributed rate-limit infrastructure.

## Explicit exclusions
Registration/password/recovery, characters, feed, multiple exposed worlds, real-user interaction.

## Test scope
Guest first/repeat/conflicting key; valid access authentication; wrong issuer/audience, expired token, invalid signature, and clock-skew boundary; refresh success/rotation/expiry; same token cannot rotate successfully twice; consumed-token replay revokes that family while another device family remains valid; current-family logout; all-family revocation; sixth active family revokes the oldest; no raw refresh persistence; no raw-token/header/cookie logging; guest `UserId`/world identity continuity for later upgrade without an M17 endpoint; auth rate-limit `429`; cross-user/session attacks; world create/replay; one player; two-user denial; cross-world constraints; clean migration.

## Security and ownership considerations
Validate only RS256 JWTs for issuer `parallel-world-api` and audience `parallel-world-mobile`, with 15-minute lifetime, 30-second skew, `sub`, `jti`, and `kid`. Load signing keys externally and verify current plus previous valid rotation key. Store only cryptographic refresh hashes, rotate transactionally, contain replay to the device/session family, and derive ownership server-side. Installation IDs and client `UserId` values never authenticate. Apply the accepted 10/IP guest, 30/family refresh, 10/IP invalid-refresh, and 30/user logout limits per 10 minutes. `WorldId`, owner queries, same-world FKs, idempotency, privacy, and secret-handling rules remain mandatory.

## Acceptance criteria
Guest and owned world persist; ADR-013 validation, rotation, replay containment, family limits/revocation, redaction, and rate limits pass; foreign user gets ownership-safe denial; retries create one effect; migration applies cleanly.

## Required verification
Backend unit/API/PostgreSQL/migration/security tests and log-redaction check. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
First guest, repeat launch token refresh, world create/current, foreign-ID attempt, logout.

## Exit criteria
Isolated guest world demonstrated and M03 tests pass.
