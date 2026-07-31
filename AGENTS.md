# AGENTS.md

## Project

Parallel World is a private, single-player AI social-life simulation game.

Each real account owns one or more isolated private worlds. Each world contains one human-controlled player and AI-controlled characters. Real users never interact with one another.

## Technology

- Flutter mobile application
- Dart
- Riverpod
- GoRouter
- Dio
- Drift for local cache
- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- Modular monolith
- SignalR where useful
- BackgroundService plus catch-up simulation
- Provider-independent AI integration
- xUnit backend tests
- Flutter unit and widget tests
- Docker for backend deployment

## Non-negotiable architecture rules

1. Simulation rules decide what happens.
2. AI generates wording only.
3. PostgreSQL is the authoritative source of truth.
4. Flutter local storage is a cache and offline-read layer.
5. Every game entity belongs to a `GameWorld` unless explicitly account-level.
6. Every `GameWorld` belongs to exactly one real user.
7. Real users cannot access or affect other users' worlds.
8. Use a modular monolith; do not introduce microservices initially.
9. Do not introduce Redis, Kafka, Service Bus, or Kubernetes without an accepted architecture decision.
10. All persistent timestamps use UTC.
11. Feed and message history use cursor pagination.
12. All simulation randomness must be reproducible with a deterministic seed.
13. Schema changes require EF Core migrations.
14. Behavioural game rules require automated tests.
15. Secrets must never be committed or embedded in Flutter.
16. Do not silently change accepted product rules or architecture.
17. Prefer simple explicit code over speculative abstractions.
18. Avoid polymorphic foreign keys where practical.
19. Simulation processing must be idempotent.
20. Ownership checks must be enforced server-side.

## Required Codex behaviour

Before editing:

- Read relevant source-of-truth documents.
- Inspect the existing repository.
- State a short plan.
- State assumptions and risks.

During implementation:

- Implement only the requested task.
- Do not implement future milestones.
- Preserve existing public behaviour unless the task changes it.
- Follow existing naming and architecture.
- Add or update automated tests.
- Explain any new package.
- Update documentation when behaviour changes.

After implementation:

- Run formatting.
- Run build and relevant tests.
- Report commands and results.
- List changed files.
- List important decisions.
- State remaining risks.
- Suggest a commit message.
- Never claim success when verification did not run.
