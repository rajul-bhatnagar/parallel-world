# M07 — Reactions, replies, and follows

## Goal
Add released social interactions.

## User-visible result
Player replies, likes/unlikes, and follows/unfollows AI characters.

## Dependencies
M06.

## Scope
- **Backend:** Player replies, MVP Like, follow edges, idempotent/natural PUT-DELETE behavior, count projections, same-world/self/actor checks.
- **Database:** PostReactions and Follows plus reply use of Posts; unique active edges/actions, composite FKs, migration.
- **Flutter:** Reply flow/thread where required, like/follow optimistic state, rollback, pending/failure states.
- **Infrastructure:** None.

## Explicit exclusions
Reposts/quotes/bookmarks/hashtags/mentions, AI autonomous action, rich notifications.

## Test scope
Duplicate/natural idempotency, self-follow, wrong actor/world, reply depth/cursor, counts, rollback/reconcile, widgets.

## Security and ownership considerations
Server-derived actor, uniqueness, cached counts, optimistic rollback. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Each action produces at most one persisted effect; Flutter cannot act as an AI actor.

## Required verification
Rule/unit, API/PostgreSQL ownership/constraint, Flutter provider/widget tests. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Like/unlike, reply, follow/unfollow, retry, foreign actor/post.

## Exit criteria
Released social actions stable and idempotent.
