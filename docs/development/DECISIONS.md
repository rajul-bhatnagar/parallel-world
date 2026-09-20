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
- Installation identifiers are metadata only, never authentication, recovery, or rotation credentials. Ownership and session scope resolve server-side and never from a client-supplied `UserId`. ADR-014 defines the separate guest-bootstrap recovery proof.
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

## ADR-014 — M03 guest-bootstrap recovery and non-idempotent refresh

**Date:** 2026-08-30
**Status:** Accepted; supplements ADR-013

**Context**

Generic idempotency required credential-issuing endpoints to replay their original response, while ADR-013 permits only hash storage and treats reuse of a consumed refresh token as replay. The original raw credential therefore cannot be reproduced safely, and installation identity cannot authorize guest recovery.

**Decision**

- `POST /api/v1/auth/refresh` is intentionally non-idempotent. It accepts no generic `Idempotency-Key` semantics. A successful call atomically consumes the presented token and returns its replacement exactly once. A consumed/replaced token presented again revokes only its affected family and never replays the earlier response or issues another replacement. Losing a successful refresh response may lose that family and require bootstrap or reauthentication; this is an accepted MVP tradeoff.
- Initial `POST /api/v1/auth/guest` requires a client-generated opaque `GuestBootstrapProof` with at least 256 bits of cryptographic randomness, independent of installation identity. The server stores only its secure hash, scopes it to one bootstrap operation, never accepts it at another endpoint, and never logs or returns it.
- The bootstrap proof has a 10-minute recovery window from successful bootstrap and permits at most one successful recovery rotation. The client discards it after safely persisting the initial refresh token. Expired or recovery-consumed proof cannot recover credentials.
- First bootstrap transactionally creates exactly one guest `User`, `DeviceInstallation`, initial refresh family, `GameWorld`, player Actor/Profile, `WorldSettings`, and `WorldSimulationState`, then returns access/refresh credentials. Only hashes of bootstrap and refresh secrets are persisted.
- A valid retry within the window returns the same User/world identity without duplicating bootstrap state. Because the original refresh token is unrecoverable, the retry atomically invalidates the currently active initial-family credential, issues new access/refresh credentials in the same bootstrap/session lineage, and marks recovery consumed. Concurrent bootstrap/recovery attempts create one identity/world and permit at most one successful recovery rotation.
- Guest bootstrap is identity-idempotent, not byte-for-byte credential replay. Generic business idempotency remains unchanged for non-credential writes. Installation ID remains metadata only; generic idempotency keys remain deduplication inputs for approved non-credential operations. Neither is an authentication or recovery credential.
- Guest bootstrap and recovery use the accepted M03 guest-auth rate limit. Proof hashes are retained only as required for bootstrap integrity, replay/audit, and the later accepted retention policy.

**Alternatives considered**

Reversible credential-response storage, refresh retry grace, installation-ID authentication, treating idempotency keys as credentials, and reproducing deterministic refresh tokens were rejected because they weaken ADR-013 or violate hash-only secret storage.

**Consequences**

M03 can safely recover an initial lost bootstrap response without retaining plaintext credentials. Ordinary refresh remains deliberately unforgiving under response loss. The Flutter bootstrap client in M04 must generate, protect during the request/recovery window, and then discard the proof.

**Revisit when**

Observed mobile network behavior justifies a different reviewed rotation protocol, M17 defines registered reauthentication, or the retention policy is finalized.

## ADR-015 — M06 chronological feed ordering

**Date:** 2026-09-12
**Status:** Accepted

**Context**

M06 requires one stable total order for the private world feed before its API, cursor, database query, Flutter cache, and pagination tests can be implemented. Chronological and deterministic-ranked ordering were previously both listed as candidates, but M06 does not yet have the relationship, interaction, engagement, or simulation signals needed to justify a ranking model.

**Decision**

- M06 orders feed posts strictly by `createdAtUtc DESC, id DESC` within the authenticated user's authorized world. Newer posts appear first, and `id DESC` is the deterministic tie-breaker when timestamps are equal.
- M06 uses no ranking score, popularity weighting, relationship weighting, AI-generated ranking, simulation-based ordering, randomization, or client-side resorting.
- Cursor pagination uses the tuple `(createdAtUtc, id)`. The cursor is opaque, versioned, scope-bound, and validated by the server; clients do not decode it.
- The next page contains only posts for which `createdAtUtc < cursor.createdAtUtc`, or `createdAtUtc = cursor.createdAtUtc` and `id < cursor.id`. This seek predicate prevents adjacent cursor pages from duplicating previously returned items when newer posts arrive between requests.
- M06 guarantees stable chronological pagination under this tuple ordering. It does not introduce snapshot-isolation or feed-session semantics.
- Deterministic-ranked or personalized ordering is deferred. A later ordering model requires a separate accepted decision after its signals and stable cursor tuple are explicitly defined.

**Alternatives considered**

Deterministic ranking and personalized ranking were deferred because their inputs and scoring contract do not exist in M06. Offset pagination and client-side sorting were rejected because they do not provide the required stable server-authoritative ordering for a growing feed.

**Consequences**

The M06 API, PostgreSQL query/index, Flutter cache, and automated tests share one exact ordering oracle. New posts may appear before an existing cursor between requests, but items already returned are not duplicated solely because of cursor continuation. Any future ranking change is a public pagination-contract change requiring compatibility review.

**Revisit when**

A later milestone has approved ranking signals such as relationships, interactions, recency weighting, simulation state, or engagement, and a separate decision defines the ranking, tie-breaks, cursor compatibility, and migration behavior.

## ADR-016 — M07 parent-scoped reply thread reads

**Date:** 2026-09-16
**Status:** Accepted

**Context**

M07 requires a public thread-read contract and deterministic cursor tests. The existing API defined single-post reads and reply creation but did not define whether replies were embedded, recursively flattened, or exposed through a separate paginated collection.

**Decision**

- `GET /api/v1/worlds/{worldId}/posts/{postId}/replies` returns only the direct child replies of the specified post or reply. It uses the standard collection envelope and does not recursively nest or flatten the thread.
- `GET /api/v1/worlds/{worldId}/posts/{postId}` remains the single Post/detail endpoint. It may expose approved aggregate metadata such as reply count, but it does not embed the paginated reply collection.
- Direct replies use the total order `createdAtUtc ASC, id ASC`. The opaque cursor represents the last visible `(createdAtUtc, id)` tuple, is versioned and tamper-safe, and is bound to the authorized `worldId`, parent post/reply ID, and applicable filters.
- The next page contains only replies for which `createdAtUtc > cursor.createdAtUtc`, or `createdAtUtc = cursor.createdAtUtc` and `id > cursor.id`. Invalid, tampered, foreign-world, or wrong-parent cursors fail through the standard ownership-safe cursor/error contract.
- Thread traversal is explicit and parent-scoped: clients read the root Post separately, then request direct replies for any post/reply whose children they need. M07 does not preload or return a recursively flattened depth-two tree.
- `MAX_REPLY_DEPTH = 2` means root post depth 0, reply to root depth 1, and reply to a depth-one reply depth 2. Creating depth 3 is rejected. Reading direct children of a depth-two reply succeeds with an empty collection.

**Alternatives considered**

Embedding every reply in the Post detail, recursively nested responses, flattened whole-thread pagination, and offset pagination were rejected because they make parent ownership, stable pagination, and bounded response behavior less explicit.

**Consequences**

Backend queries, Flutter state, and tests share one parent-scoped ordering and cursor contract. Each parent collection paginates independently, and clients fetch deeper branches only when needed.

**Revisit when**

A later approved product requirement needs whole-thread search, a different nesting/depth model, or a compatible bulk thread-loading contract.

## ADR-017 — M08 autonomous follows require M10 relationship state

**Date:** 2026-09-20
**Status:** Accepted

**Context**

M08 introduces deterministic autonomous social simulation, while `FOLLOW-01` requires directional Familiarity, Trust, Affection, and Rivalry inputs owned by the M10 relationship engine. M08 explicitly excludes relationship persistence and mechanics. Synthesizing temporary values, deriving proxy values from interests or reputation, weakening the follow threshold, or moving M10 persistence into M08 would create an undocumented second relationship model and make later activation inconsistent.

**Decision**

- M08 does not synthesize, infer, persist, or otherwise substitute Familiarity, Trust, Affection, or Rivalry values.
- When M10 relationship state is unavailable, autonomous Character evaluation of `FOLLOW-01` deterministically returns unavailable/ineligible. It creates no autonomous follow action, `Follow` row, or follow-change event and consumes no fallback random roll.
- The absence of M10 relationship state is expected M08 behavior, not a simulation error. Repeating the same M08 state continues to produce no autonomous follow action.
- M07 Player follow and unfollow behavior remains fully available and unchanged.
- M10 activates autonomous `FOLLOW-01` evaluation using the real directional relationship state it introduces. M10 must not redesign the accepted `FOLLOW-01` scoring contract merely to activate it.

**Alternatives considered**

Temporary relationship defaults, hidden pair scores derived from interests or reputation, reduced follow thresholds, and early M10 relationship tables were rejected because they invent mechanics, violate milestone ownership, or produce state that M10 would later have to reinterpret.

**Consequences**

M08 autonomously creates posts, replies, and likes, but not follows. Its follow evaluation and tests must prove the deterministic unavailable state, absence of autonomous follow rows and temporary relationship state, and continued operation of M07 Player follows. M10 owns the first milestone in which autonomous Character follows can become eligible.

**Revisit when**

Only if the ownership or inputs of `FOLLOW-01` are changed through an accepted gameplay and architecture decision.

## ADR-018 — M08 world display timezone contract

**Date:** 2026-09-20
**Status:** Accepted

**Context**

M08 evaluates `ACT-01` quiet hours, character schedules, and other local activity windows. Those rules require a stable local-time projection, but the existing world model does not identify a timezone or define a deterministic default. Using the server, device, locale, IP address, or operating-system timezone would make outcomes host-dependent.

**Decision**

- `WorldSettings` stores one required IANA timezone identifier as `DisplayTimeZoneId`, persisted in PostgreSQL as `display_time_zone_id`.
- Existing and newly created worlds default to the exact identifier `UTC`. The M08 migration adds a non-null column and backfills existing rows to `UTC`.
- Canonical simulation, event, action, and persistence timestamps remain UTC instants. M08 converts the applicable UTC simulation instant through the configured IANA timezone only to derive the local date and wall-clock time used by schedule and quiet-hour rules.
- M08 uses the world timezone for every Character in that world. It introduces no per-character timezone state.
- Timezone database rules provide offsets and daylight-saving transitions. Because conversion starts from a UTC instant, ambiguous or invalid local wall-clock timestamps are never engine inputs. `UTC` has no daylight-saving transition.
- The server never infers this value from its local timezone, a client device, user locale, IP address, or operating-system settings.
- A non-empty supplied timezone identifier must resolve as a supported IANA timezone or fail through the standard validation/ProblemDetails contract. Explicit invalid values never silently fall back to `UTC`.

**Alternatives considered**

Server-local time, device-local inference, Windows timezone IDs in domain state, manual offsets/DST rules, silent invalid-value fallback, and per-character timezones in M08 were rejected because they weaken determinism, portability, or milestone scope.

**Consequences**

M08 adds the world-settings field and migration, defaults and backfills it to `UTC`, and tests UTC/non-UTC/DST projections independently of host configuration. A later configurable API may expose the setting using standard validation, but M08 does not need a timezone-selection UI. Per-character timezones require a separate accepted decision and migration.

**Revisit when**

A later approved product requirement introduces player-configurable or per-character timezones.

## New ADR template

### ADR-XXX — Title

**Date:** YYYY-MM-DD  
**Status:** Proposed / Accepted / Superseded

**Context**

**Decision**

**Alternatives considered**

**Consequences**

**Revisit when**
