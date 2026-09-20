# M08 — Rule-based simulation

## Goal
Advance autonomous character activity deterministically without external AI.

## User-visible result
The private world gains reproducible character posts, replies, and likes expressed by deterministic templates. Autonomous Character follows remain deterministically ineligible until M10 relationship state exists; M07 Player follow/unfollow remains available.

## Dependencies
M05-M07.

## Scope
- **Backend:** Injected clock/PRNG, UTC world time, deterministic projection through the world-level IANA display timezone for schedules/quiet hours, SimulationRun/Action, ACT/POST/REPLY/REACT rules, `FOLLOW-01` unavailable/ineligible evaluation without M10 relationship state, stable ordering, reasons/statuses, idempotent half-open intervals, template fallback, checkpoint-safe execution.
- **Database:** Runs/actions/idempotency/work records, exact interval uniqueness, cursor locking, composite target FKs, and required `WorldSettings.DisplayTimeZoneId` persisted as `display_time_zone_id`; the migration backfills existing worlds and defaults new worlds to `UTC`.
- **Flutter:** Development-only trigger only if securely gated; status and authoritative refresh.
- **Infrastructure:** BackgroundService may process durable PostgreSQL work; in-memory queue is not authority.

## Explicit exclusions
External AI, catch-up compression, relationships/memory/dating, real-time delivery, temporary or proxy relationship inputs, and autonomous follow activation before M10.

## Test scope
Same-state/interval/version/seed equality, candidate-order independence, caps/cooldowns/reasons, duplicate/overlap, rollback/partial resume, and cross-world targets. Verify existing/new-world `UTC` defaults; UTC and non-UTC IANA schedule projection; quiet-hour boundaries after conversion; different eligibility for the same UTC instant in different zones; deterministic host-independent repeat evaluation; timezone-database DST projection from UTC; invalid-zone rejection; and absence of per-character timezone state. Verify that `FOLLOW-01` is ineligible without relationship state, simulation creates no autonomous follow action or row, M07 Player follows remain unaffected, no temporary relationship state is persisted, repeated evaluation preserves the same follow-ineligible result without fallback randomness, and no M10 tables/entities/fields are introduced.

## Security and ownership considerations
Uncontrolled time/randomness, ordering, idempotency, AI absence, `WorldId`. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Same inputs give identical mechanics; duplicate interval gives no duplicate effect; no provider is required. Missing M10 relationship state deterministically produces no autonomous follow action or follow effect while M07 Player follow/unfollow continues to work.

## Required verification
Unit/scenario/PostgreSQL/concurrency/architecture/security tests and deterministic snapshot comparison. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Run fixed seed twice from restored fixture; inspect reasons/template posts; retry interval.

## Exit criteria
Reproducible auditable rules pass independent simulation review.
