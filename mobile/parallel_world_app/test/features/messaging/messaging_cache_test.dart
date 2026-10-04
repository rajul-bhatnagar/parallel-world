import 'package:drift/native.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/messaging/data/messaging_cache.dart';
import 'package:parallel_world_app/features/messaging/domain/messaging_models.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';

import '../../support/fakes.dart';

void main() {
  late AppDatabase database;
  late DriftMessagingCache cache;

  setUp(() async {
    database = AppDatabase(NativeDatabase.memory());
    await database.initialize();
    cache = DriftMessagingCache(database, utcNow: () => testNow);
  });

  tearDown(() => database.close());

  test('cache is isolated by user, world, and conversation', () async {
    await cache.replaceMessages(
      'user-a',
      testWorld.id,
      _conversationId,
      MessagePage([_message], 'next', true),
    );

    final value = await cache.readMessages(
      'user-a',
      testWorld.id,
      _conversationId,
    );
    expect(value!.items.single.body, 'Hello Maya');
    expect(value.nextCursor, 'next');
    expect(
      await cache.readMessages('user-b', testWorld.id, _conversationId),
      isNull,
    );
    expect(
      await cache.readMessages('user-a', 'foreign-world', _conversationId),
      isNull,
    );
    expect(
      await cache.readMessages('user-a', testWorld.id, 'other-chat'),
      isNull,
    );
  });

  test(
    'empty conversation list remains an offline-readable cache result',
    () async {
      await cache.replaceConversations('user-a', testWorld.id, []);

      final value = await cache.readConversations('user-a', testWorld.id);

      expect(value, isNotNull);
      expect(value!.items, isEmpty);
      expect(await cache.readConversations('user-b', testWorld.id), isNull);
    },
  );

  test('synced response reconciles the pending operation identity', () async {
    final pending = ConversationMessage(
      id: _clientMessageId,
      conversationId: _conversationId,
      senderActorId: 'player',
      senderType: 'player',
      body: 'Hello Maya',
      createdAtUtc: _createdAt,
      deliveryStatus: 'pending',
      clientMessageId: _clientMessageId,
      localState: MessageLocalState.pending,
    );
    await cache.putMessage('user-a', testWorld.id, pending);
    await cache.putMessage('user-a', testWorld.id, _message);
    await cache.appendMessages(
      'user-a',
      testWorld.id,
      _conversationId,
      const MessagePage([], null, false),
    );

    final items = (await cache.readMessages(
      'user-a',
      testWorld.id,
      _conversationId,
    ))!.items;
    expect(items, hasLength(1));
    expect(items.single.localState, MessageLocalState.synced);
    expect(items.single.id, _serverMessageId);
  });

  test('private-data clear removes M11 messages', () async {
    await cache.replaceMessages(
      'user-a',
      testWorld.id,
      _conversationId,
      MessagePage([_message], null, false),
    );
    await database.clearPrivateData();
    expect(
      await cache.readMessages('user-a', testWorld.id, _conversationId),
      isNull,
    );
  });
}

const _conversationId = '00000000-0000-0000-0000-000000000501';
const _clientMessageId = '00000000-0000-0000-0000-000000000502';
const _serverMessageId = '00000000-0000-0000-0000-000000000503';
final _createdAt = DateTime.utc(2026, 9, 2, 12);
final _message = ConversationMessage(
  id: _serverMessageId,
  conversationId: _conversationId,
  senderActorId: 'player',
  senderType: 'player',
  body: 'Hello Maya',
  createdAtUtc: _createdAt,
  deliveryStatus: 'delivered',
  clientMessageId: _clientMessageId,
);
