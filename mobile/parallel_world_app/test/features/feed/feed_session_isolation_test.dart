import 'dart:async';

import 'package:drift/native.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/feed/application/feed_contracts.dart';
import 'package:parallel_world_app/features/feed/application/feed_controller.dart';
import 'package:parallel_world_app/features/feed/application/feed_dependencies.dart';
import 'package:parallel_world_app/features/feed/data/feed_cache.dart';
import 'package:parallel_world_app/features/feed/data/feed_repository.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_dependencies.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';
import 'package:parallel_world_app/features/world/application/world_contracts.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';
import 'package:parallel_world_app/features/world/domain/world_summary.dart';

import '../../support/fakes.dart';
import '../../support/feed_fakes.dart';

class _MutableSessionController extends SessionController {
  @override
  SessionState build() => SessionState(
    phase: SessionPhase.authenticated,
    userId: 'user-a',
    world: testWorld,
  );

  void authenticate(String userId, WorldSummary world) {
    state = SessionState(
      phase: SessionPhase.authenticated,
      userId: userId,
      world: world,
    );
  }
}

class _DelayedFeedGateway implements FeedGateway {
  final feedRequests = <Completer<FeedPage>>[];
  final postRequests = <Completer<FeedPost>>[];

  @override
  Future<FeedPage> getFeed({
    required String worldId,
    int limit = 20,
    String? cursor,
  }) {
    final request = Completer<FeedPage>();
    feedRequests.add(request);
    return request.future;
  }

  @override
  Future<FeedPost> createPost({
    required String worldId,
    required String content,
    required String clientPostId,
    required String idempotencyKey,
  }) {
    final request = Completer<FeedPost>();
    postRequests.add(request);
    return request.future;
  }

  @override
  Future<FeedPost> getPost({required String worldId, required String postId}) =>
      throw UnimplementedError();

  @override
  Future<FeedPage> getReplies({
    required String worldId,
    required String parentPostId,
    int limit = 20,
    String? cursor,
  }) => throw UnimplementedError();

  @override
  Future<FeedPost> createReply({
    required String worldId,
    required String parentPostId,
    required String content,
    required String clientPostId,
    required String idempotencyKey,
  }) => throw UnimplementedError();

  @override
  Future<ReactionState> setLike({
    required String worldId,
    required String postId,
  }) => throw UnimplementedError();

  @override
  Future<void> removeLike({required String worldId, required String postId}) =>
      throw UnimplementedError();

  @override
  Future<FollowState> follow({
    required String worldId,
    required String actorId,
  }) => throw UnimplementedError();

  @override
  Future<void> unfollow({required String worldId, required String actorId}) =>
      throw UnimplementedError();
}

class _DatabaseWorldCache implements WorldCache {
  _DatabaseWorldCache(this.database);

  final AppDatabase database;

  @override
  Future<void> clear() => database.clearPrivateData();

  @override
  Future<void> initialize() async {}

  @override
  Future<WorldSummary?> read(String userId) async => null;

  @override
  Future<void> write(String userId, WorldSummary world) async {}
}

void main() {
  late AppDatabase database;
  late DriftFeedCache cache;
  late _DelayedFeedGateway gateway;
  late ProviderContainer container;
  late ProviderSubscription<FeedState> subscription;

  setUp(() {
    database = AppDatabase(NativeDatabase.memory());
    cache = DriftFeedCache(database, utcNow: () => testNow);
    gateway = _DelayedFeedGateway();
    container = ProviderContainer(
      overrides: [
        sessionControllerProvider.overrideWith(_MutableSessionController.new),
        feedRepositoryProvider.overrideWithValue(
          ApiCachedFeedRepository(gateway, cache),
        ),
        sessionLifecycleProvider.overrideWithValue(FakeSessionLifecycle()),
        worldCacheProvider.overrideWithValue(_DatabaseWorldCache(database)),
      ],
    );
    subscription = container.listen(
      feedControllerProvider,
      (_, _) {},
      fireImmediately: true,
    );
  });

  tearDown(() async {
    subscription.close();
    container.dispose();
    await database.close();
  });

  test('logout during feed load rejects late state and cache writes', () async {
    final load = container.read(feedControllerProvider.notifier).load();
    await _waitFor(() => gateway.feedRequests.length == 1);

    await container.read(sessionControllerProvider.notifier).logout();
    gateway.feedRequests.single.complete(
      FeedPage(items: [testFeedPost], nextCursor: null, hasMore: false),
    );
    await load;

    expect(container.read(feedControllerProvider).items, isEmpty);
    expect(await cache.read('user-a', testWorld.id), isNull);
  });

  test('logout during post removes optimistic state and late result', () async {
    final submit = container
        .read(feedControllerProvider.notifier)
        .submit('A pending player post');
    await _waitFor(() => gateway.postRequests.length == 1);
    expect(container.read(feedControllerProvider).items, hasLength(1));
    expect((await cache.read('user-a', testWorld.id))!.items, hasLength(1));

    await container.read(sessionControllerProvider.notifier).logout();
    gateway.postRequests.single.complete(_playerPost(testWorld));
    await submit;

    expect(container.read(feedControllerProvider).items, isEmpty);
    expect(await cache.read('user-a', testWorld.id), isNull);
  });

  test('next session never exposes or persists the prior user feed', () async {
    final firstLoad = container.read(feedControllerProvider.notifier).load();
    await _waitFor(() => gateway.feedRequests.length == 1);
    gateway.feedRequests[0].complete(
      FeedPage(items: [testFeedPost], nextCursor: null, hasMore: false),
    );
    await firstLoad;
    expect(
      container.read(feedControllerProvider).items.single.id,
      testFeedPost.id,
    );

    final session = _sessionController(container);
    await session.logout();
    session.authenticate('user-b', _secondWorld);

    expect(container.read(feedControllerProvider).items, isEmpty);
    final secondLoad = container.read(feedControllerProvider.notifier).load();
    await _waitFor(() => gateway.feedRequests.length == 2);
    expect(container.read(feedControllerProvider).items, isEmpty);

    final secondPost = _characterPost(_secondWorld);
    gateway.feedRequests[1].complete(
      FeedPage(items: [secondPost], nextCursor: null, hasMore: false),
    );
    await secondLoad;

    expect(
      container.read(feedControllerProvider).items.single.id,
      secondPost.id,
    );
    expect(await cache.read('user-a', testWorld.id), isNull);
    expect(
      (await cache.read('user-b', _secondWorld.id))!.items.single.id,
      secondPost.id,
    );
  });

  test('world switch creates isolated state and cache scope', () async {
    final firstLoad = container.read(feedControllerProvider.notifier).load();
    await _waitFor(() => gateway.feedRequests.length == 1);
    gateway.feedRequests[0].complete(
      FeedPage(items: [testFeedPost], nextCursor: null, hasMore: false),
    );
    await firstLoad;

    _sessionController(container).authenticate('user-a', _secondWorld);
    expect(container.read(feedControllerProvider).items, isEmpty);

    final secondLoad = container.read(feedControllerProvider.notifier).load();
    await _waitFor(() => gateway.feedRequests.length == 2);
    final secondPost = _characterPost(_secondWorld);
    gateway.feedRequests[1].complete(
      FeedPage(items: [secondPost], nextCursor: null, hasMore: false),
    );
    await secondLoad;

    expect(
      container.read(feedControllerProvider).items.single.worldId,
      _secondWorld.id,
    );
    expect(
      (await cache.read('user-a', testWorld.id))!.items.single.worldId,
      testWorld.id,
    );
    expect(
      (await cache.read('user-a', _secondWorld.id))!.items.single.worldId,
      _secondWorld.id,
    );
  });
}

Future<void> _waitFor(bool Function() condition) async {
  for (var attempt = 0; attempt < 100 && !condition(); attempt++) {
    await Future<void>.delayed(Duration.zero);
  }
  expect(condition(), isTrue);
}

_MutableSessionController _sessionController(ProviderContainer container) =>
    container.read(sessionControllerProvider.notifier)
        as _MutableSessionController;

final _secondWorld = WorldSummary(
  id: '00000000-0000-0000-0000-000000000401',
  name: 'Second Parallel World',
  status: 'Active',
  currentGameTimeUtc: testNow,
  playerActorId: '00000000-0000-0000-0000-000000000402',
  playerDisplayName: 'Second Player',
  createdAtUtc: testNow,
);

FeedPost _characterPost(WorldSummary world) => FeedPost(
  id: '00000000-0000-0000-0000-000000000403',
  worldId: world.id,
  author: const FeedAuthor(
    actorId: '00000000-0000-0000-0000-000000000404',
    displayName: 'Second Character',
    handle: 'second',
    actorType: 'character',
  ),
  content: 'A different private feed.',
  createdAtUtc: testNow,
  counts: const FeedCounts(likes: 0, replies: 0),
  visibility: 'world',
);

FeedPost _playerPost(WorldSummary world) => FeedPost(
  id: '00000000-0000-0000-0000-000000000405',
  worldId: world.id,
  author: FeedAuthor(
    actorId: world.playerActorId,
    displayName: world.playerDisplayName,
    handle: 'player',
    actorType: 'player',
  ),
  content: 'A pending player post',
  createdAtUtc: testNow,
  counts: const FeedCounts(likes: 0, replies: 0),
  visibility: 'world',
);
