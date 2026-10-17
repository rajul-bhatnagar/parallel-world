# M03 — Guest session and private world

## Goal
Deliver the first owned private-world flow.

## User-visible result
Online guest creates/restores a session, creates one exposed world, and retrieves it.

## Dependencies
M02 and accepted ADR-013/ADR-014 M03 access-token, session-family, guest-bootstrap, and recovery policy.

## Scope
- **Backend:** Proof-bound guest bootstrap that transactionally creates one User/session/world/player result; one new-credential recovery within 10 minutes; 15-minute RS256 JWT issue/validation; deliberately non-idempotent opaque hash-only 30-day refresh rotation; transactional family replay containment; five-active-family policy; current-family logout and all-family revocation operation; M03 auth rate limits; world list/current/get and ownership-safe handling of the already-created MVP world; reusable world ownership policy. Public logout-all/session management and registered authentication remain M17.
- **Database:** Users, DeviceInstallations, hash-only GuestBootstrapOperations with one bounded recovery, RefreshTokens with family/device, expiry, consumption, replacement, revocation, and safe audit state, GameWorlds, WorldSettings, WorldSimulationState (`world_simulation_states`), player Actor/Profile; composite ownership constraints and migration. No character Actor or Character row is created.
- **Flutter:** None beyond contract fixtures; M04 owns UI/session implementation.
- **Infrastructure:** Local/CI PostgreSQL migration execution; protected current/previous signing-key configuration with `kid`; no committed private key, vendor-specific secret store, access-token denylist, Redis, or distributed rate-limit infrastructure.

## Explicit exclusions
Registration/password/registered-account recovery, characters, feed, multiple exposed worlds, real-user interaction.

## Test scope
First proof bootstrap and same-proof identity retry; one recovery returns new credentials for the same User/world/session lineage; 10-minute expiry, one-recovery limit, proof endpoint restriction/hash-only persistence/redaction, and synchronized bootstrap/recovery concurrency; installation ID never authenticates or recovers; valid access authentication; wrong issuer/audience, expired token, invalid signature, and clock-skew boundary; non-idempotent refresh success/rotation/expiry; same token cannot rotate successfully twice; lost-response/consumed-token replay returns no credentials and revokes that family while another device family remains valid; current-family logout; all-family revocation; sixth active family revokes the oldest; no raw refresh/proof persistence; no raw-token/proof/header/cookie logging; guest `UserId`/world identity continuity for later upgrade without an M17 endpoint; auth rate-limit `429`; cross-user/session attacks; atomic world/player creation; one player; two-user denial; cross-world constraints; clean migration.

## Security and ownership considerations
Validate only RS256 JWTs for issuer `parallel-world-api` and audience `parallel-world-mobile`, with 15-minute lifetime, 30-second skew, `sub`, `jti`, and `kid`. Load signing keys externally and verify current plus previous valid rotation key. Store only cryptographic refresh/proof hashes. The client-generated bootstrap proof has at least 256 bits of entropy, is independent of installation ID, is valid only for the bootstrap plus one recovery within 10 minutes, and is then consumed/discarded. It never authenticates normal API calls. Refresh rotates transactionally and non-idempotently; consumed replay, including after response loss, contains the device/session family and never replays credentials. Installation IDs and client `UserId` values never authenticate or recover a session. Apply the accepted 10/IP guest, 30/family refresh, 10/IP invalid-refresh, and 30/user logout limits per 10 minutes. `WorldId`, owner queries, same-world FKs, endpoint-specific idempotency, privacy, and secret-handling rules remain mandatory.

## Acceptance criteria
Guest and owned world persist; ADR-014 proof bootstrap/recovery/expiry/concurrency/scope/redaction pass; ADR-013 validation plus non-idempotent rotation, replay containment, family limits/revocation, redaction, and rate limits pass; foreign user gets ownership-safe denial; retries create one identity/world effect without replaying credential bytes; migration applies cleanly.

## Required verification
Backend unit/API/PostgreSQL/migration/security tests and log-redaction check. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
First guest, simulated lost-bootstrap-response recovery, repeat launch token refresh, current world, foreign-ID attempt, logout.

## Exit criteria
Isolated guest world demonstrated and M03 tests pass.
