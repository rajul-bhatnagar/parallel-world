import 'package:drift/drift.dart';
import 'package:parallel_world_app/features/notifications/application/notification_contracts.dart';
import 'package:parallel_world_app/features/notifications/domain/notification_models.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';

class DriftNotificationCache implements NotificationCache {
  DriftNotificationCache(this._db, {DateTime Function()? utcNow})
    : _utcNow = utcNow ?? (() => DateTime.now().toUtc());

  final AppDatabase _db;
  final DateTime Function() _utcNow;

  @override
  Future<NotificationPage> read(NotificationScope scope) async {
    await _db.ensureNotificationTables();
    final rows = await _db
        .customSelect(
          'SELECT notification_id, category, title, resource_id, is_read, created_at_utc FROM cached_m16_notifications WHERE user_id = ? AND world_id = ? ORDER BY created_at_utc DESC, notification_id DESC',
          variables: [Variable(scope.userId), Variable(scope.worldId)],
        )
        .get();
    final metadata = await _db
        .customSelect(
          'SELECT next_cursor, has_more FROM cached_m16_notification_metadata WHERE user_id = ? AND world_id = ?',
          variables: [Variable(scope.userId), Variable(scope.worldId)],
        )
        .getSingleOrNull();
    return NotificationPage(
      items: rows.map(_map).toList(),
      nextCursor: metadata?.readNullable<String>('next_cursor'),
      hasMore: (metadata?.read<int>('has_more') ?? 0) != 0,
      isOffline: true,
    );
  }

  @override
  Future<void> replace(NotificationScope scope, NotificationPage page) =>
      _write(scope, page, replace: true);

  @override
  Future<void> append(NotificationScope scope, NotificationPage page) =>
      _write(scope, page, replace: false);

  Future<void> _write(
    NotificationScope scope,
    NotificationPage page, {
    required bool replace,
  }) => _db.transaction(() async {
    await _db.ensureNotificationTables();
    if (replace) {
      await _db.customStatement(
        'DELETE FROM cached_m16_notifications WHERE user_id = ? AND world_id = ?',
        [scope.userId, scope.worldId],
      );
    }
    for (final item in page.items) {
      await _db.customStatement(
        'INSERT OR REPLACE INTO cached_m16_notifications (user_id, world_id, notification_id, category, title, resource_id, is_read, created_at_utc) VALUES (?, ?, ?, ?, ?, ?, ?, ?)',
        [
          scope.userId,
          scope.worldId,
          item.id,
          item.category,
          item.title,
          item.resourceId,
          item.isRead ? 1 : 0,
          item.createdAtUtc.millisecondsSinceEpoch,
        ],
      );
    }
    await _db.customStatement(
      'INSERT OR REPLACE INTO cached_m16_notification_metadata (user_id, world_id, next_cursor, has_more, cached_at_utc) VALUES (?, ?, ?, ?, ?)',
      [
        scope.userId,
        scope.worldId,
        page.nextCursor,
        page.hasMore ? 1 : 0,
        _utcNow().millisecondsSinceEpoch,
      ],
    );
  });

  @override
  Future<void> markRead(NotificationScope scope, String notificationId) async {
    await _db.ensureNotificationTables();
    await _db.customStatement(
      'UPDATE cached_m16_notifications SET is_read = 1 WHERE user_id = ? AND world_id = ? AND notification_id = ?',
      [scope.userId, scope.worldId, notificationId],
    );
  }

  @override
  Future<void> markAllRead(NotificationScope scope) async {
    await _db.ensureNotificationTables();
    await _db.customStatement(
      'UPDATE cached_m16_notifications SET is_read = 1 WHERE user_id = ? AND world_id = ?',
      [scope.userId, scope.worldId],
    );
  }

  @override
  Future<int> unreadCount(NotificationScope scope) async {
    await _db.ensureNotificationTables();
    final row = await _db
        .customSelect(
          'SELECT COUNT(*) AS value FROM cached_m16_notifications WHERE user_id = ? AND world_id = ? AND is_read = 0',
          variables: [Variable(scope.userId), Variable(scope.worldId)],
        )
        .getSingle();
    return row.read<int>('value');
  }

  static AppNotification _map(QueryRow row) => AppNotification(
    id: row.read<String>('notification_id'),
    category: row.read<String>('category'),
    title: row.read<String>('title'),
    resourceId: row.read<String>('resource_id'),
    isRead: row.read<int>('is_read') != 0,
    createdAtUtc: DateTime.fromMillisecondsSinceEpoch(
      row.read<int>('created_at_utc'),
      isUtc: true,
    ),
  );
}
