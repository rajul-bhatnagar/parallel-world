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
