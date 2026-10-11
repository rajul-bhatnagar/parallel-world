import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/features/notifications/application/notification_controller.dart';
import 'package:parallel_world_app/features/notifications/domain/notification_models.dart';

class NotificationScreen extends ConsumerStatefulWidget {
  const NotificationScreen({super.key});

  @override
  ConsumerState<NotificationScreen> createState() => _NotificationScreenState();
}

class _NotificationScreenState extends ConsumerState<NotificationScreen> {
  @override
  void initState() {
    super.initState();
    Future<void>.microtask(
      () => ref.read(notificationControllerProvider.notifier).load(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(notificationControllerProvider);
    final controller = ref.read(notificationControllerProvider.notifier);
    return Scaffold(
      appBar: AppBar(
        title: const Text('Notifications'),
        actions: [
          TextButton(
            onPressed: state.items.any((item) => !item.isRead)
                ? controller.markAllRead
                : null,
            child: const Text('Read all'),
          ),
        ],
      ),
      body: SafeArea(
        child: RefreshIndicator(
          onRefresh: () => controller.load(refresh: true),
          child: _body(context, state, controller),
        ),
      ),
    );
  }

  Widget _body(
    BuildContext context,
    NotificationState state,
    NotificationController controller,
  ) {
    if (state.isLoading && state.items.isEmpty) {
      return ListView(
        children: const [
          SizedBox(height: 240),
          Center(child: CircularProgressIndicator()),
        ],
      );
    }
    if (state.items.isEmpty) {
      return ListView(
        padding: const EdgeInsets.all(AppSpacing.large),
        children: [
          if (state.message != null)
            Text(state.message!, textAlign: TextAlign.center),
          const SizedBox(height: 160),
          const Icon(Icons.notifications_none, size: 48),
          const SizedBox(height: AppSpacing.small),
          const Text('No notifications yet', textAlign: TextAlign.center),
        ],
      );
    }
    return ListView(
      padding: const EdgeInsets.all(AppSpacing.medium),
      children: [
        if (state.isOffline)
          const Card(
            child: ListTile(
              leading: Icon(Icons.cloud_off_outlined),
              title: Text('Offline cached notifications'),
            ),
          ),
        if (state.message != null)
          Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.small),
            child: Text(state.message!),
          ),
        for (final item in state.items)
          Card(
            child: ListTile(
              leading: Icon(_icon(item.category)),
              title: Text(
                item.title,
                style: TextStyle(
                  fontWeight: item.isRead ? FontWeight.normal : FontWeight.bold,
                ),
              ),
              subtitle: Text(_time(item.createdAtUtc)),
              trailing: item.isRead ? null : const Icon(Icons.circle, size: 10),
              onTap: () async {
                if (!item.isRead) await controller.markRead(item.id);
                if (!context.mounted) return;
                context.push(_route(item));
              },
            ),
          ),
        if (state.hasMore)
          TextButton(
            onPressed: state.isLoadingMore ? null : controller.loadMore,
            child: state.isLoadingMore
                ? const CircularProgressIndicator()
                : const Text('Load more'),
          ),
      ],
    );
  }

  static IconData _icon(String category) => switch (category) {
    'privateMessage' => Icons.chat_bubble_outline,
    'catchUpSummary' => Icons.history,
    _ => Icons.reply,
  };

  static String _route(AppNotification item) => switch (item.category) {
    'privateMessage' => '/conversations/${item.resourceId}',
    'reply' => '/posts/${item.resourceId}',
    _ => '/home',
  };

  static String _time(DateTime value) =>
      value.toLocal().toString().substring(0, 16);
}
