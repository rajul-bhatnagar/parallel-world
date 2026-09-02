import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/session/data/auth_api.dart';

import '../../support/fakes.dart';

const guestJson = {
  'accessToken': 'access-token',
  'accessTokenExpiresAtUtc': '2026-09-02T12:10:00Z',
  'refreshToken': 'refresh-token',
  'refreshTokenExpiresAtUtc': '2026-09-03T12:00:00Z',
  'user': {'id': '00000000-0000-0000-0000-000000000100'},
  'world': {
    'id': '00000000-0000-0000-0000-000000000101',
    'name': 'My Parallel World',
    'status': 'Active',
    'currentGameTimeUtc': '2026-09-02T12:00:00Z',
    'player': {
      'actorId': '00000000-0000-0000-0000-000000000102',
      'displayName': 'Player',
    },
    'createdAtUtc': '2026-09-02T12:00:00Z',
  },
};

void main() {
  test(
    'guest request matches the M03 contract and has no idempotency key',
    () async {
      late RequestOptions captured;
      final adapter = TestHttpClientAdapter((options, body) {
        captured = options;
        return ResponseBody.fromString(
          jsonEncode(guestJson),
          201,
          headers: {
            Headers.contentTypeHeader: [Headers.jsonContentType],
          },
        );
      });
      final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
        ..httpClientAdapter = adapter;

      final result = await AuthApi(dio).bootstrapGuest(
        installationId: 'installation-id',
        appVersion: '0.1.0',
        bootstrapProof: 'proof',
        worldName: 'My Parallel World',
      );

      expect(captured.path, '/api/v1/auth/guest');
      expect(captured.headers, isNot(contains('Idempotency-Key')));
      expect(captured.data, {
        'installationId': 'installation-id',
        'platform': 'android',
        'appVersion': '0.1.0',
        'guestBootstrapProof': 'proof',
        'worldName': 'My Parallel World',
      });
      expect(result.world.id, testWorld.id);
    },
  );

  test(
    'refresh remains non-idempotent and sends only the refresh token',
    () async {
      late RequestOptions captured;
      final adapter = TestHttpClientAdapter((options, body) {
        captured = options;
        return ResponseBody.fromString(
          jsonEncode({
            'accessToken': 'new-access',
            'accessTokenExpiresAtUtc': '2026-09-02T12:10:00Z',
            'refreshToken': 'new-refresh',
            'refreshTokenExpiresAtUtc': '2026-09-03T12:00:00Z',
          }),
          200,
          headers: {
            Headers.contentTypeHeader: [Headers.jsonContentType],
          },
        );
      });
      final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
        ..httpClientAdapter = adapter;

      await AuthApi(dio).refresh('refresh-token');

      expect(captured.path, '/api/v1/auth/refresh');
      expect(captured.headers, isNot(contains('Idempotency-Key')));
      expect(captured.data, {'refreshToken': 'refresh-token'});
    },
  );

  test('ProblemDetails response maps to the typed failure', () async {
    final adapter = TestHttpClientAdapter(
      (options, body) => ResponseBody.fromString(
        jsonEncode({
          'status': 401,
          'code': 'refresh_token_replayed',
          'traceId': 'trace-1',
        }),
        401,
        headers: {
          Headers.contentTypeHeader: [Headers.jsonContentType],
        },
      ),
    );
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = adapter;

    await expectLater(
      AuthApi(dio).refresh('refresh-token'),
      throwsA(
        isA<AuthenticationFailure>().having(
          (failure) => failure.code,
          'code',
          'refresh_token_replayed',
        ),
      ),
    );
  });
}
