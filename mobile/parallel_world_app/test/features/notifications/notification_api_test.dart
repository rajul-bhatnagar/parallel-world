import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/notifications/data/notification_api.dart';

import '../../support/fakes.dart';

void main() {
  test(
    'list preserves the bounded cursor and typed notification data',
    () async {
      late RequestOptions captured;
      final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
        ..httpClientAdapter = TestHttpClientAdapter((options, body) {
          captured = options;
          return _json({
            'items': [
              {
                'id': '00000000-0000-0000-0000-000000000501',
                'category': 'privateMessage',
                'title': 'New private message',
                'resourceId': '00000000-0000-0000-0000-000000000502',
                'isRead': false,
                'createdAtUtc': '2026-10-11T04:00:00Z',
              },
            ],
            'nextCursor': 'opaque-cursor',
            'hasMore': true,
          });
        });

      final page = await NotificationApi(dio)
          .list(worldId: testWorld.id, cursor: 'previous-cursor', limit: 12);

      expect(captured.method, 'GET');
      expect(captured.path, '/api/v1/worlds/${testWorld.id}/notifications');
      expect(captured.queryParameters, {
        'limit': 12,
        'cursor': 'previous-cursor',
      });
      expect(page.items.single.category, 'privateMessage');
      expect(page.nextCursor, 'opaque-cursor');
      expect(page.hasMore, isTrue);
    },
  );

  test('read operations use only server-authoritative routes', () async {
    final requests = <RequestOptions>[];
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, body) {
        requests.add(options);
        return options.path.endsWith('unread-count')
            ? _json({'count': 3})
            : ResponseBody.fromString('', 204);
      });
    final api = NotificationApi(dio);

    expect(await api.unreadCount(worldId: testWorld.id), 3);
    await api.markRead(
      worldId: testWorld.id,
      notificationId: '00000000-0000-0000-0000-000000000503',
    );
    await api.markAllRead(worldId: testWorld.id);

    expect(requests.map((request) => request.method), ['GET', 'POST', 'POST']);
    expect(requests.map((request) => request.path), [
      '/api/v1/worlds/${testWorld.id}/notifications/unread-count',
      '/api/v1/worlds/${testWorld.id}/notifications/00000000-0000-0000-0000-000000000503/read',
      '/api/v1/worlds/${testWorld.id}/notifications/read-all',
    ]);
  });
}

ResponseBody _json(Object value) => ResponseBody.fromString(
  jsonEncode(value),
  200,
  headers: {
    Headers.contentTypeHeader: [Headers.jsonContentType],
  },
);
