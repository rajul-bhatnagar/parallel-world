# Naming Conventions

## Repository

- Solution/project prefix: `ParallelWorld`
- Projects: `ParallelWorld.Api`, `ParallelWorld.Application`, `ParallelWorld.Domain`, `ParallelWorld.Infrastructure`, `ParallelWorld.Simulation`, `ParallelWorld.AI`
- C# namespaces: `ParallelWorld.<Project>` and `ParallelWorld.<Project>.<Area>`
- Flutter package: `parallel_world_app`
- PostgreSQL identifiers: unquoted `snake_case`; C# entities/properties: PascalCase. EF Core conventions or explicit mappings translate between them.

## Branches

- Required milestone branch pattern: `feature/mNN-short-description`
- Required M01 branch: `feature/m01-repository-bootstrap`
- `feature/private-messaging`
- `fix/relationship-idempotency`
- `chore/update-dependencies`

## Commits

Use a conventional `<type>(<scope>): <description>` form. Keep commits self-contained and limited to one milestone or task.

- `feat(worlds): add guest world creation`
- `feat(messages): add cursor pagination`
- `fix(simulation): prevent duplicate interval execution`
- `test(relationships): cover dating transitions`
- `docs(architecture): record actor model decision`

## API

- Resources are nouns and plural.
- Commands use subresources/actions only when CRUD semantics are insufficient.
- Stable machine-readable error codes use lowercase `snake_case`, as required by API_CONVENTIONS.md.

## Tests

- `Method_State_ExpectedResult`
- Simulation fixtures include rule version and seed in the test name/data.
