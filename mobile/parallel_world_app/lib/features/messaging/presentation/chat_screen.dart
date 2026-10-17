import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/features/messaging/application/messaging_controllers.dart';
import 'package:parallel_world_app/features/messaging/domain/messaging_models.dart';

class ChatScreen extends ConsumerStatefulWidget {
  const ChatScreen({required this.conversationId, this.title, super.key});
  final String conversationId;
  final String? title;
  @override
  ConsumerState<ChatScreen> createState() => _ChatState();
}

class _ChatState extends ConsumerState<ChatScreen> {
  final text = TextEditingController();
  @override
  void initState() {
    super.initState();
    Future.microtask(
      () => ref
          .read(chatControllerProvider(widget.conversationId).notifier)
          .load(),
    );
  }

  @override
  void dispose() {
    text.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final s = ref.watch(chatControllerProvider(widget.conversationId));
    final c = ref.read(chatControllerProvider(widget.conversationId).notifier);
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title ?? 'Conversation'),
        actions: [
          IconButton(
            onPressed: () => c.load(refresh: true),
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh',
          ),
        ],
      ),
      body: SafeArea(
        child: Column(
          children: [
            if (s.offline)
              const MaterialBanner(
                content: Text('Offline — showing saved messages.'),
                actions: [SizedBox.shrink()],
              ),
            if (s.replyStatus == 'planned')
              const LinearProgressIndicator(
                semanticsLabel: 'Character reply pending',
              ),
            if (s.replyStatus == 'noresponse')
              const Padding(
                padding: EdgeInsets.all(8),
                child: Text('The character did not respond.'),
              ),
            if (s.message != null)
              Padding(
                padding: const EdgeInsets.all(8),
                child: Text(
                  s.message!,
                  style: TextStyle(color: Theme.of(context).colorScheme.error),
                ),
              ),
            Expanded(
              child: s.loading && s.items.isEmpty
                  ? const Center(child: CircularProgressIndicator())
                  : s.items.isEmpty
                  ? const Center(child: Text('Send the first message.'))
                  : ListView.builder(
                      reverse: true,
                      itemCount: s.items.length + (s.hasMore ? 1 : 0),
                      itemBuilder: (context, index) {
                        if (index == s.items.length) {
                          return TextButton(
                            onPressed: s.loadingMore ? null : c.loadMore,
                            child: Text(
                              s.loadingMore
                                  ? 'Loading…'
                                  : 'Load older messages',
                            ),
                          );
                        }
                        final m = s.items[index];
                        return _Bubble(
                          message: m,
                          onRetry: m.localState == MessageLocalState.failed
                              ? () => c.send(m.body, retry: m)
                              : null,
                        );
                      },
                    ),
            ),
            _Composer(
              controller: text,
              disabled: s.sending,
              onSend: () {
                final value = text.text;
                if (value.trim().isEmpty) return;
                text.clear();
                c.send(value);
              },
            ),
          ],
        ),
      ),
    );
  }
}

class _Bubble extends StatelessWidget {
  const _Bubble({required this.message, this.onRetry});
  final ConversationMessage message;
  final VoidCallback? onRetry;
  @override
  Widget build(BuildContext context) {
    final player = message.senderType == 'player';
    return Align(
      alignment: player ? Alignment.centerRight : Alignment.centerLeft,
      child: Semantics(
        label: '${player ? 'You' : 'Character'} message',
        child: Container(
          constraints: const BoxConstraints(maxWidth: 320),
          margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: player
                ? Theme.of(context).colorScheme.primaryContainer
                : Theme.of(context).colorScheme.surfaceContainerHighest,
            borderRadius: BorderRadius.circular(16),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(message.body),
              if (message.localState == MessageLocalState.pending)
                const Text('Sending…', style: TextStyle(fontSize: 12)),
              if (message.localState == MessageLocalState.failed)
                TextButton.icon(
                  onPressed: onRetry,
                  icon: const Icon(Icons.refresh),
                  label: const Text('Retry'),
                ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Composer extends StatelessWidget {
  const _Composer({
    required this.controller,
    required this.disabled,
    required this.onSend,
  });
  final TextEditingController controller;
  final bool disabled;
  final VoidCallback onSend;
  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.all(8),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        Expanded(
          child: TextField(
            controller: controller,
            maxLength: 2000,
            minLines: 1,
            maxLines: 5,
            enabled: !disabled,
            decoration: const InputDecoration(
              hintText: 'Write a message',
              border: OutlineInputBorder(),
            ),
          ),
        ),
        const SizedBox(width: 8),
        IconButton.filled(
          onPressed: disabled ? null : onSend,
          icon: const Icon(Icons.send),
          tooltip: 'Send message',
        ),
      ],
    ),
  );
}
