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

## ADR-019 — M08 relationship-gated autonomous social rules

**Date:** 2026-09-20
**Status:** Accepted; supplements ADR-017 and supersedes its M08 replies/likes consequence

**Context**

ADR-017 established that M08 cannot evaluate autonomous `FOLLOW-01` without M10-owned relationship state. The same dependency also exists in `REPLY-01`, which requires Familiarity and RelationshipRelevance, and `REACT-01`, which requires Affection. Treating those inputs as zero, using new-actor defaults as hidden stand-ins, deriving proxies, weakening thresholds, or introducing alternate formulas would create the same forbidden temporary relationship model.

**Decision**

- During M08, autonomous Character evaluation of `FOLLOW-01`, `REPLY-01`, and `REACT-01` deterministically returns unavailable/ineligible when M10 relationship state is absent.
- An unavailable rule creates no `SimulationAction`, social row, `GameplayEvent`, or aggregate-count mutation. Eligibility is resolved before a rule-specific random roll, so no fallback randomness is consumed.
- M08 does not synthesize, substitute, infer, or persist Familiarity, RelationshipRelevance, Affection, Trust, or Rivalry and does not add M10 entities, fields, or tables.
- M07 Player replies, likes/unlikes, and follows/unfollows remain fully operational and unchanged. The gate applies only to autonomous Character simulation.
- M08 still implements discoverable/evaluable rule eligibility paths, deterministic ordering and priority, tick processing, replay/idempotency, and provenance without fabricating an eligible action.
- M10 activates autonomous `FOLLOW-01`, `REPLY-01`, and `REACT-01` eligibility using the real directional relationship state it introduces and the existing rule formulas.

**Alternatives considered**

Zero substitution, new-actor defaults, interest/topic/trait/reputation proxies, weaker thresholds, alternate pre-M10 formulas, and early relationship persistence were rejected because they invent gameplay behavior or violate milestone ownership.

**Consequences**

M08 autonomously creates character posts only. Reply, reaction, and follow rules remain auditable deterministic unavailable outcomes until M10, with no persisted action or effect. M08 tests protect unchanged M07 Player behavior, unchanged counters, absence of relationship substitutes, and repeatable no-action results.

**Revisit when**

Only if the ownership or required inputs of these rules change through a later accepted gameplay decision.

## ADR-020 — M08 unavailable mandatory simulation inputs

**Date:** 2026-09-20
**Status:** Accepted; supplements ADR-017, ADR-018, and ADR-019 and supersedes ADR-019's M08 autonomous-post consequence

**Context**

M08 has no complete approved persisted source for mandatory numeric `ACT-01` and `POST-01` inputs including GoalRelevance, MoodActivation, and EventRelevance. `CurrentMoodType` is categorical rather than a numeric activation value, CharacterGoals are not assigned to M08, full world/gameplay-event relevance is deferred or undefined, and the post topic weights require unavailable goal/event categories. Substituting zero, inferring values, or redistributing weights would silently change the formulas.

**Decision**

- Every rule first classifies each mandatory input as available or unavailable. Available means approved persisted/domain state exists and its semantics and numeric interpretation are defined. Missing data, undefined numeric mapping, later-milestone ownership, or only a partial categorical value means unavailable.
- A rule with any unavailable mandatory input deterministically returns `Unavailable`/`Ineligible` before probability evaluation. It consumes no rule-specific random draw and creates no `SimulationAction`, `GameplayEvent`, social row, aggregate-count change, or other autonomous side effect.
- In M08, `ACT-01` is unavailable because GoalRelevance, numeric MoodActivation, and EventRelevance lack complete approved sources. Consequently it selects no actor or action family.
- In M08, `POST-01` is unavailable because those numeric inputs and the mandatory goal/event topic-weight categories lack approved sources. Missing components are not set to zero, dropped, redistributed, or renormalized.
- `CurrentMoodType` remains valid categorical M05 state but is not converted into numeric MoodActivation. M08 adds no temporary mood-intensity mapping or storage.
- M08 adds no CharacterGoals and does not use interests as goal proxies. Goal storage, numeric range/default behavior, formula semantics, and versioning must be assigned by a future accepted decision before dependent rules activate.
- Existing technical/provenance `GameplayEvent` rows do not imply EventRelevance. Full world events belong to later scope, and an accepted decision must define any numeric relevance mapping before dependent rules activate.
- M06/M07 Player posts, replies, reactions, and follows remain unchanged. The unavailable result applies only to autonomous simulation.
- M08 still implements meaningful deterministic infrastructure: world ticks/time, ADR-018 timezone projection, rule registration and ordered evaluation, unavailable reasons, deterministic seed/ID facilities, rule-version handling, transactions, interval claims, checkpoints, idempotency/replay, same-world concurrency control, and different-world independence.

**Alternatives considered**

Zero defaults, categorical mood-to-number mappings, interest/reputation proxies, temporary goals or mood tables, GameplayEvent-derived relevance, skipped terms, and redistributed/renormalized post-topic weights were rejected because they invent mechanics or move later scope into M08.

**Consequences**

M08 persists deterministic simulation progress and auditable rule-unavailability results but creates no autonomous social action or effect with the current inputs. Rule evaluation order is mandatory-input validation, availability/eligibility, then probability/random evaluation only for an independently eligible rule. Future activation requires approved real inputs and explicit storage, range/default, formula, and migration/versioning contracts.

**Revisit when**

Accepted source-of-truth decisions assign complete GoalRelevance, numeric MoodActivation, EventRelevance, and post topic-source semantics to implemented persisted state.

## ADR-021 — M08 rule-evaluation persistence

**Date:** 2026-09-20
**Status:** Accepted; supplements ADR-017 through ADR-020

**Context**

M08 must persist auditable successful-but-unavailable rule outcomes without fabricating a `SimulationAction` or `GameplayEvent` and without overloading `SimulationRun.ErrorCode`, which represents actual run failure. Candidate-level diagnostics would be excessive and could accidentally make enumeration order part of persisted behavior.

**Decision**

- Add `SimulationRuleEvaluation`, persisted as `simulation_rule_evaluations`, for deterministic rule-level audit state. It is not a gameplay effect, action, gameplay event, or error log.
- Persist exactly one summary row for each `(SimulationRunId, RuleCode)`. M08 does not persist actor candidates, target candidates, random draws, rejected candidates, or scoring terms.
- Required fields are deterministic `Id`, `WorldId`, `SimulationRunId`, `RuleCode`, `Outcome`, `ReasonCode`, `RuleVersion`, and `EvaluatedAtUtc`. The ID derives from a stable hash namespace containing `WorldId`, run identity, and `RuleCode`; it never uses random/time/runtime hashing.
- Outcomes are `Unavailable`, `Ineligible`, `Eligible`, and `Executed`. `Unavailable` means a mandatory approved input or semantic is absent; `Ineligible` means inputs exist but deterministic conditions reject execution; `Eligible` means eligibility succeeded before a committed effect; `Executed` means the approved action/effect committed.
- Reason codes are stable lowercase machine-readable values, never prose: `relationship_state_unavailable`, `goal_relevance_unavailable`, `mood_activation_unavailable`, `event_relevance_unavailable`, `topic_inputs_unavailable`, `quiet_hours`, and `schedule_ineligible`. New codes require the relevant rule contract; no localized or generated explanation is persisted.
- Primary missing-reason precedence is rule-specific and stable. `REPLY-01`, `REACT-01`, and `FOLLOW-01` check `relationship_state_unavailable` first, followed by their remaining mandatory inputs in rule-definition order. `ACT-01` and `POST-01` use `goal_relevance_unavailable`, then `mood_activation_unavailable`, then `event_relevance_unavailable`; `POST-01` then uses `topic_inputs_unavailable`. M08 therefore records relationship-state as the primary reason for the three relationship-gated rules and goal relevance for ACT/POST under current state.
- An unavailable or ineligible evaluation never creates a `SimulationAction` or `GameplayEvent` and never marks its `SimulationRun` failed. `SimulationRun.ErrorCode` remains null for a successful run and is reserved for actual run/transaction/infrastructure failure.
- The database enforces one row per run/rule and same-world run integrity. Replay reuses the row or deterministically reproduces the identical row; same-world interval locking plus uniqueness prevents concurrent duplicates. Different worlds and different runs remain independent.
- For each M08 tick, the run, required rule evaluations, checkpoint, and world/simulation cursor advancement commit atomically. Rollback leaves none of those records claiming completion. Evaluation rows follow their run's lifecycle; M08 adds no cleanup worker.

**Alternatives considered**

Using `SimulationRun.ErrorCode`, skipped `SimulationAction` rows, synthetic `GameplayEvent` rows, free-form diagnostic prose, candidate-level tables, nondeterministic IDs, and logs alone were rejected because they conflate semantics, fabricate effects, weaken replay, or provide insufficient durable auditability.

**Consequences**

M08 can complete a successful tick with five deterministic `Unavailable` evaluation rows and no autonomous action/effect. Tests can assert exact outcomes, primary reasons, world isolation, replay uniqueness, atomicity, and unchanged Player behavior without treating expected rule unavailability as failure.

**Revisit when**

Candidate-level diagnostics, additional outcomes, retention independent from SimulationRun, or a public diagnostic projection is explicitly approved.

## ADR-022 — M08 fixed tick bootstrap and run granularity

**Date:** 2026-09-20
**Status:** Accepted; supplements ADR-017 through ADR-021

**Context**

M08 had no exact contract for a never-simulated world's first cursor, whether `NextDueAt` or request time owns chronology, or whether one trigger may drain multiple complete ticks. Those choices change interval identity, run counts, cursor advancement, replay, and same-world concurrency behavior. Multi-interval catch-up belongs to M15 rather than the foundational active-tick implementation.

**Decision**

- M08 uses fixed 15-minute half-open UTC intervals. One `SimulationRun` represents exactly one interval `[IntervalStartUtc, IntervalEndUtc)` where `IntervalEndUtc = IntervalStartUtc + 15 minutes`.
- A normal M08 trigger processes at most the single oldest due interval. It creates at most one successful run and never drains or spans an overdue backlog. M15 owns future multi-interval batching/catch-up behavior.
- For a never-simulated world, the authoritative first cursor is `GameWorld.CreatedAtUtc`: first interval `[CreatedAtUtc, CreatedAtUtc + 15 minutes)`. It is eligible exactly when request/current UTC is greater than or equal to its end.
- `WorldSimulationState.NextDueAt` is a persisted scheduling projection, not an independent chronology source. Never-simulated worlds use `CreatedAtUtc + 15 minutes`. After committing an interval ending at `E`, set `LastCompletedIntervalEnd = E` and `NextDueAt = E + 15 minutes`.
- Existing never-simulated rows whose old initialization has `NextDueAt = GameWorld.CreatedAtUtc` are normalized/backfilled to `CreatedAtUtc + 15 minutes` by the M08 migration. World creation adopts the same new default.
- For a previously simulated world, the next interval start is `LastCompletedIntervalEnd`, and its end/due projection is start plus 15 minutes. Cursor-derived chronology is authoritative. A persisted `NextDueAt` mismatch fails safely through existing validation/failure conventions; request time never defines an alternate interval.
- `currentUtc < NextDueAt` creates no run. `currentUtc == NextDueAt` is due. Partial intervals are never processed.
- Run identity and database uniqueness distinguish `WorldId`, interval start/end, and rule version. There cannot be two successful logical runs for that tuple.
- A completed interval replay performs no second advancement or evaluation/effect set. A transactionally failed attempt leaves the same interval due. Cursor fields advance only in the atomic run/evaluation/checkpoint transaction.
- Same-world concurrent triggers contend for the same oldest due interval. One may commit it; the losing invocation returns/reuses the resulting state and must not loop forward into the next overdue interval. Different worlds remain independently lockable.
- Interval boundaries and cursor fields are UTC. ADR-018 timezone projection affects only schedule/quiet-hour rule evaluation and never changes interval identity.

**Alternatives considered**

Immediate creation-time ticks, request-now-derived intervals, one run spanning multiple ticks, multiple runs per M08 trigger, treating `NextDueAt` as independent chronology, and allowing a concurrency loser to consume the next backlog interval were rejected because they weaken replay, blur the M08/M15 boundary, or make invocation timing change interval identity.

**Consequences**

At 31 elapsed minutes, two intervals are eligible but the first M08 trigger creates only `[T,T+15)`, advances the completed cursor to `T+15`, and sets `NextDueAt=T+30`; the world remains due. A second trigger may create `[T+15,T+30)`, advance the cursor to `T+30`, and set `NextDueAt=T+45`. M08 tests assert this exact progression and the migration normalization.

**Revisit when**

M15 defines bounded multi-interval catch-up, batching limits, prioritization, yielding, and long-offline recovery policy.

## ADR-023 — M08 world-time advancement and cursor projections

**Date:** 2026-09-20
**Status:** Accepted; supplements ADR-017 through ADR-022

**Context**

ADR-022 defines the canonical 15-minute UTC interval cursor but does not fully relate it to the existing `GameWorld.CurrentWorldTime`, `GameWorld.LastSimulatedAt`, and `WorldSettings.TimeScale` fields. Without one authority model, delayed processing, replay, or a scale change could make wall-clock chronology and accumulated in-world time diverge nondeterministically.

**Decision**

- `WorldSimulationState.LastCompletedIntervalEnd` is the authoritative persisted cursor for completed canonical UTC intervals. `NextDueAt` remains its derived scheduling projection and never becomes an independent chronology source.
- `GameWorld.LastSimulatedAt` is a compatibility projection of the most recently committed canonical interval end. After `[S,E)` commits, both `LastCompletedIntervalEnd` and `LastSimulatedAt` equal `E`; request arrival, transaction completion time, server local time, and worker delay never set it.
- The existing schema keeps `LastSimulatedAt` non-null and initializes it to `GameWorld.CreatedAtUtc`. For a never-simulated world, `LastCompletedIntervalEnd == null` is the authoritative state; the creation-time `LastSimulatedAt` value is only the legacy-compatible bootstrap representation and does not claim a completed interval.
- `GameWorld.CurrentWorldTime` is the accumulated in-world clock. It continues to initialize to `GameWorld.CreatedAtUtc` and advances only when an interval commits.
- For one M08 interval, `worldTimeDelta = 15 minutes × EffectiveTimeScale`, and `new CurrentWorldTime = previous CurrentWorldTime + worldTimeDelta`. Arithmetic uses the existing exact decimal scale and timestamp/tick representation, not binary floating point, processing delay, backlog age, or wall-clock `now`.
- `WorldSettings.TimeScale` remains a positive `decimal`/PostgreSQL `numeric(8,4)` value with representable persisted range `0.0001` through `9999.9999` and default `1.0000`. It changes only in-world advancement. It does not change the fixed UTC interval duration, due check, `NextDueAt`, `LastCompletedIntervalEnd`, or SimulationRun interval identity.
- Each SimulationRun persists the positive `EffectiveTimeScale` claimed for that interval using the same `numeric(8,4)` representation. A later setting change affects future unprocessed intervals only and never retroactively changes a completed run or replay.
- Schedule and quiet-hour rules use the interval's resulting `CurrentWorldTime` as their canonical world-time instant and convert it through `WorldSettings.DisplayTimeZoneId`. Timezone projection never changes canonical UTC interval identity or cursor fields.
- One M08 trigger advances one interval only. With backlog, each committed interval adds exactly its own scaled delta. M15 may later process several logical intervals in one catch-up invocation, but accumulated world-time advancement remains the deterministic sum of those per-interval deltas.
- The SimulationRun, required SimulationRuleEvaluations, approved effects, checkpoint/claim state, `LastCompletedIntervalEnd`, `NextDueAt`, `LastSimulatedAt`, and `CurrentWorldTime` commit atomically. Failure advances none of them. Replay and same-world concurrency produce at most one world-time advancement for one logical interval; a losing M08 trigger does not consume the next backlog interval.

**Alternatives considered**

Using request or commit time for `LastSimulatedAt`, jumping `CurrentWorldTime` to wall-clock now, applying the entire backlog delta in one M08 run, allowing TimeScale to alter due chronology, reading the current setting again during replay, binary floating-point arithmetic, and leaving the effective scale unaudited were rejected because they break deterministic interval ownership and replay.

**Consequences**

At scale `2.0000`, each committed 15-minute interval advances `CurrentWorldTime` by exactly 30 minutes while the UTC cursor still advances by 15 minutes. At scale `0.5000`, it advances by exactly 7 minutes 30 seconds. Existing new-world values remain compatible, while `LastCompletedIntervalEnd` unambiguously distinguishes never-simulated state.

**Revisit when**

M15 defines multi-interval catch-up execution details or a future approved feature introduces historical TimeScale editing beyond per-run auditability.

## New ADR template

### ADR-XXX — Title

**Date:** YYYY-MM-DD  
**Status:** Proposed / Accepted / Superseded

**Context**

**Decision**

**Alternatives considered**

**Consequences**

**Revisit when**
