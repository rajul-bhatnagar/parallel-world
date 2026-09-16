import 'package:drift/drift.dart';
import 'package:parallel_world_app/features/feed/application/feed_contracts.dart'
    as contracts;
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';

class DriftFeedCache implements contracts.FeedCache {
  DriftFeedCache(this._database, {DateTime Function()? utcNow})
    : _utcNow = utcNow ?? _defaultUtcNow;

  final AppDatabase _database;
  final DateTime Function() _utcNow;

  @override
  Future<contracts.CachedFeed?> read(String userId, String worldId) async {
    final metadata =
        await (_database.select(_database.cachedFeedMetadata)..where(
              (row) => row.userId.equals(userId) & row.worldId.equals(worldId),
            ))
            .getSingleOrNull();
    if (metadata == null) {
      return null;
    }

    final rows =
        await (_database.select(_database.cachedFeedPosts)
              ..where(
                (row) =>
                    row.userId.equals(userId) & row.worldId.equals(worldId),
              )
              ..orderBy([
                (row) => OrderingTerm.desc(row.createdAtUtc),
                (row) => OrderingTerm.desc(row.postId),
              ]))
            .get();
    return contracts.CachedFeed(
      items: rows.map(_toPost).toList(growable: false),
      cachedAtUtc: metadata.cachedAtUtc.toUtc(),
      nextCursor: metadata.nextCursor,
      hasMore: metadata.hasMore,
    );
  }

  @override
  Future<void> replaceFirstPage(String userId, String worldId, FeedPage page) =>
      _database.transaction(() async {
        await (_database.delete(_database.cachedFeedPosts)..where(
              (row) =>
                  row.userId.equals(userId) &
                  row.worldId.equals(worldId) &
                  row.localState.equals(FeedPostLocalState.synced.name),
            ))
            .go();
        await _writePosts(userId, worldId, page.items);
        await _writeMetadata(userId, worldId, page);
      });

  @override
  Future<void> appendPage(String userId, String worldId, FeedPage page) =>
      _database.transaction(() async {
        await _writePosts(userId, worldId, page.items);
        await _writeMetadata(userId, worldId, page);
      });

  @override
  Future<void> putPending(String userId, String worldId, FeedPost post) =>
      _database.transaction(() async {
        await _database
            .into(_database.cachedFeedMetadata)
            .insert(
              CachedFeedMetadataCompanion.insert(
                userId: userId,
                worldId: worldId,
                cachedAtUtc: _utcNow(),
                hasMore: false,
              ),
              mode: InsertMode.insertOrIgnore,
            );
        await _database
            .into(_database.cachedFeedPosts)
            .insertOnConflictUpdate(_companion(userId, worldId, post));
      });

  @override
  Future<void> reconcile(
    String userId,
    String worldId,
    String clientPostId,
    FeedPost serverPost,
  ) => _database.transaction(() async {
    await (_database.delete(_database.cachedFeedPosts)..where(
          (row) =>
              row.userId.equals(userId) &
              row.worldId.equals(worldId) &
              row.clientPostId.equals(clientPostId),
        ))
        .go();
    await _database
        .into(_database.cachedFeedPosts)
        .insertOnConflictUpdate(_companion(userId, worldId, serverPost));
  });

  @override
  Future<void> markFailed(
    String userId,
    String worldId,
    String clientPostId,
    String message,
  ) async {
    final row =
        await (_database.select(_database.cachedFeedPosts)..where(
              (row) =>
                  row.userId.equals(userId) &
                  row.worldId.equals(worldId) &
                  row.clientPostId.equals(clientPostId),
            ))
            .getSingleOrNull();
    if (row == null) {
      return;
    }
    await _database
        .into(_database.cachedFeedPosts)
        .insertOnConflictUpdate(
          _companion(
            userId,
            worldId,
            _toPost(row).copyWith(
              localState: FeedPostLocalState.failed,
              failureMessage: message,
            ),
          ),
        );
  }

  Future<void> _writePosts(
    String userId,
    String worldId,
    Iterable<FeedPost> posts,
  ) async {
    for (final post in posts) {
      await _database
          .into(_database.cachedFeedPosts)
          .insertOnConflictUpdate(_companion(userId, worldId, post));
    }
  }

  Future<void> _writeMetadata(String userId, String worldId, FeedPage page) =>
      _database
          .into(_database.cachedFeedMetadata)
          .insertOnConflictUpdate(
            CachedFeedMetadataCompanion.insert(
              userId: userId,
              worldId: worldId,
              cachedAtUtc: _utcNow(),
              nextCursor: Value(page.nextCursor),
              hasMore: page.hasMore,
            ),
          );

  static CachedFeedPostsCompanion _companion(
    String userId,
    String worldId,
    FeedPost post,
  ) => CachedFeedPostsCompanion.insert(
    userId: userId,
    worldId: worldId,
    postId: post.id,
    authorActorId: post.author.actorId,
    authorDisplayName: post.author.displayName,
    authorHandle: post.author.handle,
    authorActorType: post.author.actorType,
    content: post.content,
    createdAtUtc: post.createdAtUtc,
    parentPostId: Value(post.parentPostId),
    likeCount: post.counts.likes,
    replyCount: post.counts.replies,
    visibility: post.visibility,
    localState: post.localState.name,
    clientPostId: Value(post.clientPostId),
    idempotencyKey: Value(post.idempotencyKey),
    failureMessage: Value(post.failureMessage),
  );

  static FeedPost _toPost(CachedFeedPost row) => FeedPost(
    id: row.postId,
    worldId: row.worldId,
    author: FeedAuthor(
      actorId: row.authorActorId,
      displayName: row.authorDisplayName,
      handle: row.authorHandle,
      actorType: row.authorActorType,
    ),
    content: row.content,
    createdAtUtc: row.createdAtUtc.toUtc(),
    parentPostId: row.parentPostId,
    counts: FeedCounts(likes: row.likeCount, replies: row.replyCount),
    visibility: row.visibility,
    localState: FeedPostLocalState.values.byName(row.localState),
    clientPostId: row.clientPostId,
    idempotencyKey: row.idempotencyKey,
    failureMessage: row.failureMessage,
  );

  static DateTime _defaultUtcNow() => DateTime.now().toUtc();
}
