import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/features/notifications/application/notification_contracts.dart';

final notificationRepositoryProvider = Provider<NotificationRepository>(
  (ref) => throw StateError('NotificationRepository was not wired by the app.'),
);
