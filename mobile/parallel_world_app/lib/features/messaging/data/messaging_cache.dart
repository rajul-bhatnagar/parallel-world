import 'package:drift/drift.dart';
import 'package:parallel_world_app/features/messaging/application/messaging_contracts.dart';
import 'package:parallel_world_app/features/messaging/domain/messaging_models.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';

class DriftMessagingCache implements MessagingCache {
  DriftMessagingCache(this.db, {DateTime Function()? utcNow})
    : now = utcNow ?? (() => DateTime.now().toUtc());

  final AppDatabase db;
  final DateTime Function() now;

  @override
  Future<CachedConversationList?> readConversations(
    String userId,
    String worldId,
  ) async {
    final metadata = await db
        .customSelect(
          'SELECT cached_at_utc FROM cached_m11_conversation_list_metadata WHERE user_id=? AND world_id=?',
          variables: [Variable(userId), Variable(worldId)],
        )
        .getSingleOrNull();
    if (metadata == null) return null;
    final rows = await db
        .customSelect(
          'SELECT * FROM cached_m11_conversations WHERE user_id=? AND world_id=? ORDER BY last_message_at_utc DESC, conversation_id DESC',
          variables: [Variable(userId), Variable(worldId)],
        )
        .get();
    final items = rows
        .map(
          (row) => ConversationSummary(
            id: row.read<String>('conversation_id'),
            character: ConversationCharacter(
              id: row.read<String>('character_id'),
              actorId: row.read<String>('character_actor_id'),
              displayName: row.read<String>('character_display_name'),
              handle: row.read<String>('character_handle'),
            ),
            createdAtUtc: _date(row.read<int>('created_at_utc')),
            lastMessageAtUtc: _date(row.read<int>('last_message_at_utc')),
            lastMessagePreview: row.readNullable<String>(
              'last_message_preview',
            ),
            unreadCount: row.read<int>('unread_count'),
            characterReplyStatus: row.readNullable<String>(
              'character_reply_status',
            ),
          ),
        )
        .toList();
    return CachedConversationList(
      items,
      _date(metadata.read<int>('cached_at_utc')),
    );
  }

  @override
  Future<void> replaceConversations(
    String userId,
    String worldId,
    List<ConversationSummary> items,
  ) => db.transaction(() async {
    await db.customStatement(
      'DELETE FROM cached_m11_conversations WHERE user_id=? AND world_id=?',
      [userId, worldId],
    );
    for (final item in items) {
      await db.customStatement(
        'INSERT INTO cached_m11_conversations VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?)',
        [
          userId,
          worldId,
          item.id,
          item.character.id,
          item.character.actorId,
          item.character.displayName,
          item.character.handle,
          _ms(item.createdAtUtc),
          _ms(item.lastMessageAtUtc),
          item.lastMessagePreview,
          item.unreadCount,
          item.characterReplyStatus,
          _ms(now()),
        ],
      );
    }
    await db.customStatement(
      'INSERT INTO cached_m11_conversation_list_metadata VALUES (?,?,?) ON CONFLICT(user_id,world_id) DO UPDATE SET cached_at_utc=excluded.cached_at_utc',
      [userId, worldId, _ms(now())],
    );
  });

  @override
  Future<CachedMessageHistory?> readMessages(
    String userId,
    String worldId,
    String conversationId,
  ) async {
    final metadata = await db
        .customSelect(
          'SELECT * FROM cached_m11_conversation_metadata WHERE user_id=? AND world_id=? AND conversation_id=?',
          variables: [
            Variable(userId),
            Variable(worldId),
            Variable(conversationId),
          ],
        )
        .getSingleOrNull();
    if (metadata == null) return null;
    final rows = await db
        .customSelect(
          'SELECT * FROM cached_m11_messages WHERE user_id=? AND world_id=? AND conversation_id=? ORDER BY created_at_utc DESC, message_id DESC',
          variables: [
            Variable(userId),
            Variable(worldId),
            Variable(conversationId),
          ],
        )
        .get();
    final items = rows
        .map(
          (row) => ConversationMessage(
            id: row.read<String>('message_id'),
            conversationId: conversationId,
            senderActorId: row.read<String>('sender_actor_id'),
            senderType: row.read<String>('sender_type'),
            body: row.read<String>('body'),
            createdAtUtc: _date(row.read<int>('created_at_utc')),
            deliveryStatus: row.read<String>('delivery_status'),
            clientMessageId: row.readNullable<String>('client_message_id'),
            localState: MessageLocalState.values.byName(
              row.read<String>('local_state'),
            ),
            failureMessage: row.readNullable<String>('failure_message'),
          ),
        )
        .toList();
    return CachedMessageHistory(
      items,
      metadata.readNullable<String>('next_cursor'),
      metadata.read<int>('has_more') == 1,
      _date(metadata.read<int>('cached_at_utc')),
    );
  }

  @override
  Future<void> replaceMessages(
    String userId,
    String worldId,
    String conversationId,
    MessagePage page,
  ) => db.transaction(() async {
    await db.customStatement(
      "DELETE FROM cached_m11_messages WHERE user_id=? AND world_id=? AND conversation_id=? AND local_state='synced'",
      [userId, worldId, conversationId],
    );
    await _write(userId, worldId, page.items);
    await _writeMetadata(userId, worldId, conversationId, page);
  });

  @override
  Future<void> appendMessages(
    String userId,
    String worldId,
    String conversationId,
    MessagePage page,
  ) => db.transaction(() async {
    await _write(userId, worldId, page.items);
    await _writeMetadata(userId, worldId, conversationId, page);
  });

  @override
  Future<void> putMessage(
    String userId,
    String worldId,
    ConversationMessage message,
  ) => db.transaction(() => _putMessage(userId, worldId, message));

  Future<void> _write(
    String userId,
    String worldId,
    List<ConversationMessage> messages,
  ) async {
    for (final message in messages) {
      await _putMessage(userId, worldId, message);
    }
  }

  Future<void> _putMessage(
    String userId,
    String worldId,
    ConversationMessage message,
  ) async {
    if (message.localState == MessageLocalState.synced &&
        message.clientMessageId != null) {
      await db.customStatement(
        'DELETE FROM cached_m11_messages WHERE user_id=? AND world_id=? AND conversation_id=? AND client_message_id=?',
        [userId, worldId, message.conversationId, message.clientMessageId],
      );
    }
    await db.customStatement(
      'INSERT INTO cached_m11_messages VALUES (?,?,?,?,?,?,?,?,?,?,?,?) ON CONFLICT(user_id,world_id,conversation_id,message_id) DO UPDATE SET sender_actor_id=excluded.sender_actor_id,sender_type=excluded.sender_type,body=excluded.body,created_at_utc=excluded.created_at_utc,delivery_status=excluded.delivery_status,client_message_id=excluded.client_message_id,local_state=excluded.local_state,failure_message=excluded.failure_message',
      [
        userId,
        worldId,
        message.conversationId,
        message.id,
        message.senderActorId,
        message.senderType,
        message.body,
        _ms(message.createdAtUtc),
        message.deliveryStatus,
        message.clientMessageId,
        message.localState.name,
        message.failureMessage,
      ],
    );
  }

  Future<void> _writeMetadata(
    String userId,
    String worldId,
    String conversationId,
    MessagePage page,
  ) => db.customStatement(
    'INSERT INTO cached_m11_conversation_metadata VALUES (?,?,?,?,?,?) ON CONFLICT(user_id,world_id,conversation_id) DO UPDATE SET next_cursor=excluded.next_cursor,has_more=excluded.has_more,cached_at_utc=excluded.cached_at_utc',
    [
      userId,
      worldId,
      conversationId,
      page.nextCursor,
      page.hasMore ? 1 : 0,
      _ms(now()),
    ],
  );

  static int _ms(DateTime value) => value.toUtc().millisecondsSinceEpoch;
  static DateTime _date(int value) =>
      DateTime.fromMillisecondsSinceEpoch(value, isUtc: true);
}
