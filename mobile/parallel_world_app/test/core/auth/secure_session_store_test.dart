import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/auth/secret_generator.dart';
import 'package:parallel_world_app/core/auth/secure_session_store.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';

import '../../support/fakes.dart';

void main() {
  group('SecureSessionStore', () {
    late MemorySecureKeyValueStore storage;
    late SecureSessionStore store;

    setUp(() {
      storage = MemorySecureKeyValueStore();
      store = SecureSessionStore(storage, SecretGenerator());
    });

    test(
      'keeps a stable installation identifier separate from authentication',
      () async {
        final first = await store.loadOrCreateInstallationId();
        final second = await store.loadOrCreateInstallationId();

        expect(second, first);
        expect(first, matches(RegExp(r'^[0-9a-f-]{36}$')));
        expect(storage.values.length, 1);
      },
    );

    test(
      'persists one proof and permits only one recovery transmission',
      () async {
        final first = await store.beginBootstrapAttempt();
        final recovery = await store.beginBootstrapAttempt();

        expect(
          base64Url.decode(base64Url.normalize(first.value)),
          hasLength(32),
        );
        expect(recovery.value, first.value);
        expect(recovery.attempts, 2);
        await expectLater(
          store.beginBootstrapAttempt(),
          throwsA(isA<SessionRecoveryFailure>()),
        );
      },
    );

    test('stores and removes the complete credential envelope', () async {
      final credentials = testCredentials();
      await store.writeCredentials(credentials);

      expect((await store.readCredentials())?.refreshToken, 'refresh-token');
      await store.clearSession();
      expect(await store.readCredentials(), isNull);
      expect(await store.loadOrCreateInstallationId(), isNotEmpty);
    });

    test('fails closed when secure storage cannot be read', () async {
      storage.failReads = true;
      await expectLater(
        store.readCredentials(),
        throwsA(isA<SecureStorageFailure>()),
      );
    });
  });
}
