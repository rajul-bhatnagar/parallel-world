import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_dependencies.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

import '../../support/fakes.dart';

ProviderContainer containerFor({
  required FakeSessionLifecycle sessions,
  required FakeWorldGateway worlds,
  required FakeWorldCache cache,
}) => ProviderContainer(
  overrides: [
    sessionLifecycleProvider.overrideWithValue(sessions),
    worldGatewayProvider.overrideWithValue(worlds),
    worldCacheProvider.overrideWithValue(cache),
    utcNowProvider.overrideWithValue(() => testNow),
  ],
);

void main() {
  test('first launch bootstraps the guest and caches its world', () async {
    final sessions = FakeSessionLifecycle();
    final worlds = FakeWorldGateway();
    final cache = FakeWorldCache();
    final container = containerFor(
      sessions: sessions,
      worlds: worlds,
      cache: cache,
    );
    addTearDown(container.dispose);

    await container.read(sessionControllerProvider.notifier).initialize();

    expect(
      container.read(sessionControllerProvider).phase,
      SessionPhase.authenticated,
    );
    expect(cache.value?.id, testWorld.id);
    expect(cache.initialized, isTrue);
    expect(sessions.installationCalls, 1);
  });

  test('returning session refreshes then resolves current world', () async {
    final sessions = FakeSessionLifecycle()
      ..restored = testCredentials(accessExpired: true);
    final worlds = FakeWorldGateway();
    final cache = FakeWorldCache();
    final container = containerFor(
      sessions: sessions,
      worlds: worlds,
      cache: cache,
    );
    addTearDown(container.dispose);

    await container.read(sessionControllerProvider.notifier).initialize();

    expect(sessions.refreshCalls, 1);
    expect(worlds.currentCalls, 1);
    expect(
      container.read(sessionControllerProvider).phase,
      SessionPhase.authenticated,
    );
  });

  test('network failure uses only the matching user cached world', () async {
    final sessions = FakeSessionLifecycle()..restored = testCredentials();
    final worlds = FakeWorldGateway()..currentError = const NetworkFailure();
    final cache = FakeWorldCache()..value = testWorld;
    final container = containerFor(
      sessions: sessions,
      worlds: worlds,
      cache: cache,
    );
    addTearDown(container.dispose);

    await container.read(sessionControllerProvider.notifier).initialize();

    final state = container.read(sessionControllerProvider);
    expect(state.phase, SessionPhase.offlineAuthenticated);
    expect(state.world?.id, testWorld.id);
  });

  test('missing server world reaches the creation route state', () async {
    final sessions = FakeSessionLifecycle()..restored = testCredentials();
    final worlds = FakeWorldGateway()..currentError = const NotFoundFailure();
    final cache = FakeWorldCache();
    final container = containerFor(
      sessions: sessions,
      worlds: worlds,
      cache: cache,
    );
    addTearDown(container.dispose);

    await container.read(sessionControllerProvider.notifier).initialize();

    expect(
      container.read(sessionControllerProvider).phase,
      SessionPhase.missingWorld,
    );
  });

  test(
    'expired refresh credentials are cleared before session recovery',
    () async {
      final sessions = FakeSessionLifecycle()
        ..restored = testCredentials(accessExpired: true, refreshExpired: true);
      final worlds = FakeWorldGateway();
      final cache = FakeWorldCache()..value = testWorld;
      final container = containerFor(
        sessions: sessions,
        worlds: worlds,
        cache: cache,
      );
      addTearDown(container.dispose);

      await container.read(sessionControllerProvider.notifier).initialize();

      expect(sessions.clearCalls, 1);
      expect(
        container.read(sessionControllerProvider).phase,
        SessionPhase.sessionExpired,
      );
      expect(cache.value, testWorld);
    },
  );

  test('failed logout retains cached private data', () async {
    final sessions = FakeSessionLifecycle()
      ..logoutError = const NetworkFailure();
    final worlds = FakeWorldGateway();
    final cache = FakeWorldCache()..value = testWorld;
    final container = containerFor(
      sessions: sessions,
      worlds: worlds,
      cache: cache,
    );
    addTearDown(container.dispose);
    await container.read(sessionControllerProvider.notifier).initialize();

    await container.read(sessionControllerProvider.notifier).logout();

    expect(cache.clearCalls, 0);
    expect(cache.value, isNotNull);
  });

  test(
    'unrecoverable local logout state offers explicit new-session recovery',
    () async {
      final sessions = FakeSessionLifecycle()
        ..logoutError = const SessionRecoveryFailure('Start a new session.');
      final worlds = FakeWorldGateway();
      final cache = FakeWorldCache()..value = testWorld;
      final container = containerFor(
        sessions: sessions,
        worlds: worlds,
        cache: cache,
      );
      addTearDown(container.dispose);
      await container.read(sessionControllerProvider.notifier).initialize();

      await container.read(sessionControllerProvider.notifier).logout();

      expect(
        container.read(sessionControllerProvider).phase,
        SessionPhase.sessionExpired,
      );
      expect(cache.clearCalls, 0);
      expect(cache.value, isNotNull);
    },
  );

  test('bootstrap exhaustion reaches explicit new-session recovery', () async {
    final sessions = FakeSessionLifecycle()
      ..guestError = const SessionRecoveryFailure('Recovery exhausted.');
    final worlds = FakeWorldGateway();
    final cache = FakeWorldCache();
    final container = containerFor(
      sessions: sessions,
      worlds: worlds,
      cache: cache,
    );
    addTearDown(container.dispose);

    await container.read(sessionControllerProvider.notifier).initialize();

    expect(
      container.read(sessionControllerProvider).phase,
      SessionPhase.bootstrapRecoveryExhausted,
    );
    expect(sessions.guestCalls, 1);

    sessions.guestError = null;
    await container
        .read(sessionControllerProvider.notifier)
        .startNewGuestSession();

    expect(sessions.clearCalls, 1);
    expect(sessions.guestCalls, 2);
    expect(
      container.read(sessionControllerProvider).phase,
      SessionPhase.authenticated,
    );
  });

  test(
    'successful logout clears cached private data and expires the route',
    () async {
      final sessions = FakeSessionLifecycle();
      final worlds = FakeWorldGateway();
      final cache = FakeWorldCache();
      final container = containerFor(
        sessions: sessions,
        worlds: worlds,
        cache: cache,
      );
      addTearDown(container.dispose);
      await container.read(sessionControllerProvider.notifier).initialize();

      await container.read(sessionControllerProvider.notifier).logout();

      expect(cache.clearCalls, 1);
      expect(cache.value, isNull);
      expect(
        container.read(sessionControllerProvider).phase,
        SessionPhase.sessionExpired,
      );
    },
  );

  test(
    'world creation reuses its idempotency key after a safe API failure',
    () async {
      final sessions = FakeSessionLifecycle()..restored = testCredentials();
      final worlds = FakeWorldGateway();
      final cache = FakeWorldCache();
      final container = containerFor(
        sessions: sessions,
        worlds: worlds,
        cache: cache,
      );
      addTearDown(container.dispose);
      await container.read(sessionControllerProvider.notifier).initialize();
      worlds.createError = const NetworkFailure();

      await container
          .read(sessionControllerProvider.notifier)
          .createWorld('New world');
      final firstKey = worlds.lastIdempotencyKey;
      worlds.createError = null;
      await container
          .read(sessionControllerProvider.notifier)
          .createWorld('New world');

      expect(firstKey, isNotNull);
      expect(worlds.lastIdempotencyKey, firstKey);
      expect(worlds.createCalls, 2);
    },
  );
}
