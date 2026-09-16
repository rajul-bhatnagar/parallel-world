import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/feed/data/feed_api.dart';

import '../../support/fakes.dart';

void main() {
  test(
    'maps the opaque collection envelope and feed post projection',
    () async {
      late RequestOptions captured;
      final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
        ..httpClientAdapter = TestHttpClientAdapter((options, body) {
          captured = options;
          return _jsonResponse({
            'items': [_postJson],
            'nextCursor': 'opaque-next-page',
            'hasMore': true,
          });
        });

      final page = await FeedApi(
        dio,
      ).getFeed(worldId: testWorld.id, limit: 3, cursor: 'opaque-prior-page');

      expect(captured.path, '/api/v1/worlds/${testWorld.id}/feed');
      expect(captured.queryParameters, {
        'limit': 3,
        'cursor': 'opaque-prior-page',
      });
      expect(page.items.single.author.displayName, 'Maya Chen');
      expect(page.items.single.parentPostId, isNull);
      expect(page.nextCursor, 'opaque-next-page');
      expect(page.hasMore, isTrue);
    },
  );

  test('sends compose operation identity without an author id', () async {
    late RequestOptions captured;
    late Map<String, Object?> requestBody;
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, body) {
        captured = options;
        requestBody = Map<String, Object?>.from(
          jsonDecode(utf8.decode(body!)) as Map,
        );
        return _jsonResponse(_postJson, statusCode: 201);
      });

    await FeedApi(dio).createPost(
      worldId: testWorld.id,
      content: 'Hello world',
      clientPostId: '00000000-0000-0000-0000-000000000350',
      idempotencyKey: 'operation-key',
    );

    expect(captured.headers['Idempotency-Key'], 'operation-key');
    expect(requestBody, {
      'content': 'Hello world',
      'clientPostId': '00000000-0000-0000-0000-000000000350',
    });
    expect(requestBody, isNot(contains('actorId')));
    expect(requestBody, isNot(contains('userId')));
  });
}

const _postJson = {
  'id': '00000000-0000-0000-0000-000000000301',
  'worldId': '00000000-0000-0000-0000-000000000101',
  'author': {
    'actorId': '00000000-0000-0000-0000-000000000202',
    'displayName': 'Maya Chen',
    'handle': 'maya',
    'actorType': 'character',
  },
  'content': 'A quiet start, and plenty to notice.',
  'createdAtUtc': '2026-09-02T12:00:00Z',
  'parent': null,
  'counts': {'likes': 0, 'replies': 0},
  'visibility': 'world',
};

ResponseBody _jsonResponse(Object value, {int statusCode = 200}) =>
    ResponseBody.fromString(
      jsonEncode(value),
      statusCode,
      headers: {
        Headers.contentTypeHeader: [Headers.jsonContentType],
      },
    );
