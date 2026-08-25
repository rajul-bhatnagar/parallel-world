# M12 — Long-term memory

## Goal
Create and retrieve meaningful structured character memories.

## User-visible result
Character wording can reference relevant prior interactions without exposing unrelated secrets.

## Dependencies
M10-M11 and GameplayEvent provenance.

## Scope
- **Backend:** MEM-01/02 creation/ranking/recall, expiry/reinforcement/contradiction, secrets/knowers, promises/resolution, authorized bounded AI context.
- **Database:** CharacterMemories, Secrets/SecretKnowers, Promises, RecallRequests/Selections with composite provenance/access FKs, checks/uniques/indexes, migration.
- **Flutter:** No raw memory browser; only sanitized history/wording projections and ordinary states.
- **Infrastructure:** No new external service; existing AI work receives bounded references.

## Explicit exclusions
AI-created authoritative memory, full transcript storage in memory, broad client endpoint, cross-world knowledge.

## Test scope
Thresholds, mandatory types, ranking/tie/cap, expiry, provenance, knower access, promise states, duplicate prevention, no full chat history, secret leakage.

## Security and ownership considerations
Knowledge provenance, secret privacy, bounded context, duplicate/reinforcement semantics. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Only qualified events create idempotent memories; recall is bounded/reproducible/authorized; AI failure changes no memory.

## Required verification
Rule/scenario/PostgreSQL/security/AI-context tests and safe projection checks. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Create meaningful/trivial interaction; inspect relevant recall and secret exclusion; force fallback.

## Exit criteria
Memory continuity works with no unauthorized context.
