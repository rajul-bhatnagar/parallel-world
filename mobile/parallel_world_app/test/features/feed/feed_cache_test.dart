import 'dart:io';

import 'package:drift/native.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/feed/data/feed_cache.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';

import '../../support/fakes.dart';
import '../../support/feed_fakes.dart';

void main() {
  late AppDatabase database;
  late DriftFeedCache cache;

  setUp(() {
    database = AppDatabase(NativeDatabase.memory());
    cache = DriftFeedCache(database, utcNow: () => testNow);
  });

  tearDown(() => database.close());

  test(
    'cache is isolated by user and world and preserves server order',
    () async {
      final newer = _post('00000000-0000-0000-0000-000000000302', 2);
      await cache.replaceFirstPage(
        'user-a',
        testWorld.id,
        FeedPage(
          items: [testFeedPost, newer],
          nextCursor: 'next',
          hasMore: true,
        ),
      );

      final value = await cache.read('user-a', testWorld.id);

      expect(value!.items.map((post) => post.id), [newer.id, testFeedPost.id]);
      expect(value.nextCursor, 'next');
      expect(await cache.read('user-b', testWorld.id), isNull);
      expect(await cache.read('user-a', 'foreign-world'), isNull);
    },
  );

  test('page append deduplicates and pending operation reconciles', () async {
    await cache.replaceFirstPage(
      'user-a',
      testWorld.id,
      FeedPage(items: [testFeedPost], nextCursor: 'next', hasMore: true),
    );
    await cache.appendPage(
      'user-a',
      testWorld.id,
      FeedPage(items: [testFeedPost], nextCursor: null, hasMore: false),
    );
    final pending = _pending();
    await cache.putPending('user-a', testWorld.id, pending);
    expect((await cache.read('user-a', testWorld.id))!.items.length, 2);

    final created = _post('00000000-0000-0000-0000-000000000399', 3);
    await cache.reconcile(
      'user-a',
      testWorld.id,
      pending.clientPostId!,
      created,
    );

    final posts = (await cache.read('user-a', testWorld.id))!.items;
    expect(posts.where((post) => post.id == created.id), hasLength(1));
    expect(posts.where((post) => post.clientPostId != null), isEmpty);
  });

  test('failed operation retains its original retry identity', () async {
    final pending = _pending();
    await cache.putPending('user-a', testWorld.id, pending);
    await cache.markFailed(
      'user-a',
      testWorld.id,
      pending.clientPostId!,
      'Try again.',
    );

    final failed = (await cache.read('user-a', testWorld.id))!.items.single;
    expect(failed.localState, FeedPostLocalState.failed);
    expect(failed.clientPostId, pending.clientPostId);
    expect(failed.idempotencyKey, pending.idempotencyKey);
    expect(failed.failureMessage, 'Try again.');
  });

  test('private-data clear removes the feed cache', () async {
    await cache.replaceFirstPage(
      'user-a',
      testWorld.id,
      FeedPage(items: [testFeedPost], nextCursor: null, hasMore: false),
    );

    await database.clearPrivateData();

    expect(await cache.read('user-a', testWorld.id), isNull);
  });

  test('cache preserves M07 reaction and follow projections', () async {
    final socialPost = FeedPost(
      id: testFeedPost.id,
      worldId: testFeedPost.worldId,
      author: testFeedAuthor.copyWith(isFollowed: true),
      content: testFeedPost.content,
      createdAtUtc: testFeedPost.createdAtUtc,
      counts: const FeedCounts(likes: 1, replies: 0),
      currentPlayerReaction: 'like',
      visibility: testFeedPost.visibility,
    );

    await cache.replaceFirstPage(
      'user-a',
      testWorld.id,
      FeedPage(items: [socialPost], nextCursor: null, hasMore: false),
    );

    final cached = (await cache.read('user-a', testWorld.id))!.items.single;
    expect(cached.author.isFollowed, isTrue);
    expect(cached.currentPlayerReaction, 'like');
    expect(cached.counts.likes, 1);
  });

  test('schema version two upgrades with M06 feed cache tables', () async {
    await database.close();
    final directory = await Directory.systemTemp.createTemp('m06_drift_');
    final file = File('${directory.path}/cache.sqlite');
    database = AppDatabase(NativeDatabase(file));
    await database.initialize();
    await database.customStatement('DROP TABLE cached_feed_metadata');
    await database.customStatement('DROP TABLE cached_feed_posts');
    await database.customStatement('PRAGMA user_version = 2');
    await database.close();
    database = AppDatabase(NativeDatabase(file));

    await database.initialize();

    final rows = await database
        .customSelect("SELECT name FROM sqlite_master WHERE type = 'table'")
        .get();
    final names = rows.map((row) => row.read<String>('name')).toSet();
    expect(names, containsAll({'cached_feed_metadata', 'cached_feed_posts'}));
    await database.close();
    await directory.delete(recursive: true);
    database = AppDatabase(NativeDatabase.memory());
  });

  test('schema version three upgrades with M07 social columns', () async {
    await database.close();
    final directory = await Directory.systemTemp.createTemp('m07_drift_');
    final file = File('${directory.path}/cache.sqlite');
    database = AppDatabase(NativeDatabase(file));
    await database.initialize();
    await database.customStatement(
      'ALTER TABLE cached_feed_posts DROP COLUMN author_is_followed',
    );
    await database.customStatement(
      'ALTER TABLE cached_feed_posts DROP COLUMN current_player_reaction',
    );
    await database.customStatement('PRAGMA user_version = 3');
    await database.close();
    database = AppDatabase(NativeDatabase(file));

    await database.initialize();

    final rows = await database
        .customSelect('PRAGMA table_info(cached_feed_posts)')
        .get();
    final columns = rows.map((row) => row.read<String>('name')).toSet();
    expect(
      columns,
      containsAll({'author_is_followed', 'current_player_reaction'}),
    );
    await database.close();
    await directory.delete(recursive: true);
    database = AppDatabase(NativeDatabase.memory());
  });
}

FeedPost _post(String id, int minutes) => FeedPost(
  id: id,
  worldId: testWorld.id,
  author: testFeedAuthor,
  content: 'Post $minutes',
  createdAtUtc: testNow.add(Duration(minutes: minutes)),
  counts: const FeedCounts(likes: 0, replies: 0),
  visibility: 'world',
);

FeedPost _pending() => FeedPost(
  id: '00000000-0000-0000-0000-000000000350',
  worldId: testWorld.id,
  author: FeedAuthor(
    actorId: testWorld.playerActorId,
    displayName: testWorld.playerDisplayName,
    handle: 'you',
    actorType: 'player',
  ),
  content: 'Pending post',
  createdAtUtc: testNow.add(const Duration(minutes: 3)),
  counts: const FeedCounts(likes: 0, replies: 0),
  visibility: 'world',
  localState: FeedPostLocalState.pending,
  clientPostId: '00000000-0000-0000-0000-000000000350',
  idempotencyKey: 'same-operation-key',
);
