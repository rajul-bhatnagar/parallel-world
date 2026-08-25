# M13 — Dating and relationship history

## Goal
Add the PRODUCT.md-approved basic romantic invitation, outcome, Dating state, and necessary history.

## User-visible result
Eligible player can invite a character, see persisted acceptance/rejection, Dating status, and the necessary invitation/outcome timeline.

## Dependencies
M10-M12 and unresolved romance/content policy decisions required before broad release.

## Scope
- **Backend:** ROM-01/02, canonical pair, invitation/cooldown/outcome, shared Dating status and necessary invitation/outcome history, rule-created memories where released, wording after outcome.
- **Database:** RomanticRelationships and RomanticStatusHistory plus invitation fields/records, canonical ordering, composite FKs/uniques/indexes, migration.
- **Flutter:** Invitation action, pending/result, safe eligibility feedback, status/timeline, loading/empty/error/offline states.
- **Infrastructure:** None.

## Explicit exclusions
Breakup lifecycle, FormerPartner re-entry, reconciliation/cooldowns/cycling, commitment beyond Dating, engagement, marriage, separation, divorce, children, and family simulation.

## Test scope
Thresholds, compatibility/cooldown, duplicate/concurrent invite, acceptance/rejection, Dating, canonical pair, required history/memory/ownership/UI, and rejection of deferred transitions.

## Security and ownership considerations
Consent/content boundaries, canonical status, client mechanical fields, history consistency. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Server rules decide one outcome and history; AI only phrases it; forbidden paths fail safely.

## Required verification
Rule/scenario/PostgreSQL/API/security/Flutter tests and romance-content review. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Eligible/ineligible/rejected/accepted path, duplicate retry, required romantic history, deferred-transition rejection, foreign character.

## Exit criteria
Basic dating vertical slice is auditable, safe, and phase-correct.
