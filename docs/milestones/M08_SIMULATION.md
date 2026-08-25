# M08 — Rule-based simulation

## Goal
Advance autonomous character activity deterministically without external AI.

## User-visible result
The private world gains reproducible character posts/replies/likes/follows expressed by deterministic templates.

## Dependencies
M05-M07.

## Scope
- **Backend:** Injected clock/PRNG, world time, SimulationRun/Action, ACT/POST/REPLY/REACT/FOLLOW rules, stable ordering, reasons/statuses, idempotent half-open intervals, template fallback, checkpoint-safe execution.
- **Database:** Runs/actions/idempotency/work records, exact interval uniqueness, cursor locking, composite target FKs, migration.
- **Flutter:** Development-only trigger only if securely gated; status and authoritative refresh.
- **Infrastructure:** BackgroundService may process durable PostgreSQL work; in-memory queue is not authority.

## Explicit exclusions
External AI, catch-up compression, relationships/memory/dating, real-time delivery.

## Test scope
Same-state/interval/version/seed equality, candidate-order independence, caps/cooldowns/reasons, duplicate/overlap, rollback/partial resume, cross-world targets.

## Security and ownership considerations
Uncontrolled time/randomness, ordering, idempotency, AI absence, `WorldId`. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Same inputs give identical mechanics; duplicate interval gives no duplicate effect; no provider is required.

## Required verification
Unit/scenario/PostgreSQL/concurrency/architecture/security tests and deterministic snapshot comparison. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Run fixed seed twice from restored fixture; inspect reasons/template posts; retry interval.

## Exit criteria
Reproducible auditable rules pass independent simulation review.
