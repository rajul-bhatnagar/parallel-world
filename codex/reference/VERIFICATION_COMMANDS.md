# Verification Commands

Run commands from the repository root and adapt paths only when the project layout requires it. Run every check relevant to the changed area. Record each as **Passed**, **Failed**, **Unavailable**, or **Not applicable — with reason**. Do not invent a path or require a tool, project, migration, database, or UI surface that the current milestone has not created.

Run milestone verification before creating the milestone-specific commit and pushing `dev`. At an approved stable checkpoint, applicable CI must pass on the `dev`-to-`main` pull request before promotion; a pull request is not required after every milestone.

## Backend

```bash
dotnet format --verify-no-changes
dotnet build --configuration Release
dotnet test --configuration Release
```

## Flutter

```bash
flutter pub get
dart format --output=none --set-exit-if-changed .
flutter analyze
flutter test
```

## Docker/local infrastructure

```bash
docker compose config
docker compose up -d postgres
```

## Migration sanity

- Applies only after a milestone creates a schema/migration and PostgreSQL-dependent tests.
- For M01, PostgreSQL migrations, PostgreSQL integration tests, and schema verification are **Not applicable — M01 creates no database schema**.
- Apply migrations to a clean PostgreSQL database.
- Start API.
- Run smoke tests.
- Verify downgrade/rollback strategy is documented even when automatic downgrade is not supported.
