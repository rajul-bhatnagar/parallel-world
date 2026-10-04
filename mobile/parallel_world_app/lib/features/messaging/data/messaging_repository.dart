import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/messaging/application/messaging_contracts.dart';
import 'package:parallel_world_app/features/messaging/domain/messaging_models.dart';

class ApiCachedMessagingRepository implements MessagingRepository {
  ApiCachedMessagingRepository(this.api, this.cache);
  final MessagingGateway api;
  final MessagingCache cache;
  @override
  Future<CachedConversationList?> cachedConversations(String u, String w) =>
      cache.readConversations(u, w);
  @override
  Future<ConversationPage> fetchConversations(
    String u,
    String w, {
    bool Function()? isCurrent,
  }) async {
    final p = await api.list(w);
    if (isCurrent?.call() ?? true) {
      await cache.replaceConversations(u, w, p.items);
    }
    return p;
  }

  @override
  Future<ConversationSummary> openDirect(String w, String c, String k) =>
      api.direct(w, c, k);
  @override
  Future<CachedMessageHistory?> cachedMessages(String u, String w, String c) =>
      cache.readMessages(u, w, c);
  @override
  Future<MessagePage> fetchMessages(
    String u,
    String w,
    String c, {
    String? cursor,
    bool Function()? isCurrent,
  }) async {
    final p = await api.messages(w, c, cursor: cursor);
    if (isCurrent?.call() ?? true) {
      if (cursor == null) {
        await cache.replaceMessages(u, w, c, p);
      } else {
        await cache.appendMessages(u, w, c, p);
      }
    }
    return p;
  }

  @override
  Future<SendMessageResponse> send(
    String u,
    String w,
    String c,
    ConversationMessage pending, {
    bool Function()? isCurrent,
  }) async {
    if (!(isCurrent?.call() ?? true)) {
      throw const UnknownFailure();
    }
    await cache.putMessage(u, w, pending);
    try {
      final result = await api.send(
        w,
        c,
        pending.body,
        pending.clientMessageId!,
      );
      if (isCurrent?.call() ?? true) {
        await cache.putMessage(u, w, result.message);
      }
      return result;
    } on AppFailure catch (e) {
      if (isCurrent?.call() ?? true) {
        await cache.putMessage(
          u,
          w,
          pending.copyWith(
            localState: MessageLocalState.failed,
            failureMessage: e.message,
          ),
        );
      }
      rethrow;
    }
  }

  @override
  Future<void> markRead(String w, String c, String m) => api.markRead(w, c, m);
}
