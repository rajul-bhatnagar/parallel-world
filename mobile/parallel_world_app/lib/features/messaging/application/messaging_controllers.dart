import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/messaging/application/messaging_dependencies.dart';
import 'package:parallel_world_app/features/messaging/domain/messaging_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_dependencies.dart';

final conversationListControllerProvider =
    NotifierProvider.autoDispose<
      ConversationListController,
      ConversationListState
    >(ConversationListController.new);
final chatControllerProvider = NotifierProvider.autoDispose
    .family<ChatController, ChatState, String>(ChatController.new);

class ConversationListState {
  const ConversationListState({
    this.items = const [],
    this.loading = false,
    this.offline = false,
    this.message,
  });
  final List<ConversationSummary> items;
  final bool loading;
  final bool offline;
  final String? message;
}

class ConversationListController extends Notifier<ConversationListState> {
  int generation = 0;
  @override
  ConversationListState build() {
    ref.watch(sessionControllerProvider);
    generation++;
    return const ConversationListState();
  }

  Future<void> load({bool refresh = false}) async {
    final s = ref.read(sessionControllerProvider);
    final u = s.userId, w = s.world?.id, g = ++generation;
    if (u == null || w == null) return;
    final repo = ref.read(messagingRepositoryProvider);
    if (!refresh) {
      final cached = await repo.cachedConversations(u, w);
      if (g != generation) return;
      if (cached != null) {
        state = ConversationListState(items: cached.items, loading: true);
      } else {
        state = const ConversationListState(loading: true);
      }
    }
    try {
      final p = await repo.fetchConversations(
        u,
        w,
        isCurrent: () =>
            g == generation && ref.read(sessionControllerProvider).userId == u,
      );
      if (g == generation) state = ConversationListState(items: p.items);
    } on NetworkFailure catch (e) {
      if (g == generation) {
        state = ConversationListState(
          items: state.items,
          offline: state.items.isNotEmpty,
          message: state.items.isEmpty
              ? e.message
              : 'Offline — showing saved conversations.',
        );
      }
    } on AppFailure catch (e) {
      if (g == generation) {
        state = ConversationListState(items: state.items, message: e.message);
      }
    }
  }

  Future<ConversationSummary?> openDirect(String characterId) async {
    final s = ref.read(sessionControllerProvider);
    if (s.world == null) return null;
    return ref
        .read(messagingRepositoryProvider)
        .openDirect(
          s.world!.id,
          characterId,
          ref.read(secretGeneratorProvider).newIdempotencyKey(),
        );
  }
}

class ChatState {
  const ChatState({
    this.items = const [],
    this.nextCursor,
    this.hasMore = false,
    this.loading = false,
    this.loadingMore = false,
    this.sending = false,
    this.offline = false,
    this.replyStatus,
    this.message,
  });
  final List<ConversationMessage> items;
  final String? nextCursor;
  final bool hasMore, loading, loadingMore, sending, offline;
  final String? replyStatus, message;
  ChatState copyWith({
    List<ConversationMessage>? items,
    String? nextCursor,
    bool? hasMore,
    bool? loading,
    bool? loadingMore,
    bool? sending,
    bool? offline,
    String? replyStatus,
    String? message,
    bool clearMessage = false,
  }) => ChatState(
    items: items ?? this.items,
    nextCursor: nextCursor ?? this.nextCursor,
    hasMore: hasMore ?? this.hasMore,
    loading: loading ?? this.loading,
    loadingMore: loadingMore ?? this.loadingMore,
    sending: sending ?? this.sending,
    offline: offline ?? this.offline,
    replyStatus: replyStatus ?? this.replyStatus,
    message: clearMessage ? null : message ?? this.message,
  );
}

class ChatController extends Notifier<ChatState> {
  ChatController(this.conversationId);
  final String conversationId;
  int generation = 0;
  @override
  ChatState build() {
    ref.watch(sessionControllerProvider);
    generation++;
    return const ChatState();
  }

  Future<void> load({bool refresh = false}) async {
    final s = ref.read(sessionControllerProvider);
    final u = s.userId, w = s.world?.id, g = ++generation;
    if (u == null || w == null) return;
    final repo = ref.read(messagingRepositoryProvider);
    if (!refresh) {
      final c = await repo.cachedMessages(u, w, conversationId);
      if (g != generation) return;
      state = c == null
          ? const ChatState(loading: true)
          : ChatState(
              items: c.items,
              nextCursor: c.nextCursor,
              hasMore: c.hasMore,
              loading: true,
            );
    }
    try {
      final p = await repo.fetchMessages(
        u,
        w,
        conversationId,
        isCurrent: () => _current(g, u, w),
      );
      if (g == generation) {
        final local = state.items
            .where((x) => x.localState != MessageLocalState.synced)
            .toList();
        final ids = local.map((x) => x.clientMessageId).toSet();
        state = ChatState(
          items: [
            ...local,
            ...p.items.where((x) => !ids.contains(x.clientMessageId)),
          ],
          nextCursor: p.nextCursor,
          hasMore: p.hasMore,
        );
        if (p.items.isNotEmpty) {
          unawaited(repo.markRead(w, conversationId, p.items.first.id));
        }
      }
    } on NetworkFailure catch (e) {
      if (g == generation) {
        state = state.copyWith(
          loading: false,
          offline: state.items.isNotEmpty,
          message: state.items.isEmpty
              ? e.message
              : 'Offline — showing saved messages.',
        );
      }
    } on AppFailure catch (e) {
      if (g == generation) {
        state = state.copyWith(loading: false, message: e.message);
      }
    }
  }

  Future<void> loadMore() async {
    if (state.loadingMore || !state.hasMore) return;
    final s = ref.read(sessionControllerProvider);
    final u = s.userId, w = s.world?.id, g = generation;
    if (u == null || w == null) return;
    state = state.copyWith(loadingMore: true);
    try {
      final p = await ref
          .read(messagingRepositoryProvider)
          .fetchMessages(
            u,
            w,
            conversationId,
            cursor: state.nextCursor,
            isCurrent: () => _current(g, u, w),
          );
      if (_current(g, u, w)) {
        final ids = state.items.map((x) => x.id).toSet();
        state = state.copyWith(
          items: [...state.items, ...p.items.where((x) => ids.add(x.id))],
          nextCursor: p.nextCursor,
          hasMore: p.hasMore,
          loadingMore: false,
        );
      }
    } on AppFailure catch (e) {
      if (_current(g, u, w)) {
        state = state.copyWith(loadingMore: false, message: e.message);
      }
    }
  }

  Future<void> send(String body, {ConversationMessage? retry}) async {
    final normalized = body.trim();
    if (normalized.isEmpty || normalized.runes.length > 2000) return;
    final s = ref.read(sessionControllerProvider);
    final u = s.userId,
        w = s.world?.id,
        p = s.world?.playerActorId,
        g = generation;
    if (u == null || w == null || p == null) return;
    final id =
        retry?.clientMessageId ??
        _uuid(ref.read(secretGeneratorProvider).newIdempotencyKey());
    final pending =
        retry ??
        ConversationMessage(
          id: id,
          conversationId: conversationId,
          senderActorId: p,
          senderType: 'player',
          body: normalized,
          createdAtUtc: DateTime.now().toUtc(),
          deliveryStatus: 'pending',
          clientMessageId: id,
          localState: MessageLocalState.pending,
        );
    state = state.copyWith(
      items: [pending, ...state.items.where((x) => x.clientMessageId != id)],
      sending: true,
      clearMessage: true,
    );
    try {
      final r = await ref
          .read(messagingRepositoryProvider)
          .send(
            u,
            w,
            conversationId,
            pending,
            isCurrent: () => _current(g, u, w),
          );
      if (_current(g, u, w)) {
        state = state.copyWith(
          items: [
            r.message,
            ...state.items.where((x) => x.clientMessageId != id),
          ],
          sending: false,
          replyStatus: r.characterReplyStatus,
        );
        await load(refresh: true);
      }
    } on AppFailure catch (e) {
      if (_current(g, u, w)) {
        state = state.copyWith(
          items: state.items
              .map(
                (x) => x.clientMessageId == id
                    ? x.copyWith(
                        localState: MessageLocalState.failed,
                        failureMessage: e.message,
                      )
                    : x,
              )
              .toList(),
          sending: false,
          offline: e is NetworkFailure,
          message: e.message,
        );
      }
    }
  }

  bool _current(int g, String u, String w) {
    final s = ref.read(sessionControllerProvider);
    return g == generation && s.userId == u && s.world?.id == w;
  }

  static String _uuid(String x) {
    final v = x.padRight(32, '0').substring(0, 32);
    return '${v.substring(0, 8)}-${v.substring(8, 12)}-${v.substring(12, 16)}-${v.substring(16, 20)}-${v.substring(20)}';
  }
}
