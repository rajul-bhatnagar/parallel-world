import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:parallel_world_app/features/messaging/application/messaging_controllers.dart';

class ConversationListScreen extends ConsumerStatefulWidget {
  const ConversationListScreen({super.key});
  @override
  ConsumerState<ConversationListScreen> createState() => _State();
}

class _State extends ConsumerState<ConversationListScreen> {
  @override
  void initState() {
    super.initState();
    Future.microtask(
      () => ref.read(conversationListControllerProvider.notifier).load(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final s = ref.watch(conversationListControllerProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Messages')),
      body: RefreshIndicator(
        onRefresh: () => ref
            .read(conversationListControllerProvider.notifier)
            .load(refresh: true),
        child: s.loading && s.items.isEmpty
            ? const Center(child: CircularProgressIndicator())
            : ListView(
                children: [
                  if (s.offline)
                    const ListTile(
                      leading: Icon(Icons.cloud_off),
                      title: Text('Offline saved conversations'),
                    ),
                  if (s.message != null && s.items.isEmpty)
                    ListTile(
                      title: Text(s.message!),
                      trailing: IconButton(
                        onPressed: () => ref
                            .read(conversationListControllerProvider.notifier)
                            .load(refresh: true),
                        icon: const Icon(Icons.refresh),
                      ),
                    ),
                  if (s.items.isEmpty && !s.loading)
                    const Padding(
                      padding: EdgeInsets.all(32),
                      child: Text(
                        'No conversations yet. Open a character profile to start one.',
                        textAlign: TextAlign.center,
                      ),
                    ),
                  ...s.items.map(
                    (x) => ListTile(
                      leading: CircleAvatar(
                        child: Text(x.character.displayName.substring(0, 1)),
                      ),
                      title: Text(x.character.displayName),
                      subtitle: Text(
                        x.lastMessagePreview ?? 'No messages yet',
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      trailing: x.unreadCount > 0
                          ? Badge(label: Text('${x.unreadCount}'))
                          : null,
                      onTap: () => context.push(
                        '/conversations/${x.id}',
                        extra: x.character.displayName,
                      ),
                    ),
                  ),
                ],
              ),
      ),
    );
  }
}
