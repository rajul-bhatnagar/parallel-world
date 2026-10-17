import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/features/messaging/application/messaging_contracts.dart';
import 'package:parallel_world_app/features/messaging/domain/messaging_models.dart';

class MessagingApi implements MessagingGateway {
  MessagingApi(this._dio);
  final Dio _dio;
  @override
  Future<ConversationPage> list(
    String worldId, {
    int limit = 20,
    String? cursor,
  }) async {
    try {
      final r = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/conversations',
        queryParameters: {'limit': limit, 'cursor': ?cursor},
      );
      final j = _object(r.data);
      return ConversationPage(
        _list(j['items']).map(ConversationSummary.fromJson).toList(),
        j['nextCursor'] as String?,
        j['hasMore'] as bool,
      );
    } on DioException catch (e) {
      throw mapDioException(e);
    }
  }

  @override
  Future<ConversationSummary> direct(
    String worldId,
    String characterId,
    String key,
  ) async {
    try {
      final r = await _dio.post<dynamic>(
        '/api/v1/worlds/$worldId/conversations/direct',
        data: {'characterId': characterId},
        options: Options(headers: {'Idempotency-Key': key}),
      );
      return ConversationSummary.fromJson(_object(r.data));
    } on DioException catch (e) {
      throw mapDioException(e);
    }
  }

  @override
  Future<MessagePage> messages(
    String worldId,
    String conversationId, {
    int limit = 20,
    String? cursor,
  }) async {
    try {
      final r = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/conversations/$conversationId/messages',
        queryParameters: {'limit': limit, 'cursor': ?cursor},
      );
      final j = _object(r.data);
      return MessagePage(
        _list(j['items']).map(ConversationMessage.fromJson).toList(),
        j['nextCursor'] as String?,
        j['hasMore'] as bool,
      );
    } on DioException catch (e) {
      throw mapDioException(e);
    }
  }

  @override
  Future<SendMessageResponse> send(
    String worldId,
    String conversationId,
    String body,
    String clientMessageId,
  ) async {
    try {
      final r = await _dio.post<dynamic>(
        '/api/v1/worlds/$worldId/conversations/$conversationId/messages',
        data: {
          'body': body,
          'clientMessageId': clientMessageId,
          'replyToMessageId': null,
        },
        options: Options(headers: {'Idempotency-Key': clientMessageId}),
      );
      final j = _object(r.data);
      return SendMessageResponse(
        ConversationMessage.fromJson(_object(j['message'])),
        j['characterReplyStatus'] as String,
      );
    } on DioException catch (e) {
      throw mapDioException(e);
    }
  }

  @override
  Future<void> markRead(
    String worldId,
    String conversationId,
    String messageId,
  ) async {
    try {
      await _dio.post<void>(
        '/api/v1/worlds/$worldId/conversations/$conversationId/read',
        data: {'lastReadMessageId': messageId},
      );
    } on DioException catch (e) {
      throw mapDioException(e);
    }
  }

  static Map<String, dynamic> _object(Object? v) {
    if (v is! Map) throw const FormatException('Expected JSON object.');
    return {
      for (final e in v.entries)
        if (e.key is String) e.key as String: e.value,
    };
  }

  static List<Map<String, dynamic>> _list(Object? v) {
    if (v is! List) throw const FormatException('Expected JSON list.');
    return v.map(_object).toList();
  }
}
