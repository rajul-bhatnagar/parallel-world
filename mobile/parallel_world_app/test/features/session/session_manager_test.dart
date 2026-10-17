import 'dart:async';

import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/auth/secret_generator.dart';
import 'package:parallel_world_app/core/auth/secure_session_store.dart';
import 'package:parallel_world_app/core/config/app_config.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/session/data/session_manager.dart';

import '../../support/fakes.dart';

void main() {
  late MemorySecureKeyValueStore storage;
  late SecureSessionStore secureStore;
  late FakeAuthGateway auth;
  late SessionManager manager;

  setUp(() {
    storage = MemorySecureKeyValueStore();
    secureStore = SecureSessionStore(storage, SecretGenerator());
    auth = FakeAuthGateway();
    manager = SessionManager(
      sessionStore: secureStore,
      authApi: auth,
      config: AppConfig.fromValues(apiBaseUrl: 'https://api.example.test'),
      utcNow: () => testNow,
    );
  });

  test(
    'guest bootstrap stores credentials before discarding the proof',
    () async {
      final result = await manager.bootstrapGuest();

      expect(result.userId, testGuestResponse.userId);
      expect(
        (await manager.restore())?.refreshToken,
        'replacement-refresh-token',
      );
      final nextProof = await secureStore.loadOrCreateBootstrapProof();
      expect(nextProof.attempts, 0);
      expect(nextProof.value, isNot(auth.lastProof));
    },
  );

  test('concurrent refresh callers share one non-idempotent request', () async {
    await secureStore.writeCredentials(testCredentials(accessExpired: true));
    auth.refreshCompleter = Completer();

    final first = manager.refreshSingleFlight();
    final second = manager.refreshSingleFlight();
    await Future<void>.delayed(Duration.zero);
    expect(auth.refreshCalls, 1);

    auth.refreshCompleter!.complete(testTokenPair);
    expect(
      await Future.wait([first, second]),
      everyElement('replacement-access-token'),
    );
    expect(auth.refreshCalls, 1);
  });

  test(
    'ambiguous refresh failure marks the token consumed and prevents replay',
    () async {
      await secureStore.writeCredentials(testCredentials(accessExpired: true));
      auth.refreshError = const NetworkFailure();

      await expectLater(
        manager.refreshSingleFlight(),
        throwsA(isA<NetworkFailure>()),
      );
      expect((await manager.restore())?.refreshMayBeConsumed, isTrue);
      await expectLater(
        manager.refreshSingleFlight(),
        throwsA(isA<AuthenticationFailure>()),
      );
      expect(auth.refreshCalls, 1);
    },
  );

  test('successful logout removes session secrets but retains installation metadata', () async {
    final installationId = await secureStore.loadOrCreateInstallationId();
    await secureStore.writeCredentials(testCredentials());

    await manager.logout();

    expect(await manager.restore(), isNull);
    expect(await secureStore.loadOrCreateInstallationId(), installationId);
    expect(auth.logoutCalls, 1);
  });

  test('expired access refreshes before revoking the current family', () async {
    await secureStore.writeCredentials(testCredentials(accessExpired: true));

    await manager.logout();

    expect(auth.refreshCalls, 1);
    expect(auth.logoutCalls, 1);
    expect(auth.lastLogoutAccessToken, 'replacement-access-token');
    expect(auth.lastLogoutRefreshToken, 'replacement-refresh-token');
    expect(await manager.restore(), isNull);
  });

  test(
    'a rejected refresh proves the family unusable and clears locally',
    () async {
      await secureStore.writeCredentials(testCredentials(accessExpired: true));
      auth.refreshError = const AuthenticationFailure(
        code: 'refresh_token_replayed',
      );

      await manager.logout();

      expect(auth.refreshCalls, 1);
      expect(auth.logoutCalls, 0);
      expect(await manager.restore(), isNull);
    },
  );

  test('a refresh network failure does not claim logout succeeded', () async {
    await secureStore.writeCredentials(testCredentials(accessExpired: true));
    auth.refreshError = const NetworkFailure();

    await expectLater(manager.logout(), throwsA(isA<NetworkFailure>()));

    expect(auth.logoutCalls, 0);
    expect((await manager.restore())?.refreshMayBeConsumed, isTrue);
  });

  test(
    'a logout network failure retains the recoverable local session',
    () async {
      await secureStore.writeCredentials(testCredentials());
      auth.logoutError = const NetworkFailure();

      await expectLater(manager.logout(), throwsA(isA<NetworkFailure>()));

      expect(auth.logoutCalls, 1);
      expect(await manager.restore(), isNotNull);
    },
  );

  test(
    'logout performs at most one refresh after an authentication failure',
    () async {
      await secureStore.writeCredentials(testCredentials());
      auth.logoutErrors.addAll([
        const AuthenticationFailure(code: 'authentication_required'),
        const AuthenticationFailure(code: 'authentication_required'),
      ]);

      await expectLater(
        manager.logout(),
        throwsA(isA<AuthenticationFailure>()),
      );

      expect(auth.refreshCalls, 1);
      expect(auth.logoutCalls, 2);
      expect(
        (await manager.restore())?.accessToken,
        'replacement-access-token',
      );
    },
  );

  test(
    'an unusable local refresh state requires explicit new-session recovery',
    () async {
      await secureStore.writeCredentials(
        testCredentials(accessExpired: true, refreshMayBeConsumed: true),
      );

      await expectLater(
        manager.logout(),
        throwsA(isA<SessionRecoveryFailure>()),
      );

      expect(auth.refreshCalls, 0);
      expect(auth.logoutCalls, 0);
      expect(await manager.restore(), isNotNull);
    },
  );

  test(
    'exhausted bootstrap proof is discarded only by new-session action',
    () async {
      auth.guestError = const NetworkFailure();

      await expectLater(
        manager.bootstrapGuest(),
        throwsA(isA<NetworkFailure>()),
      );
      await expectLater(
        manager.bootstrapGuest(),
        throwsA(isA<NetworkFailure>()),
      );
      await expectLater(
        manager.bootstrapGuest(),
        throwsA(isA<SessionRecoveryFailure>()),
      );

      expect(auth.guestCalls, 2);
      expect(auth.proofs.toSet(), hasLength(1));
      final exhaustedProof = auth.proofs.first;
      final installationId = auth.lastInstallationId;

      await manager.clearLocalSession();
      expect(auth.guestCalls, 2);
      auth.guestError = null;
      await manager.bootstrapGuest();

      expect(auth.guestCalls, 3);
      expect(auth.lastProof, isNot(exhaustedProof));
      expect(auth.lastInstallationId, installationId);
    },
  );
}
