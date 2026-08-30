# Implementation Prompt — Guest Session and Private World

```text
Read AGENTS.md, docs/milestones/M03_SESSION_WORLD.md, docs/product/PRODUCT.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/DECISIONS.md (especially ADR-013), docs/development/DEVELOPMENT_PLAN.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Guest Session and Private World

Scope:
Implement transactional User, DeviceInstallation, RefreshToken, GameWorld, player Actor, PlayerProfile, WorldSettings, WorldSimulationState, ownership resolution, EF configurations/migration, guest session endpoint, ADR-013 token issuance/validation/rotation/session-family policy, current-family logout, backend all-family revocation, M03 authentication rate limits, world creation/current-world endpoint, and isolation integration tests. No character Actors, Character records, posts, registered-authentication endpoints, or public user-facing session management.

Explicit exclusions:
- No AI characters, posts, feed, relationships, or later registered authentication.

Tests:
- Test transactional User -> World -> player Actor -> PlayerProfile -> WorldSettings -> WorldSimulationState creation, retry/idempotency, and two-user/two-world isolation.
- Test valid access-token authentication; wrong issuer; wrong audience; expired token; invalid signature; and timestamps at the accepted 30-second clock-skew boundary.
- Test refresh success, rotation, expiry, and synchronized concurrency proving the same refresh token cannot rotate successfully twice.
- Test consumed-token replay revokes its entire family while another device/session family remains valid.
- Test current-family logout, the all-family backend revocation operation, and sixth-session creation revoking the oldest active family. Do not add a public logout-all/session-management endpoint before M17.
- Test that PostgreSQL never stores a raw refresh token and captured logs never contain raw access/refresh tokens, authorization headers, or cookies.
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
- Use opaque refresh tokens with at least 256 bits of entropy, secure hash-only PostgreSQL storage, 30-day lifetime, one-use transactional rotation, one device/session family, strict same-family replay containment, and at most five active families per user.
- Apply 10/IP/10-minute guest creation, 30/family/10-minute refresh, 10/IP/10-minute invalid refresh/replay, and 30/user/10-minute logout limits. Do not add Redis or distributed access-token/rate-limit infrastructure.
- Treat installation IDs only as metadata/recovery hints. Derive user/session/world ownership server-side and never authorize from a client-supplied `UserId`.
- Do not log raw tokens, authorization headers, or cookies. Do not persist access tokens or a distributed access-token denylist; issued access tokens ordinarily expire after 15 minutes.
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
