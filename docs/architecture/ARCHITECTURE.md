# Architecture Source of Truth

## System shape

```text
Flutter mobile app
        |
        | HTTPS / SignalR
        v
ASP.NET Core modular monolith
        |
        +-- PostgreSQL
        +-- AI provider API
        +-- optional object storage
        +-- optional push provider
```

## Backend projects

- `Parallel.Api`: HTTP, authentication middleware, OpenAPI, composition root.
- `Parallel.Application`: use cases, commands/queries, validation, DTOs.
- `Parallel.Domain`: entities, value objects, domain rules, events.
- `Parallel.Infrastructure`: EF Core, providers, persistence, external integrations.
- `Parallel.Simulation`: deterministic simulation and game rule execution.
- `Parallel.AI`: provider-neutral generation contracts and prompt construction.
- `Parallel.Tests`: unit, integration, and architecture tests; may later split by type.

Dependency direction:

```text
Api -> Application
Infrastructure -> Application + Domain
Simulation -> Domain
AI -> Application/Domain contracts as explicitly approved
Domain -> nothing external
```

## Modular-monolith modules

- Accounts
- Worlds
- Characters
- Social
- Messaging
- Relationships
- Memories
- Simulation
- EventsAndTrends
- Notifications
- AI

Modules may share the same PostgreSQL database initially but must respect ownership and application boundaries.

## Flutter structure

```text
lib/
  app/
  core/
    api/
    auth/
    cache/
    errors/
    observability/
    widgets/
  features/
    session/
    world/
    characters/
    feed/
    posts/
    messaging/
    relationships/
    memories/
    notifications/
    settings/
```

## Data authority

- PostgreSQL is authoritative.
- Drift caches server data and holds drafts/pending actions.
- Local cache conflicts never overwrite newer authoritative server state without a defined sync rule.

## Authentication evolution

Version 1 uses an automatic guest session:

1. App creates a random installation identifier.
2. Backend creates guest account/session.
3. Backend issues access and refresh tokens.
4. Tokens are stored securely.
5. Later registration upgrades the same account and preserves worlds.

No server API secret is embedded in Flutter.

## Simulation architecture

Decision creation and execution are separate.

```text
Simulation scheduler/catch-up request
  -> acquire world interval lock/idempotency key
  -> load state
  -> deterministic decision creation
  -> persist actions
  -> execute actions
  -> queue/generate wording
  -> persist content and side effects
  -> complete run
```

## AI architecture

`IAiTextGenerator` accepts an already-decided action. It receives only the minimum required personality, mood, relationship context, relevant memories, topic, stance, tone, and length limits.

AI output is validated, bounded, and replaceable with template fallback.

## Background processing

Initial deployment may sleep. Therefore:

- Use `BackgroundService` while running.
- Use catch-up simulation when the player returns.
- Never rely solely on uninterrupted background execution.
- Ensure duplicate processing is prevented.

## Rejected initially

- Microservices
- Event broker
- Redis
- Kubernetes
- Dedicated search engine
- Separate database per user
- Direct mobile access to database
- AI-controlled numerical mechanics
