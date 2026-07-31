# Verification Commands

Adapt paths to the repository.

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
