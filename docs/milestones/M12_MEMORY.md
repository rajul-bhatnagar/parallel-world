# M12 — Long-term memory

## Goal
Create and retrieve meaningful structured character memories.

## User-visible result
Character wording can reference relevant prior interactions without exposing unrelated secrets.

## Dependencies
M10-M11 and GameplayEvent provenance.

## Scope
- **Backend:** Deterministic creation of exactly Fact, Preference, Event, Secret, and Promise from authoritative structured sources; exact MEM-02 ranking; 100-active-memory retention; Secret/Promise lifecycle; authorized bounded AI context after mechanical decisions.
- **Database:** CharacterMemories, provenance-keyed MemoryCreationOutcomes, Secrets/SecretKnowers, Promises, and RecallRequests/Selections with world/owner scope, typed provenance/subjects, canonical topics, source uniqueness, checks, and composite access FKs.
- **Flutter:** No raw memory browser; only sanitized history/wording projections and ordinary states.
- **Infrastructure:** No new external service; existing AI work receives bounded references.

## Explicit exclusions
AI-created or text-inferred authoritative memory, arbitrary topic strings, embeddings/vector search, Secret disclosure pressure/leakage, full transcript storage in memory, raw client endpoints, cross-world/cross-Character knowledge, and M13 behavior.

## Exact v1 mechanics
- Source metadata must provide the owner Character's knowledge, an authoritative subject identifier, source identity, and explicit memory category. Missing owner knowledge or subject means no memory.
- Confidence is 100 for structured gameplay Fact, Preference, Secret, and Promise; 90 for an explicitly memorable structured Player-authored statement or authoritative gameplay Event. Importance is Fact=60, Preference=60, Event=50, Secret=90, Promise=90. Visibility is CharacterPrivate.
- `RecallScore = 0.40*SubjectMatch + 0.30*TopicMatch + 0.20*RelationshipMatch + 0.10*Importance`, with 0-100 inputs, exact decimal arithmetic, and one final integer rounding step midpoint-away-from-zero. Exact subject/topic ID matches score 100, otherwise 0; a missing topic scores 0. RelationshipMatch is M10 RelationshipRelevance from recalling Character to Actor subject, or 0 for a non-Actor subject.
- Recall order is score DESC, CreatedAtUtc DESC, Id DESC and is capped at the existing `MAX_MEMORIES_PER_AI_REQUEST=8`.
- Ordinary memories do not expire automatically. The active cap is 100 per Character; non-protected eviction is Importance ASC, CreatedAtUtc ASC, Id ASC. Active Secrets and Promises are protected. If all 100 active memories are protected, reject only the new memory creation with `memory_capacity_protected`; preserve the source event/message and all existing memories.
- Secret disclosure is inactive. Promise status is Active, Fulfilled, Cancelled, or Expired, driven only by structured authoritative conditions/events.
- MSG-02 decides reply/no-reply before recall; M09 uses selected memory only for wording.

## Implementation readiness
ADR-029 resolves protected-only overflow. M12 has no remaining mechanical planning blocker.

## Test scope
Approved structured-source mapping and arbitrary-prose rejection; exact confidence/importance/visibility; exact SubjectMatch/TopicMatch/RelationshipMatch and formula rounding; score/time/ID ordering and top-eight bound; ordinary no-expiry and deterministic non-protected eviction; 99-protected-plus-one-non-protected eviction; 100-protected `memory_capacity_protected` rejection; unchanged protected rows; cap enforcement; deterministic rejected replay; Secret non-disclosure; structured Promise due/fulfillment/cancellation/expiry; replay uniqueness; world/owner isolation; no AI/provider capacity decision; no full chat history; and M11/M09 authority boundaries.

## Security and ownership considerations
Knowledge provenance, secret privacy, bounded context, duplicate/reinforcement semantics. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Only approved authoritative structured sources create idempotent memories; recall is bounded/reproducible/owner-authorized; ordinary memories do not auto-expire; Secrets do not spontaneously disclose; Promise transitions require authoritative conditions; AI failure or wording changes no mechanic.

## Required verification
Rule/scenario/PostgreSQL/security/AI-context tests and safe projection checks. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Create meaningful/trivial interaction; inspect relevant recall and secret exclusion; force fallback.

## Exit criteria
Memory continuity works with no unauthorized context and never exceeds the deterministic 100-active-memory cap.
