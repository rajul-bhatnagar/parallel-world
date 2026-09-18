import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/feed/application/feed_contracts.dart';
import 'package:parallel_world_app/features/feed/application/feed_dependencies.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_dependencies.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

final feedControllerProvider = NotifierProvider<FeedController, FeedState>(
  FeedController.new,
);

class FeedState {
  const FeedState({
    this.items = const [],
    this.nextCursor,
    this.hasMore = false,
    this.isLoading = false,
    this.isRefreshing = false,
    this.isLoadingNextPage = false,
    this.isOffline = false,
    this.isSubmitting = false,
    this.pendingReactionPostIds = const {},
    this.pendingFollowActorIds = const {},
    this.message,
  });

  final List<FeedPost> items;
  final String? nextCursor;
  final bool hasMore;
  final bool isLoading;
  final bool isRefreshing;
  final bool isLoadingNextPage;
  final bool isOffline;
  final bool isSubmitting;
  final Set<String> pendingReactionPostIds;
  final Set<String> pendingFollowActorIds;
  final String? message;

  FeedState copyWith({
    List<FeedPost>? items,
    String? nextCursor,
    bool? hasMore,
    bool? isLoading,
    bool? isRefreshing,
    bool? isLoadingNextPage,
    bool? isOffline,
    bool? isSubmitting,
    Set<String>? pendingReactionPostIds,
    Set<String>? pendingFollowActorIds,
    String? message,
    bool clearCursor = false,
    bool clearMessage = false,
  }) => FeedState(
    items: items ?? this.items,
    nextCursor: clearCursor ? null : nextCursor ?? this.nextCursor,
    hasMore: hasMore ?? this.hasMore,
    isLoading: isLoading ?? this.isLoading,
    isRefreshing: isRefreshing ?? this.isRefreshing,
    isLoadingNextPage: isLoadingNextPage ?? this.isLoadingNextPage,
    isOffline: isOffline ?? this.isOffline,
    isSubmitting: isSubmitting ?? this.isSubmitting,
    pendingReactionPostIds:
        pendingReactionPostIds ?? this.pendingReactionPostIds,
    pendingFollowActorIds: pendingFollowActorIds ?? this.pendingFollowActorIds,
    message: clearMessage ? null : message ?? this.message,
  );
}

class FeedController extends Notifier<FeedState> {
  Future<void>? _activeLoad;
  _FeedScope? _scope;
  int _generation = 0;

  @override
  FeedState build() {
    _scope = ref.watch(sessionControllerProvider.select(_scopeFromSession));
    _generation++;
    _activeLoad = null;
    return const FeedState();
  }

  Future<void> load({bool refresh = false}) {
    final operation = _operation();
    if (operation == null) {
      state = const FeedState(
        message: 'A private world is required to view the feed.',
      );
      return Future.value();
    }
    if (_activeLoad case final active?) {
      return active;
    }
    final load = _load(operation, refresh: refresh);
    _activeLoad = load;
    return load.whenComplete(() {
      if (identical(_activeLoad, load)) {
        _activeLoad = null;
      }
    });
  }

  Future<void> loadNextPage() async {
    if (!state.hasMore || state.isLoadingNextPage) {
      return;
    }
    final operation = _operation();
    if (operation == null) {
      return;
    }
    final scope = operation.scope;

    state = state.copyWith(isLoadingNextPage: true, clearMessage: true);
    try {
      final page = await ref
          .read(feedRepositoryProvider)
          .fetchFeed(
            userId: scope.userId,
            worldId: scope.worldId,
            isCurrent: () => _isCurrent(operation),
            cursor: state.nextCursor,
          );
      if (!_isCurrent(operation)) {
        return;
      }
      final knownIds = state.items.map((post) => post.id).toSet();
      state = state.copyWith(
        items: [
          ...state.items,
          ...page.items.where((post) => knownIds.add(post.id)),
        ],
        nextCursor: page.nextCursor,
        hasMore: page.hasMore,
        isLoadingNextPage: false,
        isOffline: false,
      );
    } on AppFailure catch (failure) {
      if (!_isCurrent(operation)) {
        return;
      }
      state = state.copyWith(
        isLoadingNextPage: false,
        isOffline: failure is NetworkFailure,
        message: failure.message,
      );
    } on FormatException {
      if (!_isCurrent(operation)) {
        return;
      }
      state = state.copyWith(
        isLoadingNextPage: false,
        message: const UnknownFailure().message,
      );
    }
  }

  Future<void> submit(String content) async {
    final normalized = content.trim();
    final operation = _operation();
    if (operation == null ||
        normalized.isEmpty ||
        normalized.characters.length > 500) {
      return;
    }
    final scope = operation.scope;

    final generator = ref.read(secretGeneratorProvider);
    final clientPostId = generator.newIdempotencyKey();
    final pending = FeedPost(
      id: clientPostId,
      worldId: scope.worldId,
      author: FeedAuthor(
        actorId: scope.playerActorId,
        displayName: scope.playerDisplayName,
        handle: 'you',
        actorType: 'player',
      ),
      content: normalized,
      createdAtUtc: ref.read(utcNowProvider)(),
      counts: const FeedCounts(likes: 0, replies: 0),
      visibility: 'world',
      localState: FeedPostLocalState.pending,
      clientPostId: clientPostId,
      idempotencyKey: generator.newIdempotencyKey(),
    );
    state = state.copyWith(
      items: [pending, ...state.items],
      isSubmitting: true,
      clearMessage: true,
    );
    await _sendPending(operation, pending);
  }

  Future<void> retry(String clientPostId) async {
    final operation = _operation();
    if (operation == null || state.isSubmitting) {
      return;
    }
    final index = state.items.indexWhere(
      (post) => post.clientPostId == clientPostId,
    );
    if (index < 0) {
      return;
    }
    final pending = state.items[index].copyWith(
      localState: FeedPostLocalState.pending,
      clearFailure: true,
    );
    final items = [...state.items]..[index] = pending;
    state = state.copyWith(
      items: items,
      isSubmitting: true,
      clearMessage: true,
    );
    await _sendPending(operation, pending);
  }

  Future<void> toggleLike(String postId) async {
    final operation = _operation();
    final index = state.items.indexWhere((post) => post.id == postId);
    if (operation == null ||
        index < 0 ||
        state.pendingReactionPostIds.contains(postId)) {
      return;
    }
    final original = state.items[index];
    if (original.author.actorId == operation.scope.playerActorId) {
      return;
    }
    final active = original.currentPlayerReaction != 'like';
    final optimistic = original.copyWith(
      counts: FeedCounts(
        likes: active
            ? original.counts.likes + 1
            : original.counts.likes > 0
            ? original.counts.likes - 1
            : 0,
        replies: original.counts.replies,
      ),
      currentPlayerReaction: active ? 'like' : null,
    );
    state = state.copyWith(
      items: _replacePost(state.items, optimistic),
      pendingReactionPostIds: {...state.pendingReactionPostIds, postId},
      clearMessage: true,
    );
    try {
      final result = await ref
          .read(feedRepositoryProvider)
          .setLike(
            userId: operation.scope.userId,
            worldId: operation.scope.worldId,
            post: original,
            active: active,
            isCurrent: () => _isCurrent(operation),
          );
      if (!_isCurrent(operation)) {
        return;
      }
      final authoritative = optimistic.copyWith(
        counts: FeedCounts(
          likes: result.likeCount,
          replies: optimistic.counts.replies,
        ),
        currentPlayerReaction: result.active ? 'like' : null,
      );
      state = state.copyWith(
        items: _replacePost(state.items, authoritative),
        pendingReactionPostIds: {...state.pendingReactionPostIds}
          ..remove(postId),
      );
    } on FeedOperationCancelled {
      return;
    } on AppFailure catch (failure) {
      if (!_isCurrent(operation)) {
        return;
      }
      state = state.copyWith(
        items: _replacePost(state.items, original),
        pendingReactionPostIds: {...state.pendingReactionPostIds}
          ..remove(postId),
        message: failure.message,
      );
    }
  }

  Future<void> toggleFollow(String actorId) async {
    final operation = _operation();
    if (operation == null || state.pendingFollowActorIds.contains(actorId)) {
      return;
    }
    final matching = state.items.where(
      (post) => post.author.actorId == actorId,
    );
    if (matching.isEmpty || matching.first.author.actorType != 'character') {
      return;
    }
    final active = !matching.first.author.isFollowed;
    state = state.copyWith(
      items: _setActorFollowed(state.items, actorId, active),
      pendingFollowActorIds: {...state.pendingFollowActorIds, actorId},
      clearMessage: true,
    );
    try {
      await ref
          .read(feedRepositoryProvider)
          .setFollow(
            worldId: operation.scope.worldId,
            actorId: actorId,
            active: active,
          );
      if (!_isCurrent(operation)) {
        return;
      }
      state = state.copyWith(
        pendingFollowActorIds: {...state.pendingFollowActorIds}
          ..remove(actorId),
      );
    } on AppFailure catch (failure) {
      if (!_isCurrent(operation)) {
        return;
      }
      state = state.copyWith(
        items: _setActorFollowed(state.items, actorId, !active),
        pendingFollowActorIds: {...state.pendingFollowActorIds}
          ..remove(actorId),
        message: failure.message,
      );
    }
  }

  Future<void> _sendPending(_FeedOperation operation, FeedPost pending) async {
    final scope = operation.scope;
    try {
      final created = await ref
          .read(feedRepositoryProvider)
          .createPost(
            userId: scope.userId,
            worldId: scope.worldId,
            pendingPost: pending,
            isCurrent: () => _isCurrent(operation),
          );
      if (!_isCurrent(operation)) {
        return;
      }
      state = state.copyWith(
        items: _replaceOperation(state.items, pending.clientPostId!, created),
        isSubmitting: false,
      );
    } on FeedOperationCancelled {
      return;
    } on AppFailure catch (failure) {
      if (!_isCurrent(operation)) {
        return;
      }
      state = state.copyWith(
        items: _markOperationFailed(
          state.items,
          pending.clientPostId!,
          failure.message,
        ),
        isSubmitting: false,
        isOffline: failure is NetworkFailure,
        message: failure.message,
      );
    } on FormatException {
      if (!_isCurrent(operation)) {
        return;
      }
      final message = const UnknownFailure().message;
      state = state.copyWith(
        items: _markOperationFailed(
          state.items,
          pending.clientPostId!,
          message,
        ),
        isSubmitting: false,
        message: message,
      );
    }
  }

  Future<void> _load(_FeedOperation operation, {required bool refresh}) async {
    final scope = operation.scope;
    final repository = ref.read(feedRepositoryProvider);
    if (!refresh) {
      CachedFeed? cached;
      try {
        cached = await repository.readCachedFeed(scope.userId, scope.worldId);
      } catch (_) {
        cached = null;
      }
      if (!_isCurrent(operation)) {
        return;
      }
      if (cached == null) {
        state = const FeedState(isLoading: true);
      } else {
        state = FeedState(
          items: cached.items,
          nextCursor: cached.nextCursor,
          hasMore: cached.hasMore,
          isRefreshing: true,
        );
      }
    } else {
      state = state.copyWith(
        isLoading: state.items.isEmpty,
        isRefreshing: state.items.isNotEmpty,
        clearMessage: true,
      );
    }

    try {
      final page = await repository.fetchFeed(
        userId: scope.userId,
        worldId: scope.worldId,
        isCurrent: () => _isCurrent(operation),
      );
      if (!_isCurrent(operation)) {
        return;
      }
      final localItems = state.items
          .where((post) => post.localState != FeedPostLocalState.synced)
          .toList(growable: false);
      final localIds = localItems.map((post) => post.id).toSet();
      state = FeedState(
        items: [
          ...localItems,
          ...page.items.where((post) => localIds.add(post.id)),
        ],
        nextCursor: page.nextCursor,
        hasMore: page.hasMore,
      );
    } on NetworkFailure catch (failure) {
      if (!_isCurrent(operation)) {
        return;
      }
      if (state.items.isNotEmpty || !state.isLoading) {
        state = state.copyWith(
          isLoading: false,
          isRefreshing: false,
          isOffline: true,
          message: 'Offline — showing a saved feed that may be out of date.',
        );
      } else {
        state = FeedState(message: failure.message);
      }
    } on AppFailure catch (failure) {
      if (!_isCurrent(operation)) {
        return;
      }
      state = state.copyWith(
        isLoading: false,
        isRefreshing: false,
        message: failure.message,
      );
    } on FormatException {
      if (!_isCurrent(operation)) {
        return;
      }
      state = state.copyWith(
        isLoading: false,
        isRefreshing: false,
        message: const UnknownFailure().message,
      );
    }
  }

  _FeedOperation? _operation() {
    final scope = _scope;
    return scope == null ? null : _FeedOperation(scope, _generation);
  }

  bool _isCurrent(_FeedOperation operation) =>
      operation.generation == _generation &&
      operation.scope == _scopeFromSession(ref.read(sessionControllerProvider));

  static _FeedScope? _scopeFromSession(SessionState session) {
    final userId = session.userId;
    final world = session.world;
    return userId == null || world == null
        ? null
        : _FeedScope(
            userId,
            world.id,
            world.playerActorId,
            world.playerDisplayName,
          );
  }

  static List<FeedPost> _replaceOperation(
    List<FeedPost> items,
    String clientPostId,
    FeedPost created,
  ) {
    final result = <FeedPost>[];
    var replaced = false;
    for (final item in items) {
      if (item.clientPostId == clientPostId) {
        if (!replaced) {
          result.add(created);
          replaced = true;
        }
      } else if (item.id != created.id) {
        result.add(item);
      }
    }
    if (!replaced) {
      result.insert(0, created);
    }
    return result;
  }

  static List<FeedPost> _markOperationFailed(
    List<FeedPost> items,
    String clientPostId,
    String message,
  ) => items
      .map(
        (post) => post.clientPostId == clientPostId
            ? post.copyWith(
                localState: FeedPostLocalState.failed,
                failureMessage: message,
              )
            : post,
      )
      .toList(growable: false);

  static List<FeedPost> _replacePost(
    List<FeedPost> items,
    FeedPost replacement,
  ) => items
      .map((post) => post.id == replacement.id ? replacement : post)
      .toList(growable: false);

  static List<FeedPost> _setActorFollowed(
    List<FeedPost> items,
    String actorId,
    bool isFollowed,
  ) => items
      .map(
        (post) => post.author.actorId == actorId
            ? post.copyWith(
                author: post.author.copyWith(isFollowed: isFollowed),
              )
            : post,
      )
      .toList(growable: false);
}

class _FeedScope {
  const _FeedScope(
    this.userId,
    this.worldId,
    this.playerActorId,
    this.playerDisplayName,
  );

  final String userId;
  final String worldId;
  final String playerActorId;
  final String playerDisplayName;

  @override
  bool operator ==(Object other) =>
      other is _FeedScope &&
      other.userId == userId &&
      other.worldId == worldId &&
      other.playerActorId == playerActorId &&
      other.playerDisplayName == playerDisplayName;

  @override
  int get hashCode =>
      Object.hash(userId, worldId, playerActorId, playerDisplayName);
}

class _FeedOperation {
  const _FeedOperation(this.scope, this.generation);

  final _FeedScope scope;
  final int generation;
}
