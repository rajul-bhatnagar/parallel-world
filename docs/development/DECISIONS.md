# Architecture Decision Log

Do not rewrite accepted entries. Add a new record and mark an older decision superseded when necessary.

## ADR-001 — Private isolated worlds

**Status:** Accepted

Each real user owns isolated single-player worlds. Real users never interact.

## ADR-002 — Modular monolith

**Status:** Accepted

Use one deployable ASP.NET Core backend divided into modules. Avoid microservices initially.

## ADR-003 — PostgreSQL authority

**Status:** Accepted

PostgreSQL is the source of truth. Flutter local storage is a cache/offline layer.

## ADR-004 — Rules before language generation

**Status:** Accepted

The deterministic engine decides mechanics; AI generates wording only.

## ADR-005 — Guest-first authentication

**Status:** Accepted

Start through automatic guest sessions and allow later account upgrade without progress loss.

## ADR-006 — Catch-up simulation

**Status:** Accepted

The world uses compressed catch-up processing because free/low-cost hosting may sleep and mobile users may remain away for long periods.

## ADR-007 — M01 technology baseline

**Date:** 2026-08-25
**Status:** Accepted

Baseline checked against official Flutter stable-release and supported-platform documentation on 2026-08-25. Use the stable .NET 10 LTS SDK and `net10.0`; stable Flutter 3.47 with its bundled compatible Dart 3.13 SDK; Android API 24 as the minimum supported Android level; and Android as the initial required launch platform. iOS is deferred. M01 must pin this accepted toolchain, including the latest stable compatible patch available within the accepted lines, in repository toolchain/configuration files. Patch updates may be adopted after applicable CI passes; any future minor or major Flutter/Dart upgrade requires explicit review. Preview, RC, beta, dev, main/master, and nightly releases are prohibited.

## ADR-008 — Project and namespace prefix

**Date:** 2026-08-25
**Status:** Accepted

Use `ParallelWorld` consistently: projects are `ParallelWorld.Api`, `ParallelWorld.Application`, `ParallelWorld.Domain`, `ParallelWorld.Infrastructure`, `ParallelWorld.Simulation`, and `ParallelWorld.AI`; namespaces are `ParallelWorld.<Project>` and `ParallelWorld.<Project>.<Area>`. The Flutter package remains `parallel_world_app`.

## ADR-009 — PostgreSQL identifier naming

**Date:** 2026-08-25
**Status:** Accepted

Use unquoted `snake_case` PostgreSQL identifiers and PascalCase C# entity/property names. EF Core naming conventions or explicit mappings translate between them. Do not create quoted PascalCase database identifiers.

## ADR-010 — Initial CI baseline

**Date:** 2026-08-25
**Status:** Accepted

M01 creates a working initial GitHub Actions workflow containing all applicable M01 checks for only the projects that exist: backend restore, configured format check, build, and the unit/empty test suite; Flutter `pub get`, format check, analyze, and tests. PostgreSQL integration jobs begin only in the milestone that introduces PostgreSQL-dependent tests. A check that is not yet applicable must be reported as such rather than simulated.

## ADR-011 — Merge workflow

**Date:** 2026-08-25
**Status:** Accepted

Use a milestone feature branch, push it, open a pull request into `main`, require all available CI checks to succeed, and review before merge. For a one-developer project self-review is acceptable, but the independent Codex diff review required by `DEVELOPMENT_PLAN.md` remains mandatory.

## ADR-012 — Persistent development branch and stable promotion

**Date:** 2026-08-25
**Status:** Accepted; supersedes ADR-011

`main` is the stable/release-ready branch and `dev` is the persistent active development and integration branch. Implement M01 and later milestones sequentially on `dev`. After each milestone, run its verification, inspect the diff, create a milestone-specific commit, and push `dev`; a pull request is not required after every milestone. When `dev` reaches an approved stable checkpoint, promote it through a pull request from `dev` into `main`, require all applicable CI checks to succeed, review the pull request, and merge through that pull request. A reviewed local merge is not an accepted substitute for this promotion. Direct feature development on `main` is prohibited. Short-lived `feature/...` branches are optional for isolated or risky work and are not required by the milestone workflow. For a one-developer project self-review is acceptable, but the independent Codex diff review required by `DEVELOPMENT_PLAN.md` remains mandatory.

## ADR-013 — M03 access-token and session-family policy

**Date:** 2026-08-30
**Status:** Accepted

**Context**

M03 requires concrete access-token, refresh-token, replay-containment, session-limit, signing-key, revocation, and authentication-rate-limit rules. The earlier sources defined the security boundaries but intentionally left these values open, blocking implementation.

**Decision**

- Access tokens are JWT bearer tokens signed only with RS256. Validation requires the signature, issuer `parallel-world-api`, audience `parallel-world-mobile`, expiration, and `not-before` when present, with an allowed clock skew of exactly 30 seconds. Tokens last 15 minutes, carry a stable user identifier in `sub` and a unique `jti`, and contain no secrets, private content, world lists, or unnecessary profile data.
- Every token carries a `kid`. The signing private key is never committed. Development loads a local key from outside the repository; production obtains key material through environment or secret-management infrastructure. Verification accepts the current key and the previous still-valid verification key during controlled rotation. The production secret provider remains an M18 decision.
- Refresh tokens are opaque cryptographically random bearer values with at least 256 bits of entropy and a 30-day lifetime from issuance. PostgreSQL stores only a secure cryptographic hash. Successful use atomically consumes the token and issues a replacement in the same family; no token can rotate successfully twice.
- One refresh-token family represents one device/session. A consumed or replaced token replay transactionally revokes that entire family and requires a new or restored valid session, but does not revoke unrelated families. Records retain device association, expiry, consumption, replacement, revocation, and safe audit timestamps without raw tokens.
- A user may have at most five active device/session families. Creating a sixth revokes the oldest active family. Guest and registered users share this model. Current-device logout revokes the current family; all-device logout revokes every active family for the user. Revoked or expired families cannot mint access tokens. Guest upgrade preserves the same `UserId` and worlds.
- Installation identifiers are metadata/recovery hints, never authentication credentials. Ownership and session scope resolve server-side and never from a client-supplied `UserId`.
- Issued access tokens ordinarily remain usable until their 15-minute expiry. MVP has no distributed access-token denylist. A future immediate-revocation or sender-constrained-token requirement requires a new decision.
- Initial M03 limits are: guest/session creation 10 attempts per IP per 10 minutes; refresh 30 attempts per device/session family per 10 minutes; invalid refresh/replay 10 attempts per IP per 10 minutes; logout 30 requests per authenticated user per 10 minutes. Exceeding a limit returns the standard `429` ProblemDetails response. M03 uses a simple modular-monolith-compatible implementation and does not introduce Redis or other distributed rate-limit infrastructure.
- Access and refresh tokens, authorization headers, and cookies remain protected by the existing logging redaction rules. Refresh rotation is concurrency-safe; replay detection and family revocation are transactional.

M03 must test the concrete token-validation matrix, rotation/concurrency/replay behavior, family isolation, expiry and logout scopes, five-family limit, absence of raw token persistence/logging, rate limiting, cross-user/session attacks, and identity continuity required for later same-user upgrade. Registered authentication, recovery, and public upgrade/session-management endpoints remain M17 work.

**Alternatives considered**

Symmetric signing, JWT refresh tokens, nonrotating refresh tokens, plaintext refresh storage, global revocation after one-device replay, a distributed access-token denylist, and Redis-backed MVP rate limiting were rejected because they either weaken containment or add unnecessary initial operational scope.

**Consequences**

M03 can implement and verify one precise session model. Ordinary logout does not immediately invalidate an already issued access token, so the short expiry is an explicit accepted limitation. Production key custody, hosting integration, and tuning remain later operational work.

**Revisit when**

M17 selects registered authentication/recovery, M18 selects production secret infrastructure, observed traffic requires rate-limit tuning or distributed coordination, or a higher-risk use case requires immediate or sender-constrained access-token revocation such as DPoP or mTLS.

## New ADR template

### ADR-XXX — Title

**Date:** YYYY-MM-DD  
**Status:** Proposed / Accepted / Superseded

**Context**

**Decision**

**Alternatives considered**

**Consequences**

**Revisit when**
