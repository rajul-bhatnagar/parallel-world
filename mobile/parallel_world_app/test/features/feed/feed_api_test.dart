import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/feed/data/feed_api.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';

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

  test('maps reply, reaction, and follow contracts', () async {
    final requests = <RequestOptions>[];
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, body) {
        requests.add(options);
        if (options.path.endsWith('/replies') && options.method == 'GET') {
          return _jsonResponse({
            'items': [_postJson],
            'nextCursor': null,
            'hasMore': false,
          });
        }
        if (options.path.endsWith('/reaction')) {
          return _jsonResponse({
            'postId': _postJson['id'],
            'type': 'like',
            'active': true,
            'likeCount': 1,
          });
        }
        if (options.path.endsWith('/follow')) {
          return _jsonResponse({
            'actorId': (_postJson['author']! as Map)['actorId'],
            'isFollowing': true,
            'followedAtUtc': '2026-09-16T12:00:00Z',
          });
        }
        return _jsonResponse(_postJson, statusCode: 201);
      });
    final api = FeedApi(dio);

    final replies = await api.getReplies(
      worldId: testWorld.id,
      parentPostId: _postJson['id']! as String,
      cursor: 'reply-cursor',
    );
    final reply = await api.createReply(
      worldId: testWorld.id,
      parentPostId: _postJson['id']! as String,
      content: 'Reply',
      clientPostId: 'reply-client-id',
      idempotencyKey: 'reply-key',
    );
    final reaction = await api.setLike(worldId: testWorld.id, postId: reply.id);
    final follow = await api.follow(
      worldId: testWorld.id,
      actorId: reply.author.actorId,
    );
    await api.removeLike(worldId: testWorld.id, postId: reply.id);
    await api.unfollow(worldId: testWorld.id, actorId: reply.author.actorId);

    expect(replies.items.single.id, reply.id);
    expect(requests[0].queryParameters['cursor'], 'reply-cursor');
    expect(requests[1].headers['Idempotency-Key'], 'reply-key');
    expect(reaction, isA<ReactionState>());
    expect(reaction.likeCount, 1);
    expect(follow.isFollowing, isTrue);
    expect(
      requests.map((request) => request.method),
      containsAll(['DELETE', 'DELETE']),
    );
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
    'isFollowed': false,
  },
  'content': 'A quiet start, and plenty to notice.',
  'createdAtUtc': '2026-09-02T12:00:00Z',
  'parent': null,
  'counts': {'likes': 0, 'replies': 0},
  'currentPlayerReaction': null,
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
