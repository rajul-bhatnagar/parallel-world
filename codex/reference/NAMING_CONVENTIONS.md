# Naming Conventions

## Repository

- Solution/project prefix: `Parallel`
- C# namespaces: `Parallel.<Project>.<Area>`
- Flutter package: `parallel_app`
- PostgreSQL tables: use one consistent EF-generated snake_case or PascalCase policy. Until the policy is accepted in `docs/development/DECISIONS.md`, do not guess or mix conventions; follow the existing schema and mappings.

## Branches

- `milestone/01-repository-bootstrap`
- `feature/private-messaging`
- `fix/relationship-idempotency`
- `chore/update-dependencies`

## Commits

Use a conventional `<type>(<scope>): <description>` form. Keep commits self-contained and limited to one milestone or task.

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
