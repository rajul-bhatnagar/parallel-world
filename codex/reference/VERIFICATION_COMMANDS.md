# Verification Commands

Run commands from the repository root and adapt paths only when the project layout requires it. Run every check relevant to the changed area. Record the exact command and whether it passed, failed, or could not run; never treat a skipped check as a pass.

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

- Apply migrations to a clean PostgreSQL database.
- Start API.
- Run smoke tests.
- Verify downgrade/rollback strategy is documented even when automatic downgrade is not supported.
