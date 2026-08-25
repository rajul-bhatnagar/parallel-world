# M05 — Character catalogue

## Goal
Create and display a deterministic initial cast.

## User-visible result
Approximately 10 distinct same-world characters can be listed and viewed, including cached offline summaries.

## Dependencies
M03 ownership and M04 app shell.

## Scope
- **Backend:** Reuse the Actor abstraction created in M03; add character Actors, Characters, traits, interests, opinions, basic moods/schedules/profession, list/detail projections, hidden-field filtering.
- **Database:** Character/detail tables, bounds, handles, same-world composite FKs/indexes, EF migration.
- **Flutter:** Catalogue/profile, navigation, cache, loading/empty/error/offline states.
- **Infrastructure:** None beyond migration in CI.

## Explicit exclusions
Autonomous actions, relationships, feed, AI-generated character creation, trait evolution.

## Test scope
Seed reproducibility, score constraints, one-world scope, hidden-field DTO contract, cursor/catalogue mapping, widget/cache states.

## Security and ownership considerations
Hidden state disclosure, deterministic seed, Actor/detail integrity, cache scope. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Cast is stable for seed/version, distinct, private, safely projected, and cache-readable offline.

## Required verification
Unit, PostgreSQL/migration, API ownership/contract, Flutter mapping/provider/widget tests. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Create world, inspect about 10 profiles, offline revisit, foreign character link.

## Exit criteria
Character vertical slice passes automated/manual acceptance.
