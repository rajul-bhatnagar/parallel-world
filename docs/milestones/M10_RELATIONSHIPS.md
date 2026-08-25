# M10 — Relationship engine

## Goal
Persist deterministic directional relationships and explain meaningful changes.

## User-visible result
Friendship/rivalry/attraction summaries and recent history respond to interactions.

## Dependencies
M08; M09 only for phrasing, never mechanics.

## Scope
- **Backend:** REL-01 dimensions/events, caps/ledgers/asymmetry, derived labels, same-world application contracts; shared romance status is not stored in directional rows.
- **Database:** Relationships, RelationshipEvents, daily ledgers, bounds/uniques/composite FKs/history indexes, migration.
- **Flutter:** Safe qualitative summary/recent history with loading/empty/error/offline states; no hidden raw scores unless approved.
- **Infrastructure:** None.

## Explicit exclusions
Romantic pair transitions, dating, marriage/divorce, client-authored deltas, passive MVP decay.

## Test scope
Initial values, deltas/multipliers/clamps/daily caps, asymmetry, labels/priority, duplicate event, transaction rollback, ownership, UI projection.

## Security and ownership considerations
Formula fidelity, transaction/idempotency, hidden-score privacy, separation of romance. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Qualified events create one auditable directional change; history explains it; no romantic status column exists here.

## Required verification
Rule/scenario/PostgreSQL/ownership/API/Flutter tests and architecture review. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Positive/negative interaction, asymmetric projection, cap boundary, foreign actor.

## Exit criteria
Relationship slice passes game-rule/database/Flutter review.
