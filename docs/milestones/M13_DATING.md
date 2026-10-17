# M13 — Dating and relationship history

## Goal
Add the PRODUCT.md-approved basic romantic invitation, outcome, Dating state, and necessary history.

## User-visible result
Eligible player can invite a character, see persisted acceptance/rejection, Dating status, and the necessary invitation/outcome timeline.

## Dependencies
M10-M12 and accepted ADR-030. Broader romance/content-rating policy remains a release gate, not an M13 mechanics blocker.

## Scope
- **Backend:** ADR-030 ROM-01/02, structured romance availability/modes, canonical pair, strict Dating exclusivity, `CasualDate`, 14-game-day cooldown, 24-game-hour invitation expiry, explicit Player outcomes, immediate deterministic Character outcomes, bilateral one-time Commitment +10, shared Dating status and necessary invitation/outcome history, and wording only after outcome.
- **Database:** World/profile romance fields, RomanticRelationships, RomanticInvitations, RomanticStatusHistory, canonical ordering, Actor-level unresolved-invitation/Dating conflict protection, composite FKs/uniques/indexes, and migration.
- **Flutter:** Invitation action, pending/result, safe eligibility feedback, status/timeline, loading/empty/error/offline states.
- **Infrastructure:** None.

## Explicit exclusions
Breakup lifecycle, Dating end, FormerPartner re-entry, re-dating, reconciliation/cooldowns/cycling, commitment stage beyond Dating, additional preference dimensions/date types, engagement, marriage, separation, divorce, children, and family simulation.

## Test scope
Romance enabled/disabled, both romance modes, exact compatibility and neutral inputs, reason precedence, thresholds, cooldown, duplicate/concurrent invite, Actor-level exclusivity, 24-game-hour expiry, explicit Player outcome, immediate deterministic Character outcome, `CasualDate`, Dating, bilateral Commitment +10/clamp/replay, canonical pair, append-only history, persistence-level preservation of seeded distinct ended/re-dated episodes without enabling their transitions, ownership/UI, AI/text/memory invariance, and rejection of deferred transitions.

## Security and ownership considerations
Consent/content boundaries, canonical status, client mechanical fields, history consistency. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Server rules decide one replay-safe outcome, transition, Commitment effect, and history; AI only phrases it; forbidden paths fail safely.

## Required verification
Rule/scenario/PostgreSQL/API/security/Flutter tests and romance-content review. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Eligible/ineligible/rejected/accepted path, duplicate retry, required romantic history, deferred-transition rejection, foreign character.

## Exit criteria
Basic dating vertical slice is auditable, safe, and phase-correct.
