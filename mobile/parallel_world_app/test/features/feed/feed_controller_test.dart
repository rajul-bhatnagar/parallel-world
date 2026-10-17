import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/feed/application/feed_contracts.dart';
import 'package:parallel_world_app/features/feed/application/feed_controller.dart';
import 'package:parallel_world_app/features/feed/application/feed_dependencies.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

import '../../support/fakes.dart';
import '../../support/feed_fakes.dart';

class _AuthenticatedSessionController extends SessionController {
  @override
  SessionState build() => SessionState(
    phase: SessionPhase.authenticated,
    userId: 'user-a',
    world: testWorld,
  );
}

ProviderContainer _container(FakeFeedRepository repository) =>
    ProviderContainer(
      overrides: [
        sessionControllerProvider.overrideWith(
          _AuthenticatedSessionController.new,
        ),
        feedRepositoryProvider.overrideWithValue(repository),
      ],
    );

void main() {
  test('cached feed stays visible and is marked stale when offline', () async {
    final repository = FakeFeedRepository()
      ..cached = CachedFeed(
        items: [testFeedPost],
        cachedAtUtc: testNow,
        nextCursor: null,
        hasMore: false,
      )
      ..fetchError = const NetworkFailure();
    final container = _container(repository);
    addTearDown(container.dispose);

    await container.read(feedControllerProvider.notifier).load();

    final state = container.read(feedControllerProvider);
    expect(state.items.single.id, testFeedPost.id);
    expect(state.isOffline, isTrue);
    expect(state.message, contains('may be out of date'));
  });

  test('next page retains server order and deduplicates post ids', () async {
    final repository = FakeFeedRepository()
      ..page = FeedPage(
        items: [testFeedPost],
        nextCursor: 'opaque-next',
        hasMore: true,
      );
    final container = _container(repository);
    addTearDown(container.dispose);
    await container.read(feedControllerProvider.notifier).load();
    final second = FeedPost(
      id: '00000000-0000-0000-0000-000000000302',
      worldId: testWorld.id,
      author: testFeedAuthor,
      content: 'Older post',
      createdAtUtc: testNow.subtract(const Duration(minutes: 1)),
      counts: const FeedCounts(likes: 0, replies: 0),
      visibility: 'world',
    );
    repository.page = FeedPage(
      items: [testFeedPost, second],
      nextCursor: null,
      hasMore: false,
    );

    await container.read(feedControllerProvider.notifier).loadNextPage();

    expect(repository.lastCursor, 'opaque-next');
    expect(container.read(feedControllerProvider).items, [
      testFeedPost,
      second,
    ]);
  });

  test(
    'failed optimistic post retries with the same operation identity',
    () async {
      final repository = FakeFeedRepository()
        ..createError = const NetworkFailure();
      final container = _container(repository);
      addTearDown(container.dispose);
      await container.read(feedControllerProvider.notifier).load();

      await container.read(feedControllerProvider.notifier).submit('Hello');
      final failed = container
          .read(feedControllerProvider)
          .items
          .firstWhere((post) => post.localState == FeedPostLocalState.failed);
      final clientPostId = failed.clientPostId!;
      final idempotencyKey = failed.idempotencyKey;
      expect(failed.author.actorId, testWorld.playerActorId);

      repository.createError = null;
      await container.read(feedControllerProvider.notifier).retry(clientPostId);

      expect(repository.lastPending!.clientPostId, clientPostId);
      expect(repository.lastPending!.idempotencyKey, idempotencyKey);
      expect(
        container.read(feedControllerProvider).items.first.localState,
        FeedPostLocalState.synced,
      );
    },
  );

  test('missing world does not call the feed repository', () async {
    final repository = FakeFeedRepository();
    final container = ProviderContainer(
      overrides: [feedRepositoryProvider.overrideWithValue(repository)],
    );
    addTearDown(container.dispose);

    await container.read(feedControllerProvider.notifier).load();

    expect(repository.fetchCalls, 0);
    expect(container.read(feedControllerProvider).message, contains('world'));
  });

  test(
    'like and follow optimistically reconcile and roll back failures',
    () async {
      final repository = FakeFeedRepository();
      final container = _container(repository);
      addTearDown(container.dispose);
      final controller = container.read(feedControllerProvider.notifier);
      await controller.load();

      await controller.toggleLike(testFeedPost.id);
      var post = container.read(feedControllerProvider).items.single;
      expect(post.currentPlayerReaction, 'like');
      expect(post.counts.likes, 1);
      expect(repository.lastLikeActive, isTrue);

      repository.reactionError = const ServerFailure();
      await controller.toggleLike(testFeedPost.id);
      post = container.read(feedControllerProvider).items.single;
      expect(post.currentPlayerReaction, 'like');
      expect(post.counts.likes, 1);

      await controller.toggleFollow(testFeedAuthor.actorId);
      post = container.read(feedControllerProvider).items.single;
      expect(post.author.isFollowed, isTrue);
      repository.followError = const ServerFailure();
      await controller.toggleFollow(testFeedAuthor.actorId);
      post = container.read(feedControllerProvider).items.single;
      expect(post.author.isFollowed, isTrue);
    },
  );
}
