# Test Strategy

## Backend unit tests

- Character activity decisions
- Feed scoring
- Trend scoring
- Relationship score changes and clamping
- Idempotent relationship events
- Dating eligibility and state transitions
- Breakup pressure
- Memory recall ranking
- Catch-up volume calculations
- AI output validation and fallback

## Backend integration tests

- Guest-session creation and reuse
- Refresh-token rotation
- World ownership isolation
- EF mappings and migrations
- Feed cursor ordering
- Message cursor ordering
- Idempotent post/message creation
- Simulation run persistence and duplicate prevention
- Transactional relationship updates
- AI provider stub integration

Use a real PostgreSQL test container where practical rather than replacing relational behaviour with an in-memory provider.

## Architecture tests

- Domain has no infrastructure dependencies.
- API does not access DbContext directly if application boundary prohibits it.
- Modules do not introduce forbidden references.

## Flutter tests

- Repository mapping and error handling
- Riverpod state transitions
- Session bootstrap
- Feed loading, pagination, refresh, empty, offline, and failure states
- Post composition
- Conversation loading and sending
- Relationship timeline rendering
- Secure-token/session behaviour through abstractions

## Deterministic simulation fixtures

Every simulation test declares:

- initial state
- rule version
- seed
- expected decisions
- expected persisted effects

## Test honesty

Codex must report exactly which tests ran. Skipped integration tests must be explicitly stated with reason.
