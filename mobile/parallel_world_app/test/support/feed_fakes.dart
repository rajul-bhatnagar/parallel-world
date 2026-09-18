import 'package:parallel_world_app/features/feed/application/feed_contracts.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';

import 'fakes.dart';

const testFeedAuthor = FeedAuthor(
  actorId: '00000000-0000-0000-0000-000000000202',
  displayName: 'Maya Chen',
  handle: 'maya',
  actorType: 'character',
);

final testFeedPost = FeedPost(
  id: '00000000-0000-0000-0000-000000000301',
  worldId: testWorld.id,
  author: testFeedAuthor,
  content: 'A quiet start, and plenty to notice.',
  createdAtUtc: testNow,
  counts: const FeedCounts(likes: 0, replies: 0),
  visibility: 'world',
);

class FakeFeedRepository implements FeedRepository {
  CachedFeed? cached;
  FeedPage page = FeedPage(
    items: [testFeedPost],
    nextCursor: null,
    hasMore: false,
  );
  Object? fetchError;
  Object? createError;
  Future<FeedPage>? pendingPage;
  Future<FeedPost>? pendingCreate;
  FeedPost? lastPending;
  String? lastCursor;
  int fetchCalls = 0;
  int createCalls = 0;
  Object? reactionError;
  Object? followError;
  bool? lastLikeActive;
  bool? lastFollowActive;
  FeedPage replyPage = const FeedPage(
    items: [],
    nextCursor: null,
    hasMore: false,
  );
  Object? replyCreateError;
  String? lastReplyCursor;
  String? lastReplyParentId;
  Future<FeedPost>? pendingGetPost;
  Future<FeedPage>? pendingReplies;

  @override
  Future<FeedPost> getPost({
    required String worldId,
    required String postId,
  }) async =>
      pendingGetPost ?? page.items.firstWhere((post) => post.id == postId);

  @override
  Future<FeedPage> getReplies({
    required String worldId,
    required String parentPostId,
    int limit = 20,
    String? cursor,
  }) async {
    lastReplyParentId = parentPostId;
    lastReplyCursor = cursor;
    return pendingReplies ?? replyPage;
  }

  @override
  Future<FeedPost> createReply({
    required String worldId,
    required String parentPostId,
    required String content,
    required String clientPostId,
    required String idempotencyKey,
  }) async {
    if (replyCreateError case final error?) {
      throw error;
    }
    return FeedPost(
      id: clientPostId,
      worldId: worldId,
      author: FeedAuthor(
        actorId: testWorld.playerActorId,
        displayName: testWorld.playerDisplayName,
        handle: 'player',
        actorType: 'player',
      ),
      content: content,
      createdAtUtc: testNow,
      parentPostId: parentPostId,
      counts: const FeedCounts(likes: 0, replies: 0),
      visibility: 'world',
    );
  }

  @override
  Future<ReactionState> setLike({
    required String userId,
    required String worldId,
    required FeedPost post,
    required bool active,
    required FeedOperationIsCurrent isCurrent,
  }) async {
    lastLikeActive = active;
    if (reactionError case final error?) {
      throw error;
    }
    return ReactionState(
      postId: post.id,
      type: 'like',
      active: active,
      likeCount: active ? post.counts.likes + 1 : post.counts.likes - 1,
    );
  }

  @override
  Future<void> setFollow({
    required String worldId,
    required String actorId,
    required bool active,
  }) async {
    lastFollowActive = active;
    if (followError case final error?) {
      throw error;
    }
  }

  @override
  Future<CachedFeed?> readCachedFeed(String userId, String worldId) async =>
      cached;

  @override
  Future<FeedPage> fetchFeed({
    required String userId,
    required String worldId,
    required FeedOperationIsCurrent isCurrent,
    int limit = 20,
    String? cursor,
  }) async {
    fetchCalls++;
    lastCursor = cursor;
    if (fetchError case final error?) {
      throw error;
    }
    return pendingPage ?? page;
  }

  @override
  Future<FeedPost> createPost({
    required String userId,
    required String worldId,
    required FeedPost pendingPost,
    required FeedOperationIsCurrent isCurrent,
  }) async {
    createCalls++;
    lastPending = pendingPost;
    if (createError case final error?) {
      throw error;
    }
    if (pendingCreate case final pending?) {
      return pending;
    }
    return FeedPost(
      id: '00000000-0000-0000-0000-000000000399',
      worldId: worldId,
      author: FeedAuthor(
        actorId: testWorld.playerActorId,
        displayName: testWorld.playerDisplayName,
        handle: 'player',
        actorType: 'player',
      ),
      content: pendingPost.content,
      createdAtUtc: testNow.add(const Duration(minutes: 1)),
      counts: const FeedCounts(likes: 0, replies: 0),
      visibility: 'world',
    );
  }
}
