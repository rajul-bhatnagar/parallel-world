# M14 — World events and trends

## Goal
Add full fictional world-event, life-event, and trend systems only after Version 1 activation.

## User-visible result
When approved, released fictional events influence characters and auditable trends appear.

## Dependencies
M08 and M10; explicit post-MVP product activation. This is not a hard dependency of M15.

## Scope
- **Backend:** Version 1 templates/lifecycles, trend scoring/snapshots, actor reactions and eligible posts; no real-news ingestion.
- **Database:** Topics decision first; then WorldEvents, CharacterLifeEvents, Trends/Snapshots, same-world constraints/status/uniqueness, migration.
- **Flutter:** Event/trend projections only when approved; no required “Explore” surface without a UX/product decision.
- **Infrastructure:** None beyond existing workers; no news service.

## Explicit exclusions
MVP implementation, real-world news, advanced politics/economy, multiple cities.

## Test scope
Eligibility/scoring/lifecycle/caps, interest matching, duplicate trend, world isolation, deterministic reactions, UI states when released.

## Security and ownership considerations
Release authorization, deterministic mechanics, event frequency, deferred-feature leakage. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
While gated, no code is created. After activation, events/trends are deterministic, auditable, world-local, and tested.

## Required verification
Pre-activation source-decision audit; after activation full rule/PostgreSQL/API/Flutter suites. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
After activation only: event start/end, actor reaction, trend start/end, foreign world.

## Exit criteria
Either documented intentional deferral, or approved Version 1 slice passes all gates.
