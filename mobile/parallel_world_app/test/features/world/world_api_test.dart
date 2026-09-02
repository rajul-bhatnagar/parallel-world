import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/world/data/world_api.dart';

import '../../support/fakes.dart';

const worldJson = {
  'id': '00000000-0000-0000-0000-000000000101',
  'name': 'My Parallel World',
  'status': 'Active',
  'currentGameTimeUtc': '2026-09-02T12:00:00Z',
  'player': {
    'actorId': '00000000-0000-0000-0000-000000000102',
    'displayName': 'Player',
  },
  'createdAtUtc': '2026-09-02T12:00:00Z',
};

void main() {
  test('loads the current owned world from the M03 route', () async {
    late RequestOptions captured;
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, body) {
        captured = options;
        return ResponseBody.fromString(
          jsonEncode(worldJson),
          200,
          headers: {
            Headers.contentTypeHeader: [Headers.jsonContentType],
          },
        );
      });

    final world = await WorldApi(dio).getCurrent();

    expect(captured.path, '/api/v1/worlds/current');
    expect(world.id, testWorld.id);
  });

  test(
    'world creation carries a stable caller-supplied idempotency key',
    () async {
      late RequestOptions captured;
      final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
        ..httpClientAdapter = TestHttpClientAdapter((options, body) {
          captured = options;
          return ResponseBody.fromString(
            jsonEncode(worldJson),
            201,
            headers: {
              Headers.contentTypeHeader: [Headers.jsonContentType],
            },
          );
        });

      await WorldApi(dio)
          .create(name: 'My Parallel World', idempotencyKey: 'stable-key');

      expect(captured.path, '/api/v1/worlds');
      expect(captured.headers['Idempotency-Key'], 'stable-key');
      expect(captured.data, {'name': 'My Parallel World'});
    },
  );
}
