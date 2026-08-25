# M15 — Catch-up simulation

## Goal
Advance an Active world after absence through bounded deterministic compression.

## User-visible result
Returning player sees reliable world progress and a concise “while you were away” summary.

## Dependencies
M08 and released M10-M13 behavior. Full M14 is not required; seeded MVP topics remain sufficient.

## Scope
- **Backend:** CATCH-01 elapsed-time calculation, six-hour/daily buckets, caps/priorities, checkpoint/Partial state, summary facts/wording, duplicate/concurrent resume, cursor updates.
- **Database:** Catch-up run/bucket checkpoint and world-summary/item persistence as required, constraints/indexes/idempotency, migration.
- **Flutter:** Resume/progress/partial/error state, summary and safe links to released posts/characters/conversations; no unreleased event link.
- **Infrastructure:** Durable bounded background processing and lease recovery using PostgreSQL.

## Explicit exclusions
Simulating every minute, full trend updates without M14 approval, unbounded catch-up, AI-invented summary facts.

## Test scope
Paused/archived exclusion, compression/caps/order, relationship/message effects, checkpoint retry, concurrent resume, summary correctness/fallback, UI states.

## Security and ownership considerations
Cursor correctness, bounded work, transaction checkpoints, priority, AI independence. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Same snapshot/interval/version/seed yields same committed progress; retry duplicates nothing; summary contains only committed facts.

## Required verification
Fixed-seed scenario, PostgreSQL concurrency/recovery, API idempotency, Flutter provider/widget/integration tests. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
No/short/long absence, forced partial failure/retry, concurrent resume, fallback summary.

## Exit criteria
The core MVP gameplay loop is demonstrable and stable; release gates remain.
