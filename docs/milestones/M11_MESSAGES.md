# M11 — Private messaging

## Goal
Deliver persistent one-to-one player/character conversations.

## User-visible result
Player opens one direct conversation per character, sends messages, reads history offline, and receives an eligible immediate reply or explicit no-response state.

## Dependencies
M03-M05 and M08-M10.

## Scope
- **Backend:** Conversation resolve/create, participation, message send/read/cursor, MSG-02 deterministic reply eligibility, persisted planned action, immediate MVP AI/template reply, notification hook.
- **Database:** Conversations, Participants, Messages, composite conversation/read-cursor FKs, unique active player-character pair, client-operation uniqueness, migration.
- **Flutter:** Conversation list/chat/composer, paging, cache, pending/failed/retry, approved reply state, polling/refresh.
- **Infrastructure:** Durable wording work through existing BackgroundService pattern; realtime not required.

## Explicit exclusions
Simulated delayed replies, character initiation, follow-up, group chat, editing/deletion, SignalR requirement.

## Test scope
Conversation uniqueness, sender/membership, duplicate client ID, order/cursor, eligibility/fallback, ownership, pending retry, log redaction, and screen states. MSG-02 coverage fixes every Player-authored message at neutral `Urgency=50` on the 0-100 scale and `ConflictAvoidancePenalty=0`; wording, punctuation, length, client input, provider state/output, fallback, and Rivalry alone cannot change those mechanics. Same mechanical state with different text produces the same eligibility evaluation.

## Security and ownership considerations
Private-body logging, participant/world checks, idempotency, full-history exclusion. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Messages persist and paginate; one logical send creates one message; eligible reply mechanics precede wording; offline history is cache-only.

## Required verification
Rule/API/PostgreSQL/privacy/Flutter repository-provider-widget tests. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
New/existing conversation, send timeout/retry, fallback/no-response, offline history, foreign conversation.

## Exit criteria
Persistent private messaging works without unreleased timing behavior.

## Implementation readiness
Unblocked by ADR-027. M11 v1 uses fixed neutral `Urgency=50` and `ConflictAvoidancePenalty=0`; richer intent, urgency, conflict, and memory-aware mechanics remain deferred.
