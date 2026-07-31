# AGENTS.md

## Project

Parallel World is a private, single-player AI social-life simulation game.

Each real account owns one or more isolated private worlds. Each world contains one human-controlled player and AI-controlled characters. Real users never interact with one another.

## Governance and sources of truth

- `AGENTS.md` defines repository-wide engineering and working rules.
- `docs/product/PRODUCT.md` defines product scope and MVP boundaries.
- `docs/game-design/GAME_RULES.md` defines simulation behaviour.
- `docs/architecture/ARCHITECTURE.md`, `docs/architecture/DATABASE.md`, `docs/architecture/API_CONVENTIONS.md`, and `docs/architecture/SECURITY.md` define technical constraints.
- `docs/development/TEST_STRATEGY.md` defines required verification.
- `docs/development/DECISIONS.md` records accepted decisions and superseding decisions.
- `docs/milestones/` defines milestone scope and acceptance criteria.
- Files under `codex/` are working prompts and references. They do not override accepted source-of-truth documents.

Before changing code, read this file, the relevant source-of-truth documents, and the current milestone. If requirements conflict or an authoritative rule is unclear, stop and report the conflict instead of choosing a new product rule or architecture silently.

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

1. Simulation rules decide actions, state transitions, and outcomes.
2. AI generates natural-language wording only. AI output must not directly choose or change mechanical outcomes.
3. PostgreSQL is the authoritative source of truth.
4. Drift and other Flutter local storage are cache and offline-read layers only; they are never authoritative.
5. Every world-owned persistent record must carry an explicit `WorldId` ownership boundary. Every game entity belongs to a `GameWorld` unless explicitly account-level.
6. Every `GameWorld` belongs to exactly one real user.
7. Real users cannot access or affect other users' worlds.
8. Use a modular monolith; do not introduce microservices initially.
9. Do not introduce Redis, Kafka, Service Bus, or Kubernetes without an accepted architecture decision.
10. All persistent timestamps use UTC.
11. Feed and message history use cursor pagination.
12. All simulation randomness must be reproducible with a deterministic seed.
13. Schema changes require EF Core migrations.
14. Behavioural game rules require automated tests.
15. Secrets must never be committed, embedded in Flutter, returned to clients, or written to logs. Logs must not expose credentials, tokens, private message content, or unnecessary personal data.
16. Do not silently change accepted product rules or architecture.
17. Prefer simple explicit code over speculative abstractions.
18. Avoid polymorphic foreign keys where practical.
19. Simulation processing must be idempotent.
20. Ownership checks must be enforced server-side.

## Required Codex behaviour

Before editing:

- Read `AGENTS.md`, relevant source-of-truth documents, and the current milestone.
- Inspect the existing code, repository structure, `git status`, and relevant diffs before modifying files.
- State a short plan.
- State assumptions and risks.
- Identify conflicting or ambiguous requirements and resolve them before implementation.

During implementation:

- Implement only the requested task.
- Do not implement future milestones, speculative foundations for them, or unrelated cleanup.
- Preserve unrelated user changes in the working tree.
- Preserve existing public behaviour unless the task changes it.
- Follow existing naming and architecture.
- Add or update automated tests.
- Explain any new package.
- Update relevant documentation when behaviour, contracts, setup, or operations change.
- Add a new entry to `docs/development/DECISIONS.md` when an accepted architecture or behavioural decision changes; do not rewrite accepted history.

After implementation:

- Run applicable formatting checks, builds, and relevant tests.
- Report the exact verification commands and whether each passed, failed, or could not run.
- If verification fails or cannot run, report the failure and reason honestly; do not describe the task as fully verified.
- List changed files.
- List important decisions.
- State remaining risks.
- Suggest a commit message.
- Confirm that no future-milestone work was included.
