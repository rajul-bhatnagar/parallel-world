# Implementation Prompt — Guest Session and Private World

```text
Read AGENTS.md, docs/milestones/M03_SESSION_WORLD.md, docs/product/PRODUCT.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/DECISIONS.md (especially ADR-013 and ADR-014), docs/development/DEVELOPMENT_PLAN.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Guest Session and Private World

Scope:
Implement transactional User, DeviceInstallation, GuestBootstrapOperation, RefreshToken, GameWorld, player Actor, PlayerProfile, WorldSettings, WorldSimulationState, ownership resolution, EF configurations/migration, proof-bound guest bootstrap/recovery endpoint, ADR-013/ADR-014 token issuance/validation/rotation/session-family policy, current-family logout, backend all-family revocation, M03 authentication rate limits, world creation/current-world endpoint, and isolation integration tests. The first guest-bootstrap success creates the entire User/session/world/player result atomically. No character Actors, Character records, posts, registered-authentication endpoints, or public user-facing session management.

Explicit exclusions:
- No AI characters, posts, feed, relationships, or later registered authentication.

Tests:
- Test first guest bootstrap with an independent client-held `GuestBootstrapProof` of at least 256 bits creates exactly one User -> DeviceInstallation/session family -> World -> player Actor -> PlayerProfile -> WorldSettings -> WorldSimulationState transactionally.
- Test an exact same-proof retry within 10 minutes resolves the same User/world/session lineage, invalidates the prior active initial-family credential as needed, returns newly generated access/refresh credentials, consumes the one allowed recovery, and never replays original credential bytes.
- Test proof expiry, second recovery rejection, endpoint restriction, installation-ID substitution rejection, hash-only persistence, raw-proof log/error/URL/response redaction, and that the client proof is not derived from installation ID or treated as a refresh/authentication credential.
- Test synchronized first-bootstrap requests create no duplicate User, world, player, or refresh family, and synchronized recovery attempts produce at most one successful recovery.
- Test two-user/two-world isolation and that later world-create retries cannot create a second exposed world.
- Test valid access-token authentication; wrong issuer; wrong audience; expired token; invalid signature; and timestamps at the accepted 30-second clock-skew boundary.
- Test non-idempotent refresh success, rotation, expiry, and synchronized concurrency proving the same refresh token cannot rotate successfully twice or use generic idempotency response replay.
- Test consumed-token replay, including a simulated lost success response, returns no prior/new credentials and revokes its entire family while another device/session family remains valid.
- Test current-family logout, the all-family backend revocation operation, and sixth-session creation revoking the oldest active family. Do not add a public logout-all/session-management endpoint before M17.
- Test that PostgreSQL never stores a raw refresh token or bootstrap proof and captured logs never contain raw access/refresh tokens, bootstrap proofs, authorization headers, or cookies.
- Test the M03 identity/schema invariant that later guest upgrade retains the same User and owned world; do not implement the M17 upgrade endpoint.
- Test each accepted M03 authentication rate limit returns standard `429` ProblemDetails and that cross-user/session ownership attacks fail safely.

Before editing:
1. List relevant existing files.
2. Provide a short plan.
3. State assumptions and risks.

Requirements:
- Implement only this task.
- Do not implement future milestones.
- Implement ADR-013 exactly: RS256 JWT, issuer `parallel-world-api`, audience `parallel-world-mobile`, 15-minute lifetime, 30-second clock skew, `sub`, `jti`, and `kid`; current plus previous valid verification key; private keys outside source control; no vendor-specific production key provider.
- Implement ADR-014 exactly. `/auth/guest` accepts a client-generated opaque `GuestBootstrapProof` with at least 256 bits of CSPRNG entropy. Store only its secure hash, scope it only to that bootstrap operation, expire recovery 10 minutes after successful bootstrap, and allow at most one successful recovery. It is independent of installation ID, is not a refresh token, never authenticates normal API requests, never bypasses JWT/world ownership, and is never logged or returned.
- Complete the first bootstrap in one transaction: proof hash, User, DeviceInstallation/session lineage, initial refresh family, GameWorld, player Actor/Profile, WorldSettings, and WorldSimulationState. A same-proof first-request race creates no duplicate rows or families.
- On one valid proof retry, keep the same User/world/bootstrap/session lineage, atomically consume recovery, invalidate the previously active initial-family credential as needed, and issue a new access/refresh pair. At most one concurrent recovery succeeds. The client discards the proof after durably persisting the returned refresh token.
- Use opaque refresh tokens with at least 256 bits of entropy, secure hash-only PostgreSQL storage, 30-day lifetime, one-use transactional rotation, one device/session family, strict same-family replay containment, and at most five active families per user.
- Treat `/auth/refresh` as deliberately non-idempotent. It does not use generic `Idempotency-Key`, does not persist reversible credentials, and returns each successful rotation once. Reuse of the consumed token, including after response loss, revokes the affected family and returns neither the prior response nor another token. Clients must not retry refresh through generic idempotency.
- Apply 10/IP/10-minute guest creation, 30/family/10-minute refresh, 10/IP/10-minute invalid refresh/replay, and 30/user/10-minute logout limits. Do not add Redis or distributed access-token/rate-limit infrastructure.
- Treat installation IDs only as metadata. They never authenticate, recover, or rotate a session. Derive user/session/world ownership server-side and never authorize from a client-supplied `UserId`.
- Do not log raw tokens, bootstrap proofs, authorization headers, or cookies. Do not persist raw proofs, raw refresh/access tokens, reversible credential responses, or a distributed access-token denylist; issued access tokens ordinarily expire after 15 minutes.
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
