# Working Method with Codex

## One thread per responsibility

Use separate Codex threads for:

- Product and architecture planning
- Backend implementation
- Flutter implementation
- Simulation and game-rule implementation
- Independent code review

Do not let two agents modify the same contracts, migrations, or shared models at the same time.

## One branch per milestone

Recommended branch style:

```text
milestone/01-repository-bootstrap
milestone/02-backend-foundation
feature/character-catalogue
feature/private-messaging
fix/message-pagination
```

## One vertical slice at a time

A vertical slice should include, where relevant:

- Domain model
- Persistence mapping and migration
- Application behaviour
- API endpoint
- Flutter repository/state/UI
- Automated tests
- Documentation update

## Definition of a safe Codex task

A task is small enough when:

- It has one measurable user-visible or engineering result.
- Its acceptance criteria fit on one page.
- It can be tested independently.
- It can be committed without unfinished future features.
- It does not require Codex to redesign multiple modules silently.

## Never accept these behaviours

- Codex claims tests passed without showing commands or results.
- AI output directly changes trust, dating state, reputation, or other mechanics.
- World-specific rows are created without `WorldId` ownership.
- Mobile application secrets contain server API keys.
- Database schema changes have no migration.
- Random simulation behaviour cannot be reproduced with a seed.
- A free-hosting restart can duplicate simulation runs.
