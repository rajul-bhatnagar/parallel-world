# API Conventions

## Base conventions

- Base path: `/api/v1`.
- JSON uses camelCase.
- Timestamps use ISO-8601 UTC.
- Errors use RFC-compatible Problem Details.
- Validation errors include stable field/error codes.
- Never expose internal exception details in production.

## Ownership

Every world-scoped endpoint must resolve the current authenticated user and verify that the requested world belongs to that user. A guessed world ID must not leak existence or data.

## Pagination

Feeds and messages use opaque cursor pagination.

Example response:

```json
{
  "items": [],
  "nextCursor": "opaque-value",
  "hasMore": true
}
```

Cursor encoding must include ordering fields and be integrity-protected or treated as untrusted input.

## Idempotency

Create actions susceptible to network retry accept an idempotency key. Repeating the same key with the same operation returns the existing result; conflicting reuse is rejected.

## Endpoint groups

- `/api/v1/auth`
- `/api/v1/worlds`
- `/api/v1/characters`
- `/api/v1/feed`
- `/api/v1/posts`
- `/api/v1/conversations`
- `/api/v1/messages`
- `/api/v1/relationships`
- `/api/v1/events`
- `/api/v1/trends`
- `/api/v1/notifications`
- `/api/v1/simulation`

## Versioning

Breaking API changes require a new version or a coordinated client migration. Internal refactors do not.

## DTO rules

- Do not return EF entities directly.
- Request and response DTOs are explicit.
- Numeric game mechanics not intended for the player may be summarized rather than exposed raw.
- Client-provided ownership IDs are never trusted without server validation.
