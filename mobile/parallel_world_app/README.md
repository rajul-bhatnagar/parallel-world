# Parallel World mobile application

This Android-first Flutter application is pinned to Flutter 3.47.1 with bundled Dart 3.13.1. M04 supplies the production-shaped client foundation for the M03 guest-session and private-world API. Character, feed, relationship, messaging, simulation, and AI features remain deferred.

## Configuration

`API_BASE_URL` is required at compile time. Release builds also require an explicit `APP_ENVIRONMENT=production` and reject non-HTTPS URLs. Debug builds default an omitted environment to `local`. The Android emulator reaches an API running on the development host through `10.0.2.2`; the debug Android manifest permits cleartext traffic only to that emulator host:

```bash
flutter run \
  --dart-define=API_BASE_URL=http://10.0.2.2:8080 \
  --dart-define=APP_ENVIRONMENT=local
```

Optional defines are `APP_VERSION`, `INITIAL_WORLD_NAME`, `API_CONNECT_TIMEOUT_MS`, `API_SEND_TIMEOUT_MS`, and `API_RECEIVE_TIMEOUT_MS`. Each timeout must be between 100 and 60,000 milliseconds. API URLs containing embedded user information are rejected.

## Client boundaries

- Riverpod owns dependency wiring and session state.
- GoRouter permits only the M04 splash, retry, world-creation, and home destinations.
- Dio uses separate public and authenticated clients. Refresh is single-flight and never automatically replays an ambiguously consumed refresh token.
- `flutter_secure_storage` contains installation, bootstrap-recovery, and session secrets. Installation ID is metadata, never authentication.
- Drift contains only the user-scoped cached world projection. Cached data is presented as offline data and never treated as authorization.
- Network logging records sanitized request metadata only, never headers, bodies, tokens, bootstrap proofs, or installation IDs.

Android requests that backup be disabled and additionally excludes the three `flutter_secure_storage` preference files from both legacy backup and Android 12+ cloud/device-transfer extraction rules. Other non-sensitive app data is not broadly excluded by those rules.

## Generate and verify

From this directory:

```bash
flutter pub get
dart run build_runner build
dart format --output=none --set-exit-if-changed .
flutter analyze
flutter test
flutter build apk --debug \
  --dart-define=API_BASE_URL=http://10.0.2.2:8080 \
  --dart-define=APP_ENVIRONMENT=test
flutter build apk --release \
  --dart-define=API_BASE_URL=https://api.example.test \
  --dart-define=APP_ENVIRONMENT=production
```
