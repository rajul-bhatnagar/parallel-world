import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/notifications/application/notification_contracts.dart';
import 'package:parallel_world_app/features/notifications/domain/notification_models.dart';

class ApiCachedNotificationRepository implements NotificationRepository {
  ApiCachedNotificationRepository(this._gateway, this._cache);
  final NotificationGateway _gateway;
  final NotificationCache _cache;

  @override
  Future<NotificationPage> load(
    NotificationScope scope, {
    String? cursor,
  }) async {
    try {
      final page = await _gateway.list(worldId: scope.worldId, cursor: cursor);
      if (cursor == null) {
        await _cache.replace(scope, page);
      } else {
        await _cache.append(scope, page);
      }
      return page;
    } on NetworkFailure {
      if (cursor != null) rethrow;
      return _cache.read(scope);
    }
  }

  @override
  Future<int> unreadCount(NotificationScope scope) async {
    try {
      return await _gateway.unreadCount(worldId: scope.worldId);
    } on NetworkFailure {
      return _cache.unreadCount(scope);
    }
  }

  @override
  Future<void> markRead(NotificationScope scope, String notificationId) async {
    await _gateway.markRead(
      worldId: scope.worldId,
      notificationId: notificationId,
    );
    await _cache.markRead(scope, notificationId);
  }

  @override
  Future<void> markAllRead(NotificationScope scope) async {
    await _gateway.markAllRead(worldId: scope.worldId);
    await _cache.markAllRead(scope);
  }
}
