import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'dart:async';

import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/feed/application/feed_dependencies.dart';
import 'package:parallel_world_app/features/feed/application/post_thread_controller.dart';
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

class _MutableThreadSessionController extends SessionController {
  @override
  SessionState build() => SessionState(
    phase: SessionPhase.authenticated,
    userId: 'user-a',
    world: testWorld,
  );

  void replaceSession() {
    state = SessionState(
      phase: SessionPhase.authenticated,
      userId: 'user-b',
      world: testWorld,
    );
  }
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
  test(
    'loads root separately and paginates one parent scope in server order',
    () async {
      final firstReply = FeedPost(
        id: 'reply-1',
        worldId: testWorld.id,
        author: testFeedAuthor,
        content: 'First reply',
        createdAtUtc: testNow.add(const Duration(minutes: 1)),
        parentPostId: testFeedPost.id,
        counts: const FeedCounts(likes: 0, replies: 0),
        visibility: 'world',
      );
      final secondReply = FeedPost(
        id: 'reply-2',
        worldId: testWorld.id,
        author: testFeedAuthor,
        content: 'Second reply',
        createdAtUtc: testNow.add(const Duration(minutes: 2)),
        parentPostId: testFeedPost.id,
        counts: const FeedCounts(likes: 0, replies: 0),
        visibility: 'world',
      );
      final repository = FakeFeedRepository()
        ..replyPage = FeedPage(
          items: [firstReply],
          nextCursor: 'parent-cursor',
          hasMore: true,
        );
      final container = _container(repository);
      addTearDown(container.dispose);
      final provider = postThreadControllerProvider(testFeedPost.id);

      await container.read(provider.notifier).load();
      repository.replyPage = FeedPage(
        items: [secondReply],
        nextCursor: null,
        hasMore: false,
      );
      await container.read(provider.notifier).loadNextPage();

      final state = container.read(provider);
      expect(state.root!.id, testFeedPost.id);
      expect(state.replies.map((reply) => reply.id), ['reply-1', 'reply-2']);
      expect(repository.lastReplyParentId, testFeedPost.id);
      expect(repository.lastReplyCursor, 'parent-cursor');
    },
  );

  test('failed optimistic reply retains its operation for retry', () async {
    final repository = FakeFeedRepository()
      ..replyCreateError = const NetworkFailure();
    final container = _container(repository);
    addTearDown(container.dispose);
    final provider = postThreadControllerProvider(testFeedPost.id);
    final controller = container.read(provider.notifier);
    await controller.load();

    await controller.submitReply('Player reply');
    final failed = container.read(provider).replies.single;
    expect(failed.localState, FeedPostLocalState.failed);
    final clientPostId = failed.clientPostId!;
    final idempotencyKey = failed.idempotencyKey;

    repository.replyCreateError = null;
    await controller.retryReply(clientPostId);
    final retried = container.read(provider).replies.single;
    expect(retried.localState, FeedPostLocalState.synced);
    expect(retried.id, clientPostId);
    expect(failed.idempotencyKey, idempotencyKey);
  });

  test(
    'old-session thread completion cannot repopulate replacement state',
    () async {
      final postCompleter = Completer<FeedPost>();
      final repliesCompleter = Completer<FeedPage>();
      final repository = FakeFeedRepository()
        ..pendingGetPost = postCompleter.future
        ..pendingReplies = repliesCompleter.future;
      final container = ProviderContainer(
        overrides: [
          sessionControllerProvider.overrideWith(
            _MutableThreadSessionController.new,
          ),
          feedRepositoryProvider.overrideWithValue(repository),
        ],
      );
      addTearDown(container.dispose);
      final provider = postThreadControllerProvider(testFeedPost.id);
      final load = container.read(provider.notifier).load();

      (container.read(
        sessionControllerProvider.notifier,
      ) as _MutableThreadSessionController).replaceSession();
      postCompleter.complete(testFeedPost);
      repliesCompleter.complete(
        const FeedPage(items: [], nextCursor: null, hasMore: false),
      );
      await load;

      expect(container.read(provider).root, isNull);
      expect(container.read(provider).replies, isEmpty);
    },
  );
}
