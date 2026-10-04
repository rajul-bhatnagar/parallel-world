import 'package:parallel_world_app/features/messaging/domain/messaging_models.dart';

abstract interface class MessagingGateway {
  Future<ConversationPage> list(
    String worldId, {
    int limit = 20,
    String? cursor,
  });
  Future<ConversationSummary> direct(
    String worldId,
    String characterId,
    String key,
  );
  Future<MessagePage> messages(
    String worldId,
    String conversationId, {
    int limit = 20,
    String? cursor,
  });
  Future<SendMessageResponse> send(
    String worldId,
    String conversationId,
    String body,
    String clientMessageId,
  );
  Future<void> markRead(
    String worldId,
    String conversationId,
    String messageId,
  );
}

class CachedConversationList {
  const CachedConversationList(this.items, this.cachedAtUtc);
  final List<ConversationSummary> items;
  final DateTime cachedAtUtc;
}

class CachedMessageHistory {
  const CachedMessageHistory(
    this.items,
    this.nextCursor,
    this.hasMore,
    this.cachedAtUtc,
  );
  final List<ConversationMessage> items;
  final String? nextCursor;
  final bool hasMore;
  final DateTime cachedAtUtc;
}

abstract interface class MessagingCache {
  Future<CachedConversationList?> readConversations(
    String userId,
    String worldId,
  );
  Future<void> replaceConversations(
    String userId,
    String worldId,
    List<ConversationSummary> items,
  );
  Future<CachedMessageHistory?> readMessages(
    String userId,
    String worldId,
    String conversationId,
  );
  Future<void> replaceMessages(
    String userId,
    String worldId,
    String conversationId,
    MessagePage page,
  );
  Future<void> appendMessages(
    String userId,
    String worldId,
    String conversationId,
    MessagePage page,
  );
  Future<void> putMessage(
    String userId,
    String worldId,
    ConversationMessage message,
  );
}

abstract interface class MessagingRepository {
  Future<CachedConversationList?> cachedConversations(
    String userId,
    String worldId,
  );
  Future<ConversationPage> fetchConversations(
    String userId,
    String worldId, {
    bool Function()? isCurrent,
  });
  Future<ConversationSummary> openDirect(
    String worldId,
    String characterId,
    String key,
  );
  Future<CachedMessageHistory?> cachedMessages(
    String userId,
    String worldId,
    String conversationId,
  );
  Future<MessagePage> fetchMessages(
    String userId,
    String worldId,
    String conversationId, {
    String? cursor,
    bool Function()? isCurrent,
  });
  Future<SendMessageResponse> send(
    String userId,
    String worldId,
    String conversationId,
    ConversationMessage pending, {
    bool Function()? isCurrent,
  });
  Future<void> markRead(
    String worldId,
    String conversationId,
    String messageId,
  );
}
