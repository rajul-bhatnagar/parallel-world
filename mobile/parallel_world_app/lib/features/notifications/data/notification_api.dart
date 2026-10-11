import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/features/notifications/application/notification_contracts.dart';
import 'package:parallel_world_app/features/notifications/domain/notification_models.dart';

class NotificationApi implements NotificationGateway {
  NotificationApi(this._dio);
  final Dio _dio;

  @override
  Future<NotificationPage> list({
    required String worldId,
    String? cursor,
    int limit = 30,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/notifications',
        queryParameters: {'limit': limit, 'cursor': ?cursor},
      );
      final json = _object(response.data);
      return NotificationPage(
        items: _list(json['items'])
            .map((item) => AppNotification.fromJson(_object(item)))
            .toList(),
        nextCursor: json['nextCursor'] as String?,
        hasMore: json['hasMore']! as bool,
      );
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<int> unreadCount({required String worldId}) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/notifications/unread-count',
      );
      return _object(response.data)['count']! as int;
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<void> markRead({
    required String worldId,
    required String notificationId,
  }) async {
    try {
      await _dio.post<void>(
        '/api/v1/worlds/$worldId/notifications/$notificationId/read',
      );
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<void> markAllRead({required String worldId}) async {
    try {
      await _dio.post<void>('/api/v1/worlds/$worldId/notifications/read-all');
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  static Map<String, Object?> _object(Object? value) {
    if (value is! Map) throw const FormatException('Expected a JSON object.');
    return {
      for (final entry in value.entries)
        if (entry.key is String) entry.key as String: entry.value,
    };
  }

  static List<Object?> _list(Object? value) {
    if (value is! List) throw const FormatException('Expected a JSON list.');
    return value.cast<Object?>();
  }
}
