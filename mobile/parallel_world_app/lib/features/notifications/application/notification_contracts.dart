import 'package:parallel_world_app/features/notifications/domain/notification_models.dart';

class NotificationScope {
  const NotificationScope(this.userId, this.worldId);
  final String userId;
  final String worldId;

  @override
  bool operator ==(Object other) =>
      other is NotificationScope &&
      other.userId == userId &&
      other.worldId == worldId;
  @override
  int get hashCode => Object.hash(userId, worldId);
}

abstract interface class NotificationGateway {
  Future<NotificationPage> list({
    required String worldId,
    String? cursor,
    int limit = 30,
  });
  Future<int> unreadCount({required String worldId});
  Future<void> markRead({
    required String worldId,
    required String notificationId,
  });
  Future<void> markAllRead({required String worldId});
}

abstract interface class NotificationCache {
  Future<NotificationPage> read(NotificationScope scope);
  Future<void> replace(NotificationScope scope, NotificationPage page);
  Future<void> append(NotificationScope scope, NotificationPage page);
  Future<void> markRead(NotificationScope scope, String notificationId);
  Future<void> markAllRead(NotificationScope scope);
  Future<int> unreadCount(NotificationScope scope);
}

abstract interface class NotificationRepository {
  Future<NotificationPage> load(NotificationScope scope, {String? cursor});
  Future<int> unreadCount(NotificationScope scope);
  Future<void> markRead(NotificationScope scope, String notificationId);
  Future<void> markAllRead(NotificationScope scope);
}
