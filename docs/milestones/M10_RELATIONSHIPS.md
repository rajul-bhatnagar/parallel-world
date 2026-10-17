# M10 — Relationship engine

## Goal
Persist deterministic directional relationships and explain meaningful changes.

## User-visible result
Friendship/rivalry/attraction summaries and recent history respond to interactions.

## Dependencies
M08; M09 only for phrasing, never mechanics.

## Scope
- **Backend:** Exactly nine directional REL-01 dimensions—Familiarity, Trust, Respect, Affection, Comfort, Rivalry, Jealousy, Attraction, and Commitment—plus events, caps/ledgers/asymmetry, derived labels, same-world application contracts, deterministic InterestOverlap and RelationshipRelevance, persisted neutral Character Reputation, and autonomous-rule transitions according to ADR-025/026. Shared romance status is not stored in directional rows.
- **Database:** Relationships, RelationshipEvents, daily ledgers, bounds/uniques/composite FKs/history indexes, migration.
- **Flutter:** Safe qualitative summary/recent history with loading/empty/error/offline states; no hidden raw scores unless approved.
- **Infrastructure:** None.

## Explicit exclusions
Romantic pair transitions, dating, marriage/divorce, client-authored deltas, passive MVP decay.

## Test scope
Nine-dimension initial values, Character Reputation default/backfill/bounds/independence, deltas/neutral-v1-multiplier defaults and approved exceptions/clamps/daily caps, asymmetry, labels/priority, duplicate event, transaction rollback, ownership, UI projection, InterestOverlap, RelationshipRelevance, social-action/no-relationship-effect separation, FOLLOW-01 activation, and deterministic REPLY-01/REACT-01 remaining gates.

## Implementation readiness
Unblocked by ADR-026. Character Reputation is approved; initial Follow/Like/Unlike/generic Reply intentionally have no M10 v1 REL-01 effect; FOLLOW-01 is fully evaluable; REPLY-01 and REACT-01 retain exact deterministic non-relationship gates without blocking the M10 relationship engine.

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
