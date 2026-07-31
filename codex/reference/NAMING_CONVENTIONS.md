# Naming Conventions

## Repository

- Solution/project prefix: `Parallel`
- C# namespaces: `Parallel.<Project>.<Area>`
- Flutter package: `parallel_app`
- PostgreSQL tables: use consistent EF-generated snake_case or PascalCase policy; choose once and document.

## Branches

- `milestone/01-repository-bootstrap`
- `feature/private-messaging`
- `fix/relationship-idempotency`
- `chore/update-dependencies`

## Commits

- `feat(worlds): add guest world creation`
- `feat(messages): add cursor pagination`
- `fix(simulation): prevent duplicate interval execution`
- `test(relationships): cover breakup transitions`
- `docs(architecture): record actor model decision`

## API

- Resources are nouns and plural.
- Commands use subresources/actions only when CRUD semantics are insufficient.
- Stable machine-readable error codes use uppercase snake case.

## Tests

- `Method_State_ExpectedResult`
- Simulation fixtures include rule version and seed in the test name/data.
