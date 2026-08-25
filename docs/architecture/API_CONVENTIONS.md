# Parallel World API Conventions

This document is the authoritative public HTTP API, realtime contract, pagination, validation, idempotency, and error-response specification for Parallel World. Product scope remains governed by `docs/product/PRODUCT.md`, mechanics by `docs/game-design/GAME_RULES.md`, application boundaries by `docs/architecture/ARCHITECTURE.md`, persistence by `docs/architecture/DATABASE.md`, and security by `docs/architecture/SECURITY.md`.

The contracts below describe interfaces, not implementation authorization. Endpoint release labels are **MVP**, **Version 1**, **Future**, or **Development-only**.

## 1. API principles

- Resource-oriented HTTPS APIs with JSON UTF-8 request and response bodies.
- PostgreSQL is authoritative; Flutter/Drift data is a replaceable cache and pending-action layer.
- Routes represent user and gameplay use cases, not one endpoint per database table.
- Authentication establishes the current user; authorization is enforced server-side for every use case.
- Contracts are stable, explicitly validated, and versioned for breaking changes.
- Growing collections use cursor pagination.
- Retryable writes are idempotent and safe for offline/mobile retries.
- Errors use RFC-compatible ProblemDetails with stable machine-readable codes.
- Every request has a server-recognized correlation ID; correlation and idempotency are distinct.
- Persistent and API timestamps use UTC.
- AI provider, prompt, model, seed, scoring, and hidden mechanics are not public contract fields.
- EF, database, and domain entities are never serialized directly.
- Realtime and push notify clients about persisted state; neither is an authority.

## 2. Base URL and versioning

The initial base path is:

```text
/api/v1
```

The major version is in the URL. Backward-compatible additions stay in `v1`; breaking field, meaning, requiredness, status-code, enum, or pagination changes require a new major version or an explicitly coordinated compatibility migration. There is no date-based version and no version per milestone.

Examples:

```text
/api/v1/worlds/current
/api/v1/worlds/{worldId}/feed
```

## 3. Authentication

Protected requests use:

```text
Authorization: Bearer <access-token>
```

Tokens never appear in URLs. Access tokens are short-lived. Refresh tokens are rotated, revocable, returned only at issuance/rotation, stored securely by Flutter, and never exposed as hashes.

MVP uses guest authentication through `POST /api/v1/auth/guest`, refresh, and logout. Registration, login, recovery, and guest upgrade are Version 1 contracts and must preserve the existing `UserId` and world history.

## 4. Authorization and world ownership

For every world-scoped request the server:

1. Derives `UserId` from validated authentication.
2. Treats `worldId` only as a locator.
3. Loads the world by both `worldId` and owner `UserId`.
4. Validates every referenced actor/resource belongs to that world.
5. Applies the same checks inside the application use case, not only at the endpoint.

Clients never submit `UserId` for authorization. Cross-world identifiers are rejected without disclosing whether the foreign resource exists.

- `401` means authentication is missing or invalid.
- `403` means an authenticated user is known to own the resource but lacks permission for the requested operation.
- `404 resource_not_available` is used for missing resources and ownership/resource-scope failures where existence disclosure would be unsafe.

Reusable rules include: user owns world; actor/character/post belongs to world; player actor is the world's human actor; conversation belongs to world and contains the player; relationship/memory/event belongs to world; notification belongs to the owning user; simulation targets the owned world.

## 5. Resource naming

- Lowercase plural nouns.
- Hyphen-separated multiword segments.
- UUID identifiers named `{resourceId}`.
- No database table or module implementation names in public routes.
- Verbs are reserved for genuine actions that do not fit resource creation/update.

Examples: `/worlds`, `/characters`, `/posts`, `/conversations`, `/relationship-events`, `/world-summaries`. Allowed actions include `/auth/guest`, `/auth/refresh`, `/auth/upgrade`, `/worlds/{worldId}/resume`, and notification read actions.

## 6. Route conventions

World-owned resources are normally nested under:

```text
/api/v1/worlds/{worldId}/...
```

This is the preferred public pattern even when IDs are globally unique. Any globally addressed operational resource must still resolve its `WorldId` and authorize ownership. Nested identifiers are never assumed to belong to the route world.

## 7. HTTP method conventions

- `GET`: read only.
- `POST`: create a resource or execute a non-CRUD action.
- `PUT`: set an idempotent singleton/edge state such as reaction, repost, or follow.
- `PATCH`: partial update; missing and explicit null have different meanings.
- `DELETE`: delete or logically remove an existing resource/edge.
- `HEAD`: only when a measured use case requires it.
- `OPTIONS`: framework/CORS handling.

`GET` never changes gameplay state. Read tracking uses an explicit write route.

## 8. Request conventions

- `Content-Type: application/json; charset=utf-8` for JSON bodies.
- JSON fields use lower camelCase.
- Request DTOs accept only documented fields; unknown fields are rejected with `400 validation_failed` to catch misspellings and forbidden server fields.
- Trim leading/trailing whitespace on ordinary text, then validate; preserve internal whitespace and private-message/post line breaks.
- Empty strings are invalid where null or omission is the semantic value.
- Nested `worldId` is omitted when the route already supplies it.
- Client-generated IDs are accepted only for documented retry/reconciliation use cases.
- The server controls owner, author/sender actor, timestamps, seeds, AI provider, mechanical values, notification recipient, and all AI-character actions.
- Safe optional `X-Client-Request-ID` values may be echoed in diagnostics; they do not authorize or deduplicate work.
- Clients may send `X-Correlation-ID` using 8-64 safe ASCII characters. The server validates and propagates it or generates a replacement, returns the effective value in `X-Correlation-ID`, and uses the same value as ProblemDetails `traceId`. It is never an authorization or idempotency value.

## 9. Response conventions

Single resources are returned directly, without `success` or `data` wrappers:

```json
{
  "id": "7fcde290-4426-4dab-a76f-60ee13af894f",
  "worldId": "3aa4bd64-68c7-4db3-a3e1-91d095d1877e",
  "createdAtUtc": "2026-07-31T18:20:00Z"
}
```

Collections use `{ "items": [], "nextCursor": null, "hasMore": false }`. Optional collection metadata must be documented and stable. `Location` accompanies `201` where a retrieval URL exists. Empty successful collections are `[]`, never null.

Success status meanings:

| Status | Use |
|---|---|
| `200 OK` | Reads, updates with a body, idempotent state results, completed actions |
| `201 Created` | Synchronous resource creation |
| `202 Accepted` | Durable asynchronous operation accepted; includes operation/status reference |
| `204 No Content` | Successful removal or bodyless state action |

## 10. DTO rules

- Separate request and response DTOs; no EF/domain entity serialization.
- Explicit nullability and immutable response shapes where practical.
- Summary and detail shapes are distinct; avoid universal DTOs.
- Public names describe the client concept, for example `CharacterSummaryResponse`, `FeedPostResponse`, `ConversationSummaryResponse`, `MessageResponse`, `RelationshipSummaryResponse`, and `WorldSummaryResponse`.
- Hidden goals, memories, secrets, decision weights, exact concealed attraction, prompts, provider diagnostics, internal error data, and database concurrency values are excluded.
- Embedded summaries are deliberately small; clients follow links/IDs or call detail endpoints.

## 11. Validation

Validation is layered:

1. Transport: malformed JSON, unsupported content type, invalid UUID/query shape.
2. DTO: requiredness, length, format, documented enum, date range.
3. Ownership: current user/world and same-world nested identifiers.
4. Domain: released state transition and GAME_RULES.md eligibility.
5. Database fallback: unique, FK, and check constraints mapped to safe typed errors.

DTO validators do not duplicate full gameplay rules. Current approved behavior follows `ARCHITECTURE.md`: malformed and validation failures return `400`; state-transition or current-state conflicts return `409`. `422` is reserved pending the open validation-status decision.

## 12. Error responses

| Status | Meaning |
|---|---|
| `400` | Malformed request, DTO validation, invalid/expired cursor |
| `401` | Missing/invalid authentication |
| `403` | Authenticated and resource ownership is known, but action forbidden |
| `404` | Missing or ownership-hidden resource |
| `409` | State, uniqueness, idempotency, or server-managed concurrency conflict |
| `412` | Reserved for failed `If-Match` if ETags are approved |
| `422` | Reserved; not currently used because `ARCHITECTURE.md` specifies `400` validation |
| `429` | Rate limit exceeded; include `Retry-After` |
| `500` | Sanitized unexpected failure |
| `502` | Invalid/unusable upstream response with no safe fallback |
| `503` | Temporary service/dependency unavailability with no safe fallback |

AI/provider failure normally yields validated deterministic fallback content and a successful gameplay response. It becomes a public `502/503` only when the requested text-only operation cannot safely be satisfied; committed mechanics are not rolled back.

## 13. ProblemDetails extensions

All errors use `application/problem+json` and RFC-compatible fields:

```json
{
  "type": "https://errors.parallel-world.example/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more fields are invalid.",
  "instance": "/api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/posts",
  "traceId": "01J44R2V95D37MP2J5H8GTS5JX",
  "code": "validation_failed",
  "errors": {
    "content": ["Content must not exceed 500 characters."]
  }
}
```

Extensions:

- `code`: required stable machine-readable code.
- `traceId`: required correlation value.
- `errors`: optional field-keyed map for validation.
- `retryAfterSeconds`: optional for retryable throttling/unavailability.
- `conflictResourceId`: optional only when ownership-safe.
- `currentVersion`: optional only if a public concurrency strategy approves it.

Never return stack traces, SQL/provider messages, tokens, prompts, secrets, or another user's resource existence.

Stable code catalogue:

- Authentication: `authentication_required`, `invalid_access_token`, `refresh_token_expired`, `refresh_token_reused`, `installation_revoked`.
- Authorization/not found: `world_access_denied`, `resource_not_available`.
- Validation: `validation_failed`, `invalid_cursor`, `invalid_state_transition`.
- Conflict: `idempotency_key_reused`, `duplicate_request`, `duplicate_reaction`, `already_following`, `conversation_already_exists`, `world_already_exists`, `simulation_interval_already_processed`, `concurrency_conflict`.
- Limits: `rate_limit_exceeded`, `ai_generation_limit_reached`.
- External: `ai_provider_unavailable`, `push_provider_unavailable`.

## 14. Idempotency

Retryable writes require:

```text
Idempotency-Key: <client-generated-value>
```

- 8-100 ASCII characters from letters, digits, `.`, `_`, `:`, and `-`; random UUID/ULID values are recommended.
- Persisted scope is `(UserId, operation, key)` as required by DATABASE.md. Guest creation first resolves or transactionally creates the user through the unique installation identity, then stores/loads the user-scoped idempotency result; installation identity alone never authorizes the replay.
- The request fingerprint covers method, normalized route, relevant content type, and canonical request body.
- Same key/fingerprint returns the original status, safe body, `Location`, and concurrency headers with `Idempotency-Replayed: true`.
- Same key with a different fingerprint returns `409 idempotency_key_reused`.
- Correlation IDs never substitute for idempotency keys.
- Clients persist and reuse the logical operation key across timeout/offline retries.
- Exact retention duration remains open; it must cover the supported mobile retry window and is published operationally before offline writes ship.

Required for guest session, world creation, post/reply/quote creation, message send, date invitation, world resume, account registration/upgrade, and other retryable POST actions. `PUT` reaction/repost/follow and read-state actions are naturally idempotent and return current state/no-op success on repetition.

## 15. Concurrency

Append-only writes use idempotency/uniqueness. Simulation, relationships, and AI work use server-managed concurrency and expose no database token. Editable profile/world settings require an opaque public concurrency token, but the final ETag-versus-version representation is open.

If ETags are approved, `ETag` and `If-Match` use an encoded opaque value, never raw PostgreSQL `xmin`; failed preconditions return `412`. Server-side state-transition races independent of a supplied precondition return `409 concurrency_conflict`.

## 16. Cursor pagination

```json
{
  "items": [],
  "nextCursor": "opaque-value",
  "hasMore": true
}
```

- Cursor is opaque, versioned, scope/filter-bound, safely encoded or integrity protected, and validated server-side.
- It contains the stable ordering tuple plus world/resource scope and filter fingerprint.
- A changed filter/sort requires a new first-page request.
- Invalid, mismatched, tampered, or unsupported cursors return `400 invalid_cursor`.
- `limit` is optional, positive, and capped; `Id` is the final tie-breaker.
- Page queries seek after the tuple; offset pagination is prohibited for growing histories.
- Flutter deduplicates by ID because realtime insertion and concurrent writes can alter later pages.

| Collection | Default | Maximum | Server order |
|---|---:|---:|---|
| Feed | 20 | 50 | `createdAtUtc DESC, id DESC` until feed-ranking decision changes it |
| Messages | 30 | 100 | `createdAtUtc DESC, id DESC`; Flutter may reverse for display |
| Notifications | 30 | 100 | `createdAtUtc DESC, id DESC` |
| Relationship history | 25 | 100 | `occurredAtUtc DESC, id DESC` |
| Characters | 20 | 100 | stable server-defined catalogue order ending in `id` |

Cursor encoding and expiry remain open implementation decisions; clients must never decode cursors.

## 17. Filtering and sorting

Only documented filters are allowed. Unknown filters/sorts return `400 validation_failed`.

- Characters: `status`, optionally approved `topicId`.
- Posts: `authorActorId`, `parentPostId`.
- Notifications: `isRead`, `category`.
- Predefined sort values may include `recent`; `popular` is available only after its authoritative ordering is approved.

No arbitrary properties, SQL-like expressions, or internal column names are accepted.

## 18. Date, time, and timezone handling

- ISO 8601 UTC, for example `2026-07-31T18:20:00Z`.
- Ambiguous timestamp names end in `Utc`, such as `createdAtUtc`, `currentGameTimeUtc`, `lastSimulatedAtUtc`, and `realTimeObservedAtUtc`.
- Date-only values use `YYYY-MM-DD`.
- Local time preferences include an approved timezone identifier.
- Ambiguous local timestamps without timezone/offset are rejected.
- Game time and observed real time are separate named fields; Flutter converts for display.

## 19. Enum handling

JSON enums are stable lower camelCase strings, for example `dating`, `character`, `privateMessage`, and `fallback`. Numeric database values are never exposed. Flutter must preserve/handle unknown response values safely. New values are compatible only where clients implement an unknown fallback; semantic changes require version review.

## 20. Null and optional-field handling

- For PATCH, missing means “not supplied”; explicit null means “clear” only for documented nullable fields.
- Required fields cannot be null.
- Responses use null only when absence is semantically valid.
- Empty collections are `[]`, never null.
- Empty strings do not stand in for null.
- Server may omit an optional response field only when the contract declares it optional; it does not change a field between missing and null casually.

## 21. Content-length limits

Limits count Unicode user-perceived characters after normalization unless implementation constraints require a documented stricter byte ceiling. AI/fallback text obeys the same released limit.

| Field | Initial maximum |
|---|---:|
| Player post/reply/quote comment | 500 (`POST_MAX_CHARACTERS`) |
| Private message | 2,000 |
| Display name | 60 |
| Handle | 30 |
| Bio | 300 |
| World name | 80 |
| Search query | 100 |
| Idempotency key | 100 |

Global HTTP body/header limits are configured defensively and documented operationally; media upload limits are deferred with media support.

## 22. Rate limiting

Policies are grouped by authentication (guest/login/refresh/reset), content (post/reply/message/follow/reaction), AI-heavy (message reply generation/resume/simulation), and operational traffic. Limits combine per-user, per-installation, and per-IP fallback dimensions; anonymous traffic is stricter.

Responses use `429`, `Retry-After`, `retryAfterSeconds`, and `rate_limit_exceeded` or `ai_generation_limit_reached`. Public responses never reveal internal AI budgets. Exact numbers are tunable and remain open; health checks are separately controlled so orchestration probes are not blocked by user limits.

## 23. Realtime SignalR conventions

Planned hub:

```text
/hubs/world
```

The bearer-authenticated server authorizes each requested world group from current ownership. Event names are lower camel/dot names:

```text
post.created
reply.created
message.created
notification.created
relationship.updated
world.simulationCompleted
world.summaryCreated
```

Payloads contain `eventId`, `worldId`, safe resource IDs, `occurredAtUtc`, optional minimal summary/version, and `contractVersion`. They contain no prompts, provider details, secrets, private memory, hidden scores, or mechanical decision data. Clients refetch authoritative HTTP resources. Delivery is after commit; missed or duplicate events are harmless, and reconnect uses HTTP/cursors. SignalR never mutates gameplay. Introduction milestone remains open.

## 24. Push-notification API boundaries

Push is Version 1. Device routes register/revoke the authenticated user's installation/token; they never accept `UserId`. Persisted in-app Notification creation occurs before a separate delivery attempt. Push payloads contain minimal nonsensitive category/deep-link IDs, not private message text. Provider failure does not change gameplay or notification persistence.

## 25. Guest-session endpoints

```text
POST /api/v1/auth/guest
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
```

Guest creation accepts installation ID, platform, app version, and required idempotency key. Installation identity alone is not authorization. Refresh rotates tokens; logout revokes the presented refresh session/device scope as defined by security policy.

**Example 1 — guest session creation**

```http
POST /api/v1/auth/guest
Idempotency-Key: 01J44T6P6HF3A6QZQ9S7MCEYQK
Content-Type: application/json

{"installationId":"b7f16835-d9bc-44e4-93e4-cf1053f17dcc","platform":"android","appVersion":"1.0.0"}
```

```json
{
  "accessToken": "<token>",
  "accessTokenExpiresAtUtc": "2026-07-31T18:35:00Z",
  "refreshToken": "<token>",
  "refreshTokenExpiresAtUtc": "2026-08-30T18:20:00Z",
  "user": {"id":"82af3e1d-e00f-46fd-809d-fc9e4d013937","accountType":"guest"},
  "world": null
}
```

Success: `201` for creation or replayed original result.

## 26. World endpoints

```text
POST  /api/v1/worlds
GET   /api/v1/worlds
GET   /api/v1/worlds/current
GET   /api/v1/worlds/{worldId}
PATCH /api/v1/worlds/{worldId}
POST  /api/v1/worlds/{worldId}/resume
GET   /api/v1/worlds/{worldId}/simulation-status
GET   /api/v1/worlds/{worldId}/summaries/latest
```

MVP exposes one current world while preserving collection-compatible contracts. M03 creation accepts only approved player choices such as name; the server transactionally creates the guest/account foundation, world seed/time/state, player Actor/Profile, and settings, but no character Actors or Character records. M05 subsequently initializes the deterministic cast for the existing owned world; character availability after M05 is a gameplay capability, not part of the M03 world-creation transaction. A second logical world creation during MVP returns the existing idempotent result or `409 world_already_exists`; it does not create another playable world. Profile/settings fields use the chosen concurrency strategy. Resume never accepts seed/actions/results/prompts.

**Example 2 — world creation**

```http
POST /api/v1/worlds
Idempotency-Key: 01J44TPMP55AHD0D46XBDKAHME

{"name":"My Parallel World"}
```

```json
{
  "id":"3aa4bd64-68c7-4db3-a3e1-91d095d1877e",
  "name":"My Parallel World",
  "status":"active",
  "currentGameTimeUtc":"2026-07-31T18:20:00Z",
  "player":{"actorId":"d389d482-c160-416c-93b0-dfa59045b7b5","displayName":"Player"},
  "createdAtUtc":"2026-07-31T18:20:00Z"
}
```

Success: `201` with `Location`.

**Example 12 — resume world**

```http
POST /api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/resume
Idempotency-Key: 01J44V5DMVRQZPPJ2XH8W8R7JM
```

```json
{"operationId":"a87ba7dc-1a7d-4db1-a1e1-73a57ee8dbad","status":"pending","statusUrl":"/api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/simulation-status"}
```

Success is `200` if bounded work completes in-request or `202` when durable processing continues; the exact initial mode is open.

**Example 13 — catch-up summary**

```json
{
  "id":"46102ef2-2b6f-4f53-86fe-bd4b7f49eb07",
  "worldId":"3aa4bd64-68c7-4db3-a3e1-91d095d1877e",
  "fromGameTimeUtc":"2026-07-29T18:20:00Z",
  "toGameTimeUtc":"2026-07-31T18:20:00Z",
  "status":"complete",
  "items":[{"type":"newPost","text":"Maya shared an update.","resourceId":"131d1cf2-a5e2-40cd-bdd1-917fc410b16e"}]
}
```

Summary text describes committed structured facts and exposes no hidden simulation action.

## 27. Character endpoints

```text
GET /api/v1/worlds/{worldId}/player-profile
PATCH /api/v1/worlds/{worldId}/player-profile
GET /api/v1/worlds/{worldId}/characters
GET /api/v1/worlds/{worldId}/characters/{characterId}
GET /api/v1/worlds/{worldId}/characters/{characterId}/posts
```

Player profile updates may change approved display name, handle, bio, and settings—not actor/world IDs, followers, influence, reputation, relationships, or mechanics. Character detail may expose public profile, visible mood, interests, profession, a small relationship summary, follow state, and recent posts. Full player-visible relationship detail uses the canonical `/worlds/{worldId}/relationships/{characterId}` route; there is no duplicate character-subresource route. Hidden goals, secrets, private memories, prompts, weights, and concealed scores are excluded.

**Example 3 — character list**

```json
{
  "items":[{"id":"72aad86b-7e46-4aba-8d33-929ee6054a1f","displayName":"Maya Chen","handle":"maya","profession":"designer","visibleMood":"inspired","isFollowed":true}],
  "nextCursor":null,
  "hasMore":false
}
```

## 28. Feed and post endpoints

```text
GET    /api/v1/worlds/{worldId}/feed
POST   /api/v1/worlds/{worldId}/posts
GET    /api/v1/worlds/{worldId}/posts/{postId}
DELETE /api/v1/worlds/{worldId}/posts/{postId}
POST   /api/v1/worlds/{worldId}/posts/{postId}/replies
```

Post editing is open and not in MVP. Delete performs approved logical removal. Author is always the authenticated player; the client cannot post as an AI actor. Feed origin/provider diagnostics remain internal. Feed mode is unavailable until chronological versus deterministic ranking is decided.

**Example 4 — feed page**

```json
{
  "items":[{
    "id":"131d1cf2-a5e2-40cd-bdd1-917fc410b16e",
    "author":{"actorId":"be568379-e153-4de0-953a-e09e3779bdda","displayName":"Maya Chen","handle":"maya"},
    "content":"The studio was unusually quiet today.",
    "createdAtUtc":"2026-07-31T18:10:00Z",
    "parent":null,
    "quote":null,
    "counts":{"likes":3,"replies":1,"reposts":0},
    "currentPlayerReaction":null,
    "visibility":"world"
  }],
  "nextCursor":"eyJvcGFxdWUiOiJ2YWx1ZSJ9",
  "hasMore":true
}
```

**Example 5 — create player post**

```http
POST /api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/posts
Idempotency-Key: 0e044218-cb74-4f35-a8d3-b7946d63b7cb

{"content":"Hello, world.","clientPostId":"0e044218-cb74-4f35-a8d3-b7946d63b7cb"}
```

```json
{"id":"b81ed144-fd41-4fdb-9d5c-92372bb6cff0","worldId":"3aa4bd64-68c7-4db3-a3e1-91d095d1877e","content":"Hello, world.","createdAtUtc":"2026-07-31T18:20:00Z"}
```

Success: `201`; retry returns the original result.

## 29. Reactions, reposts, follows, hashtags, and mentions

```text
PUT    /api/v1/worlds/{worldId}/posts/{postId}/reaction             MVP
DELETE /api/v1/worlds/{worldId}/posts/{postId}/reaction             MVP
PUT    /api/v1/worlds/{worldId}/actors/{actorId}/follow             MVP
DELETE /api/v1/worlds/{worldId}/actors/{actorId}/follow             MVP
PUT    /api/v1/worlds/{worldId}/posts/{postId}/repost               Version 1
DELETE /api/v1/worlds/{worldId}/posts/{postId}/repost               Version 1
POST   /api/v1/worlds/{worldId}/posts/{postId}/quotes               Version 1
GET    /api/v1/worlds/{worldId}/hashtags/{tag}/posts                Version 1
```

MVP reaction type is `like`. Repeated PUT returns current state; repeated DELETE is a successful no-op. Player follow routes target AI actors; AI follow actions are simulation-only. Self/cross-world follow is rejected. Mentions are parsed/validated only when the Version 1 feature is approved; there is no separate MVP mutation endpoint.

**Example 6 — add/remove reaction**

```http
PUT /api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/posts/131d1cf2-a5e2-40cd-bdd1-917fc410b16e/reaction

{"type":"like"}
```

```json
{"postId":"131d1cf2-a5e2-40cd-bdd1-917fc410b16e","type":"like","active":true}
```

`DELETE` on the same route returns `204` whether already absent or removed.

**Example 7 — follow/unfollow**

```http
PUT /api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/actors/be568379-e153-4de0-953a-e09e3779bdda/follow
```

```json
{"actorId":"be568379-e153-4de0-953a-e09e3779bdda","isFollowing":true,"followedAtUtc":"2026-07-31T18:20:00Z"}
```

`DELETE` returns `204` and repeated removal remains successful.

## 30. Messaging endpoints

```text
GET  /api/v1/worlds/{worldId}/conversations
POST /api/v1/worlds/{worldId}/conversations/direct
GET  /api/v1/worlds/{worldId}/conversations/{conversationId}
GET  /api/v1/worlds/{worldId}/conversations/{conversationId}/messages
POST /api/v1/worlds/{worldId}/conversations/{conversationId}/messages
POST /api/v1/worlds/{worldId}/conversations/{conversationId}/read
```

Sender comes from the authenticated player; client-submitted character messages are forbidden. Direct creation returns the existing active conversation on repetition. Messages use `clientMessageId`; duplicate ID returns the prior result. MVP reply eligibility may complete immediately, while delayed/character-initiated messages remain Version 1. Read request identifies the last read message and is idempotent. Editing/deletion/group chat are deferred.

**Example 8 — create direct conversation**

```http
POST /api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/conversations/direct
Idempotency-Key: f0f06f90-f6fa-4114-83d0-dad687f88266

{"characterId":"72aad86b-7e46-4aba-8d33-929ee6054a1f"}
```

```json
{"id":"e6fe4151-d5c3-43c4-8d93-15977233f69d","type":"direct","character":{"id":"72aad86b-7e46-4aba-8d33-929ee6054a1f","displayName":"Maya Chen"},"createdAtUtc":"2026-07-31T18:20:00Z"}
```

Success: `201` for new or `200` for existing current state.

**Example 9 — send private message**

```http
POST /api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/conversations/e6fe4151-d5c3-43c4-8d93-15977233f69d/messages
Idempotency-Key: a0d4ec24-9334-42e0-81fd-695146a13dc4

{"body":"How was your day?","clientMessageId":"a0d4ec24-9334-42e0-81fd-695146a13dc4","replyToMessageId":null}
```

```json
{"message":{"id":"577d1273-e5be-4f41-aab2-0a5fe6d79c93","body":"How was your day?","senderType":"player","createdAtUtc":"2026-07-31T18:20:00Z"},"characterReplyStatus":"pending"}
```

Success: `201`; realtime/polling supplies a later persisted reply if eligible.

## 31. Relationship endpoints

```text
GET /api/v1/worlds/{worldId}/relationships
GET /api/v1/worlds/{worldId}/relationships/{characterId}
GET /api/v1/worlds/{worldId}/relationships/{characterId}/history
GET /api/v1/worlds/{worldId}/relationships/{characterId}/romantic-history
```

All are player-visible projections. The API never accepts relationship deltas/status or exposes complete directional numeric state by default. Exact score visibility remains open.

**Example 10 — relationship summary**

```json
{
  "characterId":"72aad86b-7e46-4aba-8d33-929ee6054a1f",
  "friendshipState":"friend",
  "romanticStatus":"romanticInterest",
  "visibleSignals":{"closeness":"medium","trust":"high"},
  "recentEvents":[{"type":"keptPromise","occurredAtUtc":"2026-07-30T09:00:00Z"}]
}
```

## 32. Dating endpoints

```text
POST /api/v1/worlds/{worldId}/relationships/{characterId}/date-invitations
GET  /api/v1/worlds/{worldId}/date-invitations
GET  /api/v1/worlds/{worldId}/relationships/{characterId}/romantic-history
```

Client submits only the player's permitted choice (for example date type). Server rules decide eligibility, candidate, acceptance/rejection, status, scores, deltas, and history before AI wording. Engagement, marriage, separation, and divorce have no released endpoints.

**Example 11 — date invitation**

```http
POST /api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/relationships/72aad86b-7e46-4aba-8d33-929ee6054a1f/date-invitations
Idempotency-Key: 908ed213-58d1-4aec-ae08-222896999aa4

{"dateType":"coffee"}
```

```json
{"id":"e180626b-3045-4d44-a6ad-abd1e09313af","characterId":"72aad86b-7e46-4aba-8d33-929ee6054a1f","dateType":"coffee","status":"invitationPending","createdAtUtc":"2026-07-31T18:20:00Z"}
```

The response always represents persisted current state. Whether deterministic outcome evaluation finishes in this request or afterward remains an open API/product timing decision.

## 33. Memory endpoints

No public endpoint exposes or mutates raw character memory, recall rankings, secrets, promises, or internal context. A sanitized player-visible projection may be introduced as:

```text
GET /api/v1/worlds/{worldId}/characters/{characterId}/shared-history   Version 1/open
```

Relationship/catch-up history may include safe derived facts, never hidden knowledge or motivations. Internal memory retrieval is an application contract, not public HTTP.

## 34. Simulation and catch-up endpoints

```text
POST /api/v1/worlds/{worldId}/resume
GET  /api/v1/worlds/{worldId}/simulation-status
GET  /api/v1/worlds/{worldId}/summaries/latest
POST /api/v1/dev/worlds/{worldId}/simulate   Development-only
```

Public requests never supply seed, actor action, target, rule outcome, relationship effect, prompt, or result. The development endpoint is authenticated, authorization-checked, environment-gated, absent from production routing/OpenAPI, and cannot bypass deterministic rules.

`GET /worlds/{worldId}/summaries/latest` is the single authoritative MVP return-summary route. It returns the latest persisted catch-up summary and its committed facts, or `404` when none exists. The former `/worlds/{worldId}/summary` concept is not a second resource and is not implemented.

## 35. Trends and world-event endpoints

Version 1:

```text
GET /api/v1/worlds/{worldId}/trends
GET /api/v1/worlds/{worldId}/trends/{trendId}
GET /api/v1/worlds/{worldId}/events
GET /api/v1/worlds/{worldId}/events/{eventId}
```

Full trends/events are not MVP requirements. Character life events appear through safe character/feed/history projections unless a later use case approves a dedicated route.

## 36. Notification endpoints

```text
GET  /api/v1/worlds/{worldId}/notifications
GET  /api/v1/worlds/{worldId}/notifications/unread-count
POST /api/v1/worlds/{worldId}/notifications/{notificationId}/read
POST /api/v1/worlds/{worldId}/notifications/read-all
```

MVP supports an unread badge/deep-link indicators plus a bounded cursor-paginated minimal list for Reply, PrivateMessage, and CatchUpSummary only. The list exists to resolve those indicators and is not rich notification history: no category filtering, search, long-term history promise, or deferred categories. Read operations are idempotent POST actions; rich history/filtering and push are Version 1. Message bodies and sensitive hidden context are excluded.

**Example 14 — notification page**

```json
{
  "items":[{"id":"90dc3d51-ec9d-4f68-8a21-b10a385df488","category":"privateMessage","title":"New message from Maya","resourceId":"e6fe4151-d5c3-43c4-8d93-15977233f69d","isRead":false,"createdAtUtc":"2026-07-31T18:25:00Z"}],
  "nextCursor":null,
  "hasMore":false
}
```

## 37. Account-upgrade and login endpoints

Version 1:

```text
POST /api/v1/auth/register       Shape selected by the accepted M17 auth decision
POST /api/v1/auth/login          Shape selected by the accepted M17 auth decision
POST /api/v1/auth/upgrade        Preserves the existing UserId
POST /api/v1/auth/recovery       Shape selected by the accepted M17 recovery decision
POST /api/v1/devices
PATCH /api/v1/devices/{deviceId}
DELETE /api/v1/devices/{deviceId}
```

Upgrade attaches credentials to the existing guest user and preserves worlds, actors, profiles, posts, relationships, messages, memories, and history. It does not copy data to a new user unless a later accepted architecture decision explicitly changes that rule. Provider-token and verification shapes remain open with authentication method selection.

## 38. Health and operational endpoints

```text
GET /health/live
GET /health/ready
```

Liveness reveals process availability only. Readiness checks required dependencies with minimal sanitized output. Neither returns configuration, secrets, connection strings, provider details, user data, or stack traces. Administrative gameplay endpoints are not part of MVP.

## 39. OpenAPI conventions

- Document every production route, request/response schema, status, ProblemDetails variant, auth rule, pagination parameter, enum, idempotency header, and example.
- Stable operation IDs use module/use-case names; tags are `Auth`, `Worlds`, `Characters`, `Feed`, `Posts`, `Messaging`, `Relationships`, `Dating`, `Simulation`, `Trends`, `Notifications`, and `Operations`.
- Mark deprecated fields/routes and release availability.
- Environment-gated development endpoints are absent from production OpenAPI.
- Generated versus code-first OpenAPI approach remains open; the checked contract must detect unintended breaking changes once implementation begins.

## 40. Deprecation and backward compatibility

Compatible: new endpoint; optional response field; optional request field with unchanged default; enum value only when clients safely handle unknown values.

Breaking: rename/remove; meaning or requiredness change; changed status semantics; incompatible cursor; enum representation change. Deprecations are marked in OpenAPI and release notes, include migration guidance, and remain for a stated practical compatibility period. Removal normally occurs only in a new major version.

## 41. Security and privacy rules

- Never accept `UserId` for authorization or trust actor/world ownership from Flutter.
- Validate all nested IDs against authorized `WorldId`.
- Never return refresh-token hashes, private memories, secrets, hidden goals/scores, prompts, provider metadata, or another user's data.
- Never log tokens, message/post bodies by default, private prompts, secrets, passwords, or provider raw responses.
- Never put private text or tokens in URLs.
- Rate-limit auth, messaging, resume/simulation, and AI-generating paths.
- Sanitize ProblemDetails, realtime, push, OpenAPI examples, and diagnostics.
- Use ownership-safe `404` against cross-user enumeration.
- AI content is untrusted input/output; backend validation and deterministic mechanics remain authoritative.

`SECURITY.md` remains authoritative for token handling, refresh rotation, secrets, logging, and AI-input boundaries.

### Complete error examples

**Example 15 — validation ProblemDetails**

```json
{"type":"https://errors.parallel-world.example/validation","title":"Validation failed","status":400,"detail":"One or more fields are invalid.","instance":"/api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/posts","traceId":"01J44W1YSVVXTGQ10FSV89Q2RK","code":"validation_failed","errors":{"content":["Content must not exceed 500 characters."]}}
```

**Example 16 — conflict ProblemDetails**

```json
{"type":"https://errors.parallel-world.example/idempotency-key-reused","title":"Idempotency key conflict","status":409,"detail":"The key was already used with a different request.","instance":"/api/v1/worlds/3aa4bd64-68c7-4db3-a3e1-91d095d1877e/posts","traceId":"01J44W3HG2D5R3WD7VB5T6N62V","code":"idempotency_key_reused"}
```

**Example 17 — rate-limit ProblemDetails**

```json
{"type":"https://errors.parallel-world.example/rate-limit","title":"Too many requests","status":429,"detail":"Retry after the indicated delay.","instance":"/api/v1/auth/guest","traceId":"01J44W4WAK3N0T7QSAXM5E5F8E","code":"rate_limit_exceeded","retryAfterSeconds":30}
```

Response also includes `Retry-After: 30`.

**Example 18 — cursor-paginated response**

```json
{"items":[{"id":"577d1273-e5be-4f41-aab2-0a5fe6d79c93","createdAtUtc":"2026-07-31T18:20:00Z"}],"nextCursor":"eyJvcGFxdWUiOiJ2YWx1ZSJ9","hasMore":true}
```

**Example 19 — SignalR event payload**

```json
{"contractVersion":1,"eventId":"e09ed43d-17a1-4d1e-a3e4-2428404c43f8","eventType":"message.created","worldId":"3aa4bd64-68c7-4db3-a3e1-91d095d1877e","resourceId":"577d1273-e5be-4f41-aab2-0a5fe6d79c93","occurredAtUtc":"2026-07-31T18:25:00Z"}
```

## 42. Open API decisions

1. Exact access/refresh-token response shape and authentication methods after guest MVP.
2. ETag/`If-Match` versus explicit version fields for editable resources.
3. Final `409` versus `412` concurrency mapping after concurrency transport is selected.
4. **Recorded conflict:** this planning request recommends `400` only for malformed syntax and `422` for valid domain-validation failures, while approved `ARCHITECTURE.md` section 24 specifies `400` with field errors for validation. The API preserves `400`. If `422` is approved later, `docs/architecture/ARCHITECTURE.md` requires correction before this document changes.
5. Cursor encoding/signing, expiry, and compatibility implementation.
6. Exact date-invitation outcome timing while preserving persisted deterministic state.
7. Exact player-visible relationship values versus qualitative signals.
8. Post editing.
9. Message editing/deletion.
10. Final message read-state request/response representation.
11. First SignalR milestone; HTTP refresh/polling remains valid before it.
12. Synchronous versus asynchronous default for world resume; contract supports `200/202` without changing mechanics.
13. API host domain and final ProblemDetails type host.
14. Exact rate-limit values.
15. OpenAPI generation/compatibility-check approach.
16. Idempotency retention period and offline-write retry window.
17. Initial feed ordering and any permitted feed-mode query.

### Consolidated endpoint inventory

`Auth` means bearer authentication; `Idem` means `Idempotency-Key`; `Cursor` means opaque cursor pagination.

| Method | Route | Purpose | Auth | Idem | Cursor | Release | Success |
|---|---|---|---:|---:|---:|---|---|
| POST | `/auth/guest` | Create/resolve guest session | No | Yes | No | MVP | 201 |
| POST | `/auth/refresh` | Rotate session | Refresh | Yes | No | MVP | 200 |
| POST | `/auth/logout` | Revoke session | Yes | Natural | No | MVP | 204 |
| POST | `/worlds` | Create owned world | Yes | Yes | No | MVP | 201 |
| GET | `/worlds` | List owned worlds | Yes | No | No for MVP one-world result | MVP | 200 |
| GET | `/worlds/current` | Get MVP world | Yes | No | No | MVP | 200 |
| GET | `/worlds/{worldId}` | Read world | Yes | No | No | MVP | 200 |
| PATCH | `/worlds/{worldId}` | Update editable world fields | Yes | No; concurrency | No | MVP | 200 |
| POST | `/worlds/{worldId}/resume` | Run bounded catch-up | Yes | Yes | No | MVP | 200/202 |
| GET | `/worlds/{worldId}/simulation-status` | Resume/run state | Yes | No | No | MVP | 200 |
| GET | `/worlds/{worldId}/summaries/latest` | Latest catch-up summary | Yes | No | No | MVP | 200 |
| GET | `/worlds/{worldId}/player-profile` | Read player profile | Yes | No | No | MVP | 200 |
| PATCH | `/worlds/{worldId}/player-profile` | Update editable profile fields | Yes | No; concurrency | No | MVP | 200 |
| GET | `/worlds/{worldId}/characters` | Character catalogue | Yes | No | Yes | MVP | 200 |
| GET | `/worlds/{worldId}/characters/{characterId}` | Character detail | Yes | No | No | MVP | 200 |
| GET | `/worlds/{worldId}/characters/{characterId}/posts` | Character posts | Yes | No | Yes | MVP | 200 |
| GET | `/worlds/{worldId}/feed` | Private feed | Yes | No | Yes | MVP | 200 |
| POST | `/worlds/{worldId}/posts` | Player post | Yes | Yes | No | MVP | 201 |
| GET | `/worlds/{worldId}/posts/{postId}` | Read post | Yes | No | No | MVP | 200 |
| DELETE | `/worlds/{worldId}/posts/{postId}` | Logically delete player post | Yes | Natural | No | MVP | 204 |
| POST | `/worlds/{worldId}/posts/{postId}/replies` | Player reply | Yes | Yes | No | MVP | 201 |
| PUT | `/worlds/{worldId}/posts/{postId}/reaction` | Set like | Yes | Natural | No | MVP | 200 |
| DELETE | `/worlds/{worldId}/posts/{postId}/reaction` | Remove like | Yes | Natural | No | MVP | 204 |
| PUT | `/worlds/{worldId}/actors/{actorId}/follow` | Follow AI actor | Yes | Natural | No | MVP | 200 |
| DELETE | `/worlds/{worldId}/actors/{actorId}/follow` | Unfollow AI actor | Yes | Natural | No | MVP | 204 |
| GET | `/worlds/{worldId}/conversations` | Conversation list | Yes | No | Yes | MVP | 200 |
| POST | `/worlds/{worldId}/conversations/direct` | Resolve direct conversation | Yes | Yes | No | MVP | 200/201 |
| GET | `/worlds/{worldId}/conversations/{conversationId}` | Conversation detail | Yes | No | No | MVP | 200 |
| GET | `/worlds/{worldId}/conversations/{conversationId}/messages` | Message history | Yes | No | Yes | MVP | 200 |
| POST | `/worlds/{worldId}/conversations/{conversationId}/messages` | Player message | Yes | Yes | No | MVP | 201 |
| POST | `/worlds/{worldId}/conversations/{conversationId}/read` | Advance read cursor | Yes | Natural | No | MVP | 204 |
| GET | `/worlds/{worldId}/relationships` | Relationship summaries | Yes | No | Yes | MVP | 200 |
| GET | `/worlds/{worldId}/relationships/{characterId}` | Player-visible relationship | Yes | No | No | MVP | 200 |
| GET | `/worlds/{worldId}/relationships/{characterId}/history` | Relationship history | Yes | No | Yes | MVP | 200 |
| POST | `/worlds/{worldId}/relationships/{characterId}/date-invitations` | Player date choice | Yes | Yes | No | MVP | 201/202 |
| GET | `/worlds/{worldId}/date-invitations` | Invitation state | Yes | No | Yes | MVP | 200 |
| GET | `/worlds/{worldId}/relationships/{characterId}/romantic-history` | Romantic timeline | Yes | No | Yes | MVP | 200 |
| GET | `/worlds/{worldId}/notifications` | Released notification page | Yes | No | Yes | MVP | 200 |
| GET | `/worlds/{worldId}/notifications/unread-count` | Basic indicator | Yes | No | No | MVP | 200 |
| POST | `/worlds/{worldId}/notifications/{notificationId}/read` | Mark read | Yes | Natural | No | MVP | 204 |
| POST | `/worlds/{worldId}/notifications/read-all` | Mark all read | Yes | Natural | No | MVP | 204 |
| PUT | `/worlds/{worldId}/posts/{postId}/repost` | Set repost | Yes | Natural | No | Version 1 | 200 |
| DELETE | `/worlds/{worldId}/posts/{postId}/repost` | Remove repost | Yes | Natural | No | Version 1 | 204 |
| POST | `/worlds/{worldId}/posts/{postId}/quotes` | Quote post | Yes | Yes | No | Version 1 | 201 |
| GET | `/worlds/{worldId}/hashtags/{tag}/posts` | Hashtag feed | Yes | No | Yes | Version 1 | 200 |
| GET | `/worlds/{worldId}/trends` | Trend list | Yes | No | Yes | Version 1 | 200 |
| GET | `/worlds/{worldId}/trends/{trendId}` | Trend detail | Yes | No | No | Version 1 | 200 |
| GET | `/worlds/{worldId}/events` | World-event list | Yes | No | Yes | Version 1 | 200 |
| GET | `/worlds/{worldId}/events/{eventId}` | World-event detail | Yes | No | No | Version 1 | 200 |
| GET | `/worlds/{worldId}/characters/{characterId}/shared-history` | Sanitized shared history | Yes | No | Yes | Version 1/open | 200 |
| POST | `/auth/register` | Create registered account | No | Yes | No | Version 1 | 201 |
| POST | `/auth/login` | Create registered session | No | Yes | No | Version 1 | 200 |
| POST | `/auth/upgrade` | Upgrade current guest | Yes | Yes | No | Version 1 | 200 |
| POST | `/auth/recovery` | Selected registered-account recovery flow | Mixed | Yes | No | Version 1/gated | 200/204 |
| POST | `/devices` | Register push installation | Yes | Yes | No | Version 1 | 201 |
| PATCH | `/devices/{deviceId}` | Update owned installation | Yes | No; concurrency | No | Version 1 | 200 |
| DELETE | `/devices/{deviceId}` | Revoke owned installation | Yes | Natural | No | Version 1 | 204 |
| POST | `/dev/worlds/{worldId}/simulate` | Controlled manual simulation | Dev auth | Yes | No | Development-only | 202 |
| GET | `/health/live` | Liveness probe | Deployment policy | No | No | MVP operations | 200 |
| GET | `/health/ready` | Readiness probe | Deployment policy | No | No | MVP operations | 200/503 |

### Final consistency checks

Before implementation or API acceptance verify:

1. Every MVP use case has a release-labelled endpoint.
2. Every world route authorizes owner and validates all nested IDs as same-world.
3. No client route controls AI characters, seeds, actions, scores, memories, or outcomes.
4. No request accepts numerical relationship changes or romantic result/status.
5. No response exposes internal memory, secret, prompt, provider, or hidden mechanical data.
6. Guest, world, post, reply, message, dating, and resume retries are idempotent.
7. Feed, characters, messages, notifications, and relationship histories use opaque cursor pagination.
8. Status codes, ProblemDetails, and stable codes agree throughout the document.
9. Realtime events are post-commit hints and HTTP remains authoritative.
10. Flutter can cache, retry, deduplicate, and safely handle unknown enums/fields.
11. Version 1/Future/Development-only contracts do not become MVP implementation requirements.
12. Routes, examples, status codes, and endpoint inventory remain synchronized.
