import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/messaging/data/messaging_api.dart';

import '../../support/fakes.dart';

void main() {
  test('send uses one operation identity and never sends sender identity', () async {
    late RequestOptions captured;
    late Map<String, Object?> requestBody;
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, body) {
        captured = options;
        requestBody = Map<String, Object?>.from(
          jsonDecode(utf8.decode(body!)) as Map,
        );
        return _jsonResponse({
          'message': _messageJson,
          'characterReplyStatus': 'noresponse',
        }, statusCode: 201);
      });

    final result = await MessagingApi(dio)
        .send(testWorld.id, _conversationId, 'Hello Maya', _clientMessageId);

    expect(
      captured.path,
      '/api/v1/worlds/${testWorld.id}/conversations/$_conversationId/messages',
    );
    expect(captured.headers['Idempotency-Key'], _clientMessageId);
    expect(requestBody, {
      'body': 'Hello Maya',
      'clientMessageId': _clientMessageId,
      'replyToMessageId': null,
    });
    expect(requestBody, isNot(contains('senderActorId')));
    expect(result.message.senderType, 'player');
    expect(result.characterReplyStatus, 'noresponse');
  });

  test('message pagination keeps the server cursor opaque', () async {
    late RequestOptions captured;
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, body) {
        captured = options;
        return _jsonResponse({
          'items': [_messageJson],
          'nextCursor': 'protected.cursor.value',
          'hasMore': true,
        });
      });

    final page = await MessagingApi(dio)
        .messages(testWorld.id, _conversationId, cursor: 'prior.cursor');

    expect(captured.queryParameters['cursor'], 'prior.cursor');
    expect(page.nextCursor, 'protected.cursor.value');
    expect(page.hasMore, isTrue);
  });
}

const _conversationId = '00000000-0000-0000-0000-000000000501';
const _clientMessageId = '00000000-0000-0000-0000-000000000502';
const _messageJson = {
  'id': '00000000-0000-0000-0000-000000000503',
  'conversationId': _conversationId,
  'senderActorId': '00000000-0000-0000-0000-000000000102',
  'senderType': 'player',
  'body': 'Hello Maya',
  'createdAtUtc': '2026-09-02T12:00:00Z',
  'deliveryStatus': 'delivered',
  'clientMessageId': _clientMessageId,
};

ResponseBody _jsonResponse(Object value, {int statusCode = 200}) =>
    ResponseBody.fromString(
      jsonEncode(value),
      statusCode,
      headers: {
        Headers.contentTypeHeader: [Headers.jsonContentType],
      },
    );
