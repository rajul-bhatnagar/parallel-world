import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/feed/application/feed_contracts.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';

class ApiCachedFeedRepository implements FeedRepository {
  ApiCachedFeedRepository(this._gateway, this._cache);

  final FeedGateway _gateway;
  final FeedCache _cache;

  @override
  Future<CachedFeed?> readCachedFeed(String userId, String worldId) =>
      _cache.read(userId, worldId);

  @override
  Future<FeedPage> fetchFeed({
    required String userId,
    required String worldId,
    required FeedOperationIsCurrent isCurrent,
    int limit = 20,
    String? cursor,
  }) async {
    final page = await _gateway.getFeed(
      worldId: worldId,
      limit: limit,
      cursor: cursor,
    );
    if (isCurrent()) {
      if (cursor == null) {
        await _cache.replaceFirstPage(userId, worldId, page);
      } else {
        await _cache.appendPage(userId, worldId, page);
      }
    }
    return page;
  }

  @override
  Future<FeedPost> createPost({
    required String userId,
    required String worldId,
    required FeedPost pendingPost,
    required FeedOperationIsCurrent isCurrent,
  }) async {
    final clientPostId = pendingPost.clientPostId;
    final idempotencyKey = pendingPost.idempotencyKey;
    if (clientPostId == null || idempotencyKey == null) {
      throw const FormatException('A pending post operation is incomplete.');
    }
    if (!isCurrent()) {
      throw const FeedOperationCancelled();
    }
    await _cache.putPending(userId, worldId, pendingPost);
    if (!isCurrent()) {
      throw const FeedOperationCancelled();
    }
    try {
      final serverPost = await _gateway.createPost(
        worldId: worldId,
        content: pendingPost.content,
        clientPostId: clientPostId,
        idempotencyKey: idempotencyKey,
      );
      if (isCurrent()) {
        await _cache.reconcile(userId, worldId, clientPostId, serverPost);
      }
      return serverPost;
    } on AppFailure catch (failure) {
      if (isCurrent()) {
        await _cache.markFailed(userId, worldId, clientPostId, failure.message);
      }
      rethrow;
    } on FormatException {
      if (isCurrent()) {
        await _cache.markFailed(
          userId,
          worldId,
          clientPostId,
          const UnknownFailure().message,
        );
      }
      rethrow;
    }
  }

  @override
  Future<FeedPost> getPost({required String worldId, required String postId}) =>
      _gateway.getPost(worldId: worldId, postId: postId);

  @override
  Future<FeedPage> getReplies({
    required String worldId,
    required String parentPostId,
    int limit = 20,
    String? cursor,
  }) => _gateway.getReplies(
    worldId: worldId,
    parentPostId: parentPostId,
    limit: limit,
    cursor: cursor,
  );

  @override
  Future<FeedPost> createReply({
    required String worldId,
    required String parentPostId,
    required String content,
    required String clientPostId,
    required String idempotencyKey,
  }) => _gateway.createReply(
    worldId: worldId,
    parentPostId: parentPostId,
    content: content,
    clientPostId: clientPostId,
    idempotencyKey: idempotencyKey,
  );

  @override
  Future<ReactionState> setLike({
    required String userId,
    required String worldId,
    required FeedPost post,
    required bool active,
    required FeedOperationIsCurrent isCurrent,
  }) async {
    if (!isCurrent()) {
      throw const FeedOperationCancelled();
    }
    final ReactionState state;
    final FeedPost serverPost;
    if (active) {
      state = await _gateway.setLike(worldId: worldId, postId: post.id);
      serverPost = post.copyWith(
        counts: FeedCounts(
          likes: state.likeCount,
          replies: post.counts.replies,
        ),
        currentPlayerReaction: state.active ? 'like' : null,
      );
    } else {
      await _gateway.removeLike(worldId: worldId, postId: post.id);
      if (!isCurrent()) {
        throw const FeedOperationCancelled();
      }
      serverPost = await _gateway.getPost(worldId: worldId, postId: post.id);
      state = ReactionState(
        postId: post.id,
        type: 'like',
        active: serverPost.currentPlayerReaction == 'like',
        likeCount: serverPost.counts.likes,
      );
    }
    if (isCurrent()) {
      await _cache.putServerPost(userId, worldId, serverPost);
    }
    return state;
  }

  @override
  Future<void> setFollow({
    required String worldId,
    required String actorId,
    required bool active,
  }) async {
    if (active) {
      await _gateway.follow(worldId: worldId, actorId: actorId);
    } else {
      await _gateway.unfollow(worldId: worldId, actorId: actorId);
    }
  }
}
