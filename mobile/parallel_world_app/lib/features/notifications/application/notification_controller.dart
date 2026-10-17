import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/features/notifications/application/notification_contracts.dart';
import 'package:parallel_world_app/features/notifications/application/notification_dependencies.dart';
import 'package:parallel_world_app/features/notifications/domain/notification_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';

class NotificationState {
  const NotificationState({
    this.items = const [],
    this.nextCursor,
    this.hasMore = false,
    this.isLoading = false,
    this.isLoadingMore = false,
    this.isOffline = false,
    this.message,
  });

  final List<AppNotification> items;
  final String? nextCursor;
  final bool hasMore;
  final bool isLoading;
  final bool isLoadingMore;
  final bool isOffline;
  final String? message;

  NotificationState copyWith({
    List<AppNotification>? items,
    String? nextCursor,
    bool clearCursor = false,
    bool? hasMore,
    bool? isLoading,
    bool? isLoadingMore,
    bool? isOffline,
    String? message,
    bool clearMessage = false,
  }) => NotificationState(
    items: items ?? this.items,
    nextCursor: clearCursor ? null : nextCursor ?? this.nextCursor,
    hasMore: hasMore ?? this.hasMore,
    isLoading: isLoading ?? this.isLoading,
    isLoadingMore: isLoadingMore ?? this.isLoadingMore,
    isOffline: isOffline ?? this.isOffline,
    message: clearMessage ? null : message ?? this.message,
  );
}

final notificationControllerProvider =
    NotifierProvider.autoDispose<NotificationController, NotificationState>(
      NotificationController.new,
    );

final notificationUnreadCountProvider = FutureProvider.autoDispose<int>((
  ref,
) async {
  final session = ref.watch(sessionControllerProvider);
  final userId = session.userId;
  final worldId = session.world?.id;
  if (userId == null || worldId == null) return 0;
  return ref
      .read(notificationRepositoryProvider)
      .unreadCount(NotificationScope(userId, worldId));
});

class NotificationController extends Notifier<NotificationState> {
  NotificationScope? _scope;
  int _generation = 0;

  @override
  NotificationState build() {
    final session = ref.watch(sessionControllerProvider);
    final userId = session.userId;
    final worldId = session.world?.id;
    _scope = userId == null || worldId == null
        ? null
        : NotificationScope(userId, worldId);
    _generation++;
    return const NotificationState();
  }

  Future<void> load({bool refresh = false}) async {
    final scope = _scope;
    if (scope == null || state.isLoading) return;
    final generation = _generation;
    state = state.copyWith(isLoading: true, clearMessage: true);
    try {
      final page = await ref.read(notificationRepositoryProvider).load(scope);
      if (!_current(scope, generation)) return;
      state = NotificationState(
        items: _dedupe(page.items),
        nextCursor: page.nextCursor,
        hasMore: page.hasMore,
        isOffline: page.isOffline,
      );
      ref.invalidate(notificationUnreadCountProvider);
    } catch (_) {
      if (_current(scope, generation)) {
        state = state.copyWith(
          isLoading: false,
          message: 'Notifications could not be refreshed.',
        );
      }
    }
  }

  Future<void> loadMore() async {
    final scope = _scope;
    final cursor = state.nextCursor;
    if (scope == null ||
        cursor == null ||
        !state.hasMore ||
        state.isLoadingMore) {
      return;
    }
    final generation = _generation;
    state = state.copyWith(isLoadingMore: true, clearMessage: true);
    try {
      final page = await ref
          .read(notificationRepositoryProvider)
          .load(scope, cursor: cursor);
      if (!_current(scope, generation)) return;
      state = state.copyWith(
        items: _dedupe([...state.items, ...page.items]),
        nextCursor: page.nextCursor,
        clearCursor: page.nextCursor == null,
        hasMore: page.hasMore,
        isLoadingMore: false,
      );
    } catch (_) {
      if (_current(scope, generation)) {
        state = state.copyWith(
          isLoadingMore: false,
          message: 'More notifications could not be loaded.',
        );
      }
    }
  }

  Future<void> markRead(String id) async {
    final scope = _scope;
    if (scope == null) return;
    final generation = _generation;
    try {
      await ref.read(notificationRepositoryProvider).markRead(scope, id);
      if (!_current(scope, generation)) return;
      state = state.copyWith(
        items: state.items
            .map((item) => item.id == id ? item.copyWith(isRead: true) : item)
            .toList(),
      );
      ref.invalidate(notificationUnreadCountProvider);
    } catch (_) {
      if (_current(scope, generation)) {
        state = state.copyWith(
          message: 'The notification could not be marked read.',
        );
      }
    }
  }

  Future<void> markAllRead() async {
    final scope = _scope;
    if (scope == null) return;
    final generation = _generation;
    try {
      await ref.read(notificationRepositoryProvider).markAllRead(scope);
      if (!_current(scope, generation)) return;
      state = state.copyWith(
        items: state.items.map((item) => item.copyWith(isRead: true)).toList(),
      );
      ref.invalidate(notificationUnreadCountProvider);
    } catch (_) {
      if (_current(scope, generation)) {
        state = state.copyWith(
          message: 'Notifications could not be marked read.',
        );
      }
    }
  }

  bool _current(NotificationScope scope, int generation) =>
      generation == _generation && scope == _scope;

  static List<AppNotification> _dedupe(List<AppNotification> items) {
    final byId = <String, AppNotification>{};
    for (final item in items) {
      byId[item.id] = item;
    }
    final result = byId.values.toList()
      ..sort((a, b) {
        final time = b.createdAtUtc.compareTo(a.createdAtUtc);
        return time != 0 ? time : b.id.compareTo(a.id);
      });
    return result;
  }
}
