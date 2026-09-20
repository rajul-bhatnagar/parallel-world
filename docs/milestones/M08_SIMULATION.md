# M08 — Rule-based simulation

## Goal
Establish deterministic, replay-safe world simulation and explicit rule availability without external AI or fabricated mechanics.

## User-visible result
The private world gains reproducible simulation progress and auditable unavailable/ineligible rule outcomes. With current persisted inputs, M08 creates no autonomous social action or effect. Existing M06/M07 Player posts, replies, likes/unlikes, and follows/unfollows remain available.

## Dependencies
M05-M07.

## Scope
- **Backend:** Injected clock/PRNG, canonical UTC interval cursor, accumulated `CurrentWorldTime`, exact decimal TimeScale advancement, deterministic projection of the interval's resulting world time through the world-level IANA display timezone for schedules/quiet hours, SimulationRun/Action provenance model, ordered rule registration/evaluation, deterministic unavailable/ineligible paths for `ACT-01` and `POST-01` while mandatory numeric/topic inputs are undefined and for `REPLY-01`, `REACT-01`, and `FOLLOW-01` while M10 relationship state is absent, stable outcomes/reasons, one fixed 15-minute half-open interval per run, at most the single oldest due interval per normal trigger, template fallback component, and checkpoint-safe execution.
- **Database:** Runs/actions/rule-evaluation summaries/idempotency/work records, per-run `EffectiveTimeScale`, one evaluation per run/rule, exact interval uniqueness, cursor locking, atomic `CurrentWorldTime`/`LastSimulatedAt` projections, composite target FKs, `NextDueAt` as the cursor-derived scheduling projection, normalization of existing never-simulated rows from creation time to creation time plus 15 minutes, and required `WorldSettings.DisplayTimeZoneId` persisted as `display_time_zone_id`; the migration backfills existing worlds and defaults new worlds to `UTC`.
- **Flutter:** Development-only trigger only if securely gated; status and authoritative refresh.
- **Infrastructure:** BackgroundService may process durable PostgreSQL work; in-memory queue is not authority.

## Explicit exclusions
External AI, catch-up compression, relationships/memory/dating, real-time delivery, temporary/proxy relationship inputs, temporary CharacterGoals, invented mood activation/event relevance, topic-weight redistribution, and activation of any autonomous rule whose mandatory inputs remain unavailable.

## Test scope
Same-state/interval/version/seed equality, candidate-order independence, rule priority/reasons, duplicate/overlap, rollback/partial resume, and cross-world isolation. Verify first interval `[CreatedAtUtc, CreatedAtUtc + 15 minutes)`, not-due and exact-due boundaries, normalization of legacy never-simulated `NextDueAt`, one 15-minute run per trigger, and the exact two-trigger progression after 31 elapsed minutes without batching. Verify `CurrentWorldTime` initializes to creation time; the existing non-null `LastSimulatedAt` bootstrap equals creation time while null `LastCompletedIntervalEnd` remains the never-simulated authority; scale `1`, `2`, and supported fractional `0.5` advance one interval by exactly 15 minutes, 30 minutes, and 7 minutes 30 seconds; `LastSimulatedAt` then mirrors the committed cursor rather than processing-now; each run captures its effective scale; later scale changes affect only future intervals; two-interval backlog advances once per trigger; and host timezone/latency cannot affect the result. Verify completed replay is a no-op, a failed transaction advances none of the cursor/world-time fields, same-world concurrent triggers produce one logical run/world-time advancement and the loser does not advance into the next interval, and different worlds remain independent. Verify existing/new-world `UTC` defaults; UTC and non-UTC IANA schedule/quiet-hour projection from the resulting `CurrentWorldTime`; quiet-hour boundaries after conversion; deterministic host-independent repeat evaluation; timezone-database DST projection; invalid-zone rejection; and absence of per-character timezone state. Verify `ACT-01` is unavailable for missing GoalRelevance, numeric MoodActivation, or EventRelevance and creates no action/event/social mutation or random draw; verify `POST-01` is unavailable with incomplete mandatory weighted inputs, does not redistribute/renormalize weights, and creates no post/feed mutation. Verify `REPLY-01`, `REACT-01`, and `FOLLOW-01` remain repeatedly unavailable without relationship state and create no actions/effects/counter mutations; existing Player actions remain unaffected; no proxy/temporary/later-milestone state is persisted; and unavailable rules do not corrupt deterministic sequencing, retry, or rule-version behavior. Persist exactly one deterministic `SimulationRuleEvaluation` per run/rule with the stable primary reason; prove different rules/runs/worlds remain independent, replay/concurrency cannot duplicate rows, unavailable does not fail the run, actual failure uses `SimulationRun.ErrorCode`, and rollback cannot leave orphaned evaluations.

## Security and ownership considerations
Uncontrolled time/randomness, ordering, idempotency, AI absence, `WorldId`. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Same inputs give identical persisted progress and one identical evaluation per run/rule; each run owns exactly one 15-minute interval and captures its effective decimal TimeScale; one normal trigger processes at most the oldest due interval; cursor, due projection, `LastSimulatedAt`, and scaled `CurrentWorldTime` advance together only after commit; duplicate intervals produce no duplicate progress, world-time advancement, diagnostics, or effects; unavailable mandatory inputs are reported with deterministic primary reasons before random evaluation without failing the run; no provider is required; no autonomous social effect is fabricated; and existing M06/M07 Player actions remain unchanged.

## Required verification
Unit/scenario/PostgreSQL/concurrency/architecture/security tests and deterministic snapshot comparison. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Run a fixed seed twice from a restored fixture; inspect persisted interval progress and unavailable reason codes; retry the interval and confirm no autonomous social mutation.

## Exit criteria
Reproducible, auditable tick processing and unavailable rule evaluation pass independent simulation review.
