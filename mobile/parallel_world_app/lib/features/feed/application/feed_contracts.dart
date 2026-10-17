import 'package:parallel_world_app/features/feed/domain/feed_models.dart';

typedef FeedOperationIsCurrent = bool Function();

class FeedOperationCancelled implements Exception {
  const FeedOperationCancelled();
}

abstract interface class FeedGateway {
  Future<FeedPage> getFeed({
    required String worldId,
    int limit = 20,
    String? cursor,
  });

  Future<FeedPost> createPost({
    required String worldId,
    required String content,
    required String clientPostId,
    required String idempotencyKey,
  });

  Future<FeedPost> getPost({required String worldId, required String postId});

  Future<FeedPage> getReplies({
    required String worldId,
    required String parentPostId,
    int limit = 20,
    String? cursor,
  });

  Future<FeedPost> createReply({
    required String worldId,
    required String parentPostId,
    required String content,
    required String clientPostId,
    required String idempotencyKey,
  });

  Future<ReactionState> setLike({
    required String worldId,
    required String postId,
  });

  Future<void> removeLike({required String worldId, required String postId});

  Future<FollowState> follow({
    required String worldId,
    required String actorId,
  });

  Future<void> unfollow({required String worldId, required String actorId});
}

class CachedFeed {
  const CachedFeed({
    required this.items,
    required this.cachedAtUtc,
    required this.nextCursor,
    required this.hasMore,
  });

  final List<FeedPost> items;
  final DateTime cachedAtUtc;
  final String? nextCursor;
  final bool hasMore;
}

abstract interface class FeedCache {
  Future<CachedFeed?> read(String userId, String worldId);

  Future<void> replaceFirstPage(String userId, String worldId, FeedPage page);

  Future<void> appendPage(String userId, String worldId, FeedPage page);

  Future<void> putPending(String userId, String worldId, FeedPost post);

  Future<void> reconcile(
    String userId,
    String worldId,
    String clientPostId,
    FeedPost serverPost,
  );

  Future<void> markFailed(
    String userId,
    String worldId,
    String clientPostId,
    String message,
  );

  Future<void> putServerPost(String userId, String worldId, FeedPost post);
}

abstract interface class FeedRepository {
  Future<CachedFeed?> readCachedFeed(String userId, String worldId);

  Future<FeedPage> fetchFeed({
    required String userId,
    required String worldId,
    required FeedOperationIsCurrent isCurrent,
    int limit = 20,
    String? cursor,
  });

  Future<FeedPost> createPost({
    required String userId,
    required String worldId,
    required FeedPost pendingPost,
    required FeedOperationIsCurrent isCurrent,
  });

  Future<FeedPost> getPost({required String worldId, required String postId});

  Future<FeedPage> getReplies({
    required String worldId,
    required String parentPostId,
    int limit = 20,
    String? cursor,
  });

  Future<FeedPost> createReply({
    required String worldId,
    required String parentPostId,
    required String content,
    required String clientPostId,
    required String idempotencyKey,
  });

  Future<ReactionState> setLike({
    required String userId,
    required String worldId,
    required FeedPost post,
    required bool active,
    required FeedOperationIsCurrent isCurrent,
  });

  Future<void> setFollow({
    required String worldId,
    required String actorId,
    required bool active,
  });
}
