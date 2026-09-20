# Implementation Prompt — Rule-Based Simulation

```text
Read AGENTS.md, docs/milestones/M08_SIMULATION.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Rule-Based Simulation

Scope:
Implement SimulationRun, SimulationAction provenance/schema, deterministic random provider, world clock, ordered rule discovery/evaluation, stable unavailable/ineligible results for ACT-01, POST-01, REPLY-01, REACT-01, and FOLLOW-01, decision/execution separation, template fallback component, development trigger, idempotency, checkpoint, and transaction tests. Current inputs create no autonomous social effect. No external AI, messaging, memory, or dating.

Mandatory-input availability:
- Resolve and validate every mandatory input before eligibility/probability/random evaluation.
- ACT-01 is unavailable in M08 because GoalRelevance, numeric MoodActivation, and EventRelevance lack approved complete persisted sources/semantics.
- POST-01 is unavailable in M08 for those inputs and because mandatory goal/event topic-weight categories are unavailable.
- Do not substitute zero, infer/proxy values, map CurrentMoodType to a number, create temporary goals/mood/event state, bypass formula terms, or drop/redistribute/renormalize topic weights.
- An unavailable rule creates no SimulationAction, GameplayEvent, social row, work item, counter mutation, or rule-specific random draw. Persist stable run/checkpoint progress and diagnostic unavailability reasons instead.
- Use order-independent deterministic substreams so unavailable rules do not shift other rules. Continue evaluating any independently eligible rule in stable priority/identifier order.
- Future activation requires accepted real-input storage, numeric range/default, formula semantics, and migration/rule-version behavior; do not assign that later scope in M08.

Rule-evaluation persistence:
- Add `SimulationRuleEvaluation`/`simulation_rule_evaluations` with deterministic Id, WorldId, SimulationRunId, RuleCode, Outcome, ReasonCode, RuleVersion, and EvaluatedAtUtc.
- Persist exactly one row per `(SimulationRunId, RuleCode)`; enforce same-world composite run integrity. Do not persist candidate/target/draw/score rows.
- Outcomes are Unavailable, Ineligible, Eligible, and Executed. Unavailable/ineligible creates no SimulationAction or GameplayEvent and does not set SimulationRun.ErrorCode.
- Use stable lowercase reason codes: relationship_state_unavailable, goal_relevance_unavailable, mood_activation_unavailable, event_relevance_unavailable, topic_inputs_unavailable, quiet_hours, and schedule_ineligible.
- Primary precedence: relationship-gated rules check relationship state first; ACT-01/POST-01 check goal relevance, then mood activation, then event relevance, with POST-01 topic inputs last.
- Derive evaluation IDs from WorldId + run identity + RuleCode through the deterministic ID mechanism. Replay reuses/reproduces the identical row.
- Commit the run, required evaluations, checkpoint, and world/simulation cursor advancement atomically for an M08 tick. Same-world locking plus uniqueness prevents duplicates; different worlds remain independent. Evaluation retention follows its run; add no cleanup worker.

Relationship-gated autonomous social rules:
- M08 must not synthesize, infer, persist, or proxy Familiarity, Trust, Affection, or Rivalry.
- Without M10 relationship state, autonomous Character REPLY-01, REACT-01, and FOLLOW-01 evaluations are deterministically unavailable/ineligible and create no SimulationAction, social row, GameplayEvent, or reply/like-count mutation. Resolve this before any rule-specific random roll.
- Existing M07 Player replies, likes/unlikes, and follows/unfollows remain operational and unchanged.
- M10 activates autonomous REPLY-01, REACT-01, and FOLLOW-01 eligibility using real relationship state; do not implement or design that state in M08.

World display timezone contract:
- Add required `WorldSettings.DisplayTimeZoneId`, persisted as `display_time_zone_id`, containing an IANA timezone identifier.
- The migration and new-world creation default to the exact identifier `UTC`; backfill existing worlds to `UTC` before enforcing non-nullability.
- Persist canonical interval/event/action timestamps in UTC. Convert the interval's deterministic resulting `CurrentWorldTime` through the world timezone only to derive schedule and quiet-hour local date/time.
- Use timezone database DST rules. Do not accept an ambiguous/invalid local timestamp as engine input, because evaluation starts from UTC.
- Do not infer the timezone from the server, device, locale, IP address, or operating system. Do not add per-character timezone state.
- Reject an explicitly supplied invalid/non-IANA timezone using the standard validation/ProblemDetails contract; do not silently fall back to UTC.

Tick bootstrap and run granularity:
- Use fixed 15-minute half-open UTC intervals. One `SimulationRun` represents exactly one interval.
- For a never-simulated world, the first interval is `[GameWorld.CreatedAtUtc, GameWorld.CreatedAtUtc + 15 minutes)` and becomes eligible exactly at its end. Initialize new `NextDueAt` values to creation plus 15 minutes and normalize existing never-simulated creation-time values in the M08 migration.
- For later intervals, start at `LastCompletedIntervalEnd` and end 15 minutes later. Treat `NextDueAt` as the cursor-derived scheduling projection, not chronology authority; fail safely on a mismatch.
- A normal M08 trigger processes at most the single oldest due interval. It does not batch, drain, or span backlog; M15 owns bounded multi-interval catch-up.
- After `[S,E)` commits, atomically set `LastCompletedIntervalEnd=E` and `NextDueAt=E+15 minutes` with the run, evaluations, checkpoint, and mechanical effects. A failed transaction leaves the same interval due.
- Completed replay does not advance again. Same-world concurrent triggers may produce only one logical run for the interval, and a losing invocation must not continue into the next overdue interval. Different worlds remain independent.
- Keep interval identity and cursors in UTC. World timezone projection affects schedule/quiet-hour evaluation only.

World-time advancement:
- Treat `LastCompletedIntervalEnd` as the authoritative completed canonical UTC cursor and `NextDueAt` as its scheduling projection. `LastSimulatedAt` is compatibility-only: after commit it equals the new cursor and is never set from request/commit/processing time.
- Preserve the existing non-null bootstrap representation `LastSimulatedAt = CreatedAtUtc`; while `LastCompletedIntervalEnd` is null, it does not claim a completed interval. Preserve `CurrentWorldTime = CreatedAtUtc` initialization.
- Use the existing positive C# `decimal`/PostgreSQL `numeric(8,4)` TimeScale, representable persisted range `0.0001` through `9999.9999`, default `1.0000`. For each committed interval, calculate `worldTimeDelta = 15 minutes × EffectiveTimeScale` with exact decimal/tick arithmetic and add it to the prior `CurrentWorldTime`.
- Persist `EffectiveTimeScale` on SimulationRun using the same `numeric(8,4)` representation. Capture the setting when the interval is claimed; later changes affect future unprocessed intervals only and never alter completed replay.
- TimeScale changes only accumulated in-world time. It never changes interval boundaries, eligibility, `NextDueAt`, `LastCompletedIntervalEnd`, or interval identity. Backlog advances one interval's scaled delta per M08 trigger.
- Use the interval's resulting `CurrentWorldTime`, converted through `DisplayTimeZoneId`, for schedule and quiet-hour evaluation. Do not use host/device time or the canonical interval end as a substitute for scaled world time.
- Atomically commit the run, required evaluations, approved effects, checkpoint/claim state, `LastCompletedIntervalEnd`, `NextDueAt`, `LastSimulatedAt`, and `CurrentWorldTime`. Rollback changes none. Replay and same-world concurrency advance world time at most once; the loser cannot consume the next backlog interval.

Explicit exclusions:
- No external AI, messaging, memory, romance, catch-up, or deferred events/trends.

Tests:
- Test deterministic seeds/order, interval uniqueness, idempotency, concurrency, transaction boundaries, fallback wording, and cross-world rejection.
- Verify autonomous reply/reaction/follow ineligibility without relationship state; no autonomous action, social row, event, or counter mutation; unaffected M07 Player replies/reactions/follows; no temporary relationship persistence; deterministic repeated eligibility results without rule-specific fallback randomness; and no M10 tables/entities/fields.
- Verify existing/new-world `UTC` defaults, UTC and non-UTC IANA schedule evaluation, quiet-hour conversion boundaries, different-zone eligibility for one UTC instant, deterministic repeat behavior, host-timezone independence, DST conversion from UTC, invalid timezone rejection, and no per-character timezone state.
- Verify ACT-01 unavailable separately for missing goal, numeric mood, and event inputs; POST-01 unavailable for incomplete mandatory weighted inputs; no zero substitution, weight redistribution/renormalization, autonomous post, feed mutation, proxy persistence, or random draw after mandatory-input failure; stable retry/rule-version/PRNG sequencing; and unchanged M06/M07 Player posts.
- Verify one evaluation per run/rule; deterministic IDs/outcomes/primary reasons; independent rules/runs/worlds; duplicate insert rejection or replay reuse; same-world concurrent dedupe; rollback leaves no orphan; unavailable does not fail the run; actual run failure uses SimulationRun.ErrorCode; and no candidate-level diagnostic persistence.
- Verify first-tick bootstrap, before-due and exact-due boundaries, legacy `NextDueAt` normalization, one exact interval/run per trigger, the documented 31-minute two-trigger progression, no M08 multi-interval batching, completed replay, failed-transaction retry of the same interval, same-world concurrency loser behavior, independent worlds, and atomic cursor/due advancement.
- Verify creation-time `CurrentWorldTime` and non-null compatibility `LastSimulatedAt` bootstrap with null `LastCompletedIntervalEnd` as the never-simulated authority; exact scale `1.0000`, `2.0000`, and supported fractional `0.5000` advancement; per-run effective-scale persistence; setting changes between intervals without retroactive recomputation; one scaled advancement per overdue trigger; `LastSimulatedAt == LastCompletedIntervalEnd` after commit but not processing-now; schedule/quiet-hour projection from resulting `CurrentWorldTime`; replay, rollback, and same-world concurrency without double advancement; and host timezone/latency independence.

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
