# M16 — Notifications and realtime

## Goal
Complete the MVP in-app notification subset; introduce realtime only after the open architecture/milestone choice is accepted.

## User-visible result
Released reply, private-message, and catch-up-summary indicators appear through a badge/deep link and bounded minimal list; missed updates recover through HTTP. If separately approved, foreground realtime reduces delay.

## Dependencies
M03, M07, M11, M13, M15.

## Scope
- **Backend:** Reply, PrivateMessage, and CatchUpSummary Notification categories; unread/bounded-minimal-list/cursor/read-one/read-all/dedupe. SignalR hub/group/events only under an accepted introduction decision.
- **Database:** Notifications with recipient ownership/event provenance/uniques/indexes and migration; realtime remains delivery, not authority.
- **Flutter:** List/badge/deep links/safe previews/offline states. Optional SignalR authenticates, dedupes, reconnects, refetches, and disconnects on logout.
- **Infrastructure:** None for HTTP/polling MVP; websocket hosting/config only if SignalR approved. Push remains deferred.

## Explicit exclusions
Follow and DatingInvitation indicators, push/FCM, rich Version 1 categories/filtering/search/history, and SignalR as mandatory MVP authority.

## Test scope
Ownership/dedupe/cursor/read state/safe preview; optional hub auth/wrong-world/duplicate/reconnect/refetch/minimal payload.

## Security and ownership considerations
Recipient ownership, sensitive preview, duplicate delivery, release gating. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Basic indicators are persisted/idempotent/private; HTTP recovers state; optional realtime never mutates gameplay.

## Required verification
API/PostgreSQL/security/Flutter tests; websocket tests only when applicable. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Unread/read/deep link/offline refresh; if applicable reconnect/logout/wrong-world.

## Exit criteria
MVP notification subset passes; realtime status is explicitly Implemented or Deferred.
