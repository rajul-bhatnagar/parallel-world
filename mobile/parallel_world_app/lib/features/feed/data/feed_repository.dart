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
}
