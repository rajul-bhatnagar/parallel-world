# M15 — Catch-up simulation

## Goal
Advance an Active world after absence through bounded deterministic compression.

## User-visible result
Returning player sees reliable world progress and a concise “while you were away” summary.

## Dependencies
M08 and released M10-M13 behavior. Full M14 is not required; seeded MVP topics remain sufficient.

## Scope
- **Backend:** CATCH-01 elapsed-time calculation; one bounded CatchUp `SimulationRun` per CatchUp operation beyond M08's unchanged one-trigger/one-15-minute-run primitive; deterministic accumulated world-time advancement across the consumed range; batching, compression, caps, prioritization, yielding, internal six-hour/daily buckets, relational checkpoint/Partial state, summary facts/wording, duplicate/concurrent resume, and cursor updates.
- **Database:** Catch-up run/bucket checkpoint and world-summary/item persistence as required, constraints/indexes/idempotency, migration.
- **Flutter:** Resume/progress/partial/error state, summary and safe links to released posts/characters/conversations; no unreleased event link.
- **Infrastructure:** Durable bounded background processing and lease recovery using PostgreSQL.

## Explicit exclusions
Simulating every minute, full trend updates without M14 approval, unbounded catch-up, AI-invented summary facts.

## Test scope
Paused/archived exclusion, one CatchUp-run ownership, bounded batching and limits, deterministic compression/caps/bucket order, unchanged normal M08 granularity, yielding, relationship/message effects, checkpoint retry of the same logical run, concurrent resume, summary correctness/fallback, and UI states. CatchUp is not compared to repeated M08 triggers by run count, action count, or PRNG draw sequence.

## Security and ownership considerations
Cursor correctness, bounded work, transaction checkpoints, priority, AI independence. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
The same CatchUp snapshot/range/version/seed and checkpoint state yields the same compressed committed progress; retry resumes the same logical CatchUp run and duplicates nothing; summary contains only committed facts; normal M08 processing remains one 15-minute interval per run.

## Required verification
Fixed-seed scenario, PostgreSQL concurrency/recovery, API idempotency, Flutter provider/widget/integration tests. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
No/short/long absence, forced partial failure/retry, concurrent resume, fallback summary.

## Exit criteria
The core MVP gameplay loop is demonstrable and stable; release gates remain.
