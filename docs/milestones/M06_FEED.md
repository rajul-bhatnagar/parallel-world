# M06 — Social feed

## Goal
Deliver the private text feed and player post creation.

## User-visible result
Player reads character/player posts, paginates, refreshes, creates a post, and sees cache offline.

## Dependencies
M05; resolve chronological versus deterministic ranked ordering before final feed implementation. Until then use the approved current chronological contract only if explicitly accepted.

## Scope
- **Backend:** Posts/reply-ready parent structure, author actor, GameplayEvent provenance, world feed, opaque cursor, player post idempotency, deterministic seeded character-post fixtures.
- **Database:** Posts, composite actor/event/parent FKs, active feed/author/reply cursor indexes, count checks, migration.
- **Flutter:** Feed/post card/composer, pull-to-refresh, next-page state, Drift cache, optimistic player post, failed retry.
- **Infrastructure:** None beyond migration/test data.

## Explicit exclusions
Quote/repost/hashtags/mentions/rich reactions, feed impressions, autonomous posting, public/global feed.

## Test scope
Order/tie-break/cursor scope, duplicate pages, idempotent create, cross-world parent/author denial, optimistic reconciliation, all screen states.

## Security and ownership considerations
Cursor stability, author derivation, world isolation, pending reconciliation. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Stable private feed and one player post effect; cache is clearly stale/non-authoritative offline.

## Required verification
API/PostgreSQL/cursor/idempotency tests plus Flutter repository/provider/widget tests. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Empty and seeded feed, paging, offline cache, post timeout/retry, foreign post ID.

## Exit criteria
Feed vertical slice stable with no deferred social features.
