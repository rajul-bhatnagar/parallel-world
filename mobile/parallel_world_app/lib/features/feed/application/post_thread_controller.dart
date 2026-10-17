import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/feed/application/feed_contracts.dart';
import 'package:parallel_world_app/features/feed/application/feed_dependencies.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_dependencies.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

final postThreadControllerProvider = NotifierProvider.autoDispose
    .family<PostThreadController, PostThreadState, String>(
      PostThreadController.new,
    );

class PostThreadState {
  const PostThreadState({
    this.root,
    this.replies = const [],
    this.nextCursor,
    this.hasMore = false,
    this.isLoading = false,
    this.isLoadingNextPage = false,
    this.isSubmitting = false,
    this.pendingReactionPostIds = const {},
    this.pendingFollowActorIds = const {},
    this.message,
  });

  final FeedPost? root;
  final List<FeedPost> replies;
  final String? nextCursor;
  final bool hasMore;
  final bool isLoading;
  final bool isLoadingNextPage;
  final bool isSubmitting;
  final Set<String> pendingReactionPostIds;
  final Set<String> pendingFollowActorIds;
  final String? message;

  PostThreadState copyWith({
    FeedPost? root,
    List<FeedPost>? replies,
    String? nextCursor,
    bool? hasMore,
    bool? isLoading,
    bool? isLoadingNextPage,
    bool? isSubmitting,
    Set<String>? pendingReactionPostIds,
    Set<String>? pendingFollowActorIds,
    String? message,
    bool clearCursor = false,
    bool clearMessage = false,
  }) => PostThreadState(
    root: root ?? this.root,
    replies: replies ?? this.replies,
    nextCursor: clearCursor ? null : nextCursor ?? this.nextCursor,
    hasMore: hasMore ?? this.hasMore,
    isLoading: isLoading ?? this.isLoading,
    isLoadingNextPage: isLoadingNextPage ?? this.isLoadingNextPage,
    isSubmitting: isSubmitting ?? this.isSubmitting,
    pendingReactionPostIds:
        pendingReactionPostIds ?? this.pendingReactionPostIds,
    pendingFollowActorIds: pendingFollowActorIds ?? this.pendingFollowActorIds,
    message: clearMessage ? null : message ?? this.message,
  );
}

class PostThreadController extends Notifier<PostThreadState> {
  PostThreadController(this.postId);

  final String postId;
  _ThreadScope? _scope;
  int _generation = 0;

  @override
  PostThreadState build() {
    _scope = ref.watch(sessionControllerProvider.select(_scopeFromSession));
    _generation++;
    return const PostThreadState();
  }

  Future<void> load() async {
    final operation = _operation();
    if (operation == null) {
      state = const PostThreadState(message: 'This thread is unavailable.');
      return;
    }
    state = state.copyWith(isLoading: true, clearMessage: true);
    try {
      final repository = ref.read(feedRepositoryProvider);
      final results = await Future.wait<Object>([
        repository.getPost(worldId: operation.scope.worldId, postId: postId),
        repository.getReplies(
          worldId: operation.scope.worldId,
          parentPostId: postId,
        ),
      ]);
      if (!_isCurrent(operation)) {
        return;
      }
      final page = results[1] as FeedPage;
      state = PostThreadState(
        root: results[0] as FeedPost,
        replies: page.items,
        nextCursor: page.nextCursor,
        hasMore: page.hasMore,
      );
    } on AppFailure catch (failure) {
      if (_isCurrent(operation)) {
        state = PostThreadState(message: failure.message);
      }
    } on FormatException {
      if (_isCurrent(operation)) {
        state = PostThreadState(message: const UnknownFailure().message);
      }
    }
  }

  Future<void> loadNextPage() async {
    final operation = _operation();
    if (operation == null || !state.hasMore || state.isLoadingNextPage) {
      return;
    }
    state = state.copyWith(isLoadingNextPage: true, clearMessage: true);
    try {
      final page = await ref
          .read(feedRepositoryProvider)
          .getReplies(
            worldId: operation.scope.worldId,
            parentPostId: postId,
            cursor: state.nextCursor,
          );
      if (!_isCurrent(operation)) {
        return;
      }
      final known = state.replies.map((reply) => reply.id).toSet();
      state = state.copyWith(
        replies: [
          ...state.replies,
          ...page.items.where((reply) => known.add(reply.id)),
        ],
        nextCursor: page.nextCursor,
        hasMore: page.hasMore,
        isLoadingNextPage: false,
      );
    } on AppFailure catch (failure) {
      if (_isCurrent(operation)) {
        state = state.copyWith(
          isLoadingNextPage: false,
          message: failure.message,
        );
      }
    } on FormatException {
      if (_isCurrent(operation)) {
        state = state.copyWith(
          isLoadingNextPage: false,
          message: const UnknownFailure().message,
        );
      }
    }
  }

  Future<void> submitReply(String content) async {
    final normalized = content.trim();
    final operation = _operation();
    if (operation == null ||
        normalized.isEmpty ||
        normalized.characters.length > 500 ||
        state.isSubmitting) {
      return;
    }
    final generator = ref.read(secretGeneratorProvider);
    final clientPostId = generator.newIdempotencyKey();
    final pending = FeedPost(
      id: clientPostId,
      worldId: operation.scope.worldId,
      author: FeedAuthor(
        actorId: operation.scope.playerActorId,
        displayName: operation.scope.playerDisplayName,
        handle: 'you',
        actorType: 'player',
      ),
      content: normalized,
      createdAtUtc: ref.read(utcNowProvider)(),
      parentPostId: postId,
      counts: const FeedCounts(likes: 0, replies: 0),
      visibility: 'world',
      localState: FeedPostLocalState.pending,
      clientPostId: clientPostId,
      idempotencyKey: generator.newIdempotencyKey(),
    );
    state = state.copyWith(
      replies: [...state.replies, pending],
      isSubmitting: true,
      clearMessage: true,
    );
    await _sendReply(operation, pending);
  }

  Future<void> retryReply(String clientPostId) async {
    final operation = _operation();
    final index = state.replies.indexWhere(
      (reply) => reply.clientPostId == clientPostId,
    );
    if (operation == null || index < 0 || state.isSubmitting) {
      return;
    }
    final pending = state.replies[index].copyWith(
      localState: FeedPostLocalState.pending,
      clearFailure: true,
    );
    final replies = [...state.replies]..[index] = pending;
    state = state.copyWith(
      replies: replies,
      isSubmitting: true,
      clearMessage: true,
    );
    await _sendReply(operation, pending);
  }

  Future<void> toggleLike(String targetPostId) async {
    final operation = _operation();
    final original = _findPost(targetPostId);
    if (operation == null ||
        original == null ||
        original.author.actorId == operation.scope.playerActorId ||
        state.pendingReactionPostIds.contains(targetPostId)) {
      return;
    }
    final active = original.currentPlayerReaction != 'like';
    final optimistic = _withReaction(original, active);
    _replaceVisiblePost(optimistic);
    state = state.copyWith(
      pendingReactionPostIds: {...state.pendingReactionPostIds, targetPostId},
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
      _replaceVisiblePost(
        optimistic.copyWith(
          counts: FeedCounts(
            likes: result.likeCount,
            replies: optimistic.counts.replies,
          ),
          currentPlayerReaction: result.active ? 'like' : null,
        ),
      );
      state = state.copyWith(
        pendingReactionPostIds: {...state.pendingReactionPostIds}
          ..remove(targetPostId),
      );
    } on FeedOperationCancelled {
      return;
    } on AppFailure catch (failure) {
      if (_isCurrent(operation)) {
        _replaceVisiblePost(original);
        state = state.copyWith(
          pendingReactionPostIds: {...state.pendingReactionPostIds}
            ..remove(targetPostId),
          message: failure.message,
        );
      }
    } on FormatException {
      if (_isCurrent(operation)) {
        _replaceVisiblePost(original);
        state = state.copyWith(
          pendingReactionPostIds: {...state.pendingReactionPostIds}
            ..remove(targetPostId),
          message: const UnknownFailure().message,
        );
      }
    }
  }

  Future<void> toggleFollow(String actorId) async {
    final operation = _operation();
    final post = [state.root, ...state.replies]
        .whereType<FeedPost>()
        .where((item) => item.author.actorId == actorId)
        .firstOrNull;
    if (operation == null ||
        post == null ||
        post.author.actorType != 'character' ||
        state.pendingFollowActorIds.contains(actorId)) {
      return;
    }
    final active = !post.author.isFollowed;
    _setActorFollowed(actorId, active);
    state = state.copyWith(
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
      if (_isCurrent(operation)) {
        state = state.copyWith(
          pendingFollowActorIds: {...state.pendingFollowActorIds}
            ..remove(actorId),
        );
      }
    } on AppFailure catch (failure) {
      if (_isCurrent(operation)) {
        _setActorFollowed(actorId, !active);
        state = state.copyWith(
          pendingFollowActorIds: {...state.pendingFollowActorIds}
            ..remove(actorId),
          message: failure.message,
        );
      }
    } on FormatException {
      if (_isCurrent(operation)) {
        _setActorFollowed(actorId, !active);
        state = state.copyWith(
          pendingFollowActorIds: {...state.pendingFollowActorIds}
            ..remove(actorId),
          message: const UnknownFailure().message,
        );
      }
    }
  }

  Future<void> _sendReply(_ThreadOperation operation, FeedPost pending) async {
    try {
      final created = await ref
          .read(feedRepositoryProvider)
          .createReply(
            worldId: operation.scope.worldId,
            parentPostId: postId,
            content: pending.content,
            clientPostId: pending.clientPostId!,
            idempotencyKey: pending.idempotencyKey!,
          );
      if (!_isCurrent(operation)) {
        return;
      }
      final replies = state.replies
          .map(
            (reply) =>
                reply.clientPostId == pending.clientPostId ? created : reply,
          )
          .toList(growable: false);
      final root = state.root;
      state = state.copyWith(
        root: root?.copyWith(
          counts: FeedCounts(
            likes: root.counts.likes,
            replies: root.counts.replies + 1,
          ),
        ),
        replies: replies,
        isSubmitting: false,
      );
    } on AppFailure catch (failure) {
      if (_isCurrent(operation)) {
        state = state.copyWith(
          replies: state.replies
              .map(
                (reply) => reply.clientPostId == pending.clientPostId
                    ? reply.copyWith(
                        localState: FeedPostLocalState.failed,
                        failureMessage: failure.message,
                      )
                    : reply,
              )
              .toList(growable: false),
          isSubmitting: false,
          message: failure.message,
        );
      }
    } on FormatException {
      if (_isCurrent(operation)) {
        final message = const UnknownFailure().message;
        state = state.copyWith(
          replies: state.replies
              .map(
                (reply) => reply.clientPostId == pending.clientPostId
                    ? reply.copyWith(
                        localState: FeedPostLocalState.failed,
                        failureMessage: message,
                      )
                    : reply,
              )
              .toList(growable: false),
          isSubmitting: false,
          message: message,
        );
      }
    }
  }

  FeedPost? _findPost(String targetPostId) {
    if (state.root?.id == targetPostId) {
      return state.root;
    }
    for (final reply in state.replies) {
      if (reply.id == targetPostId) {
        return reply;
      }
    }
    return null;
  }

  void _replaceVisiblePost(FeedPost replacement) {
    if (state.root?.id == replacement.id) {
      state = state.copyWith(root: replacement);
      return;
    }
    state = state.copyWith(
      replies: state.replies
          .map((reply) => reply.id == replacement.id ? replacement : reply)
          .toList(growable: false),
    );
  }

  void _setActorFollowed(String actorId, bool isFollowed) {
    FeedPost update(FeedPost post) => post.author.actorId == actorId
        ? post.copyWith(author: post.author.copyWith(isFollowed: isFollowed))
        : post;
    state = state.copyWith(
      root: state.root == null ? null : update(state.root!),
      replies: state.replies.map(update).toList(growable: false),
    );
  }

  static FeedPost _withReaction(FeedPost post, bool active) => post.copyWith(
    counts: FeedCounts(
      likes: active
          ? post.counts.likes + 1
          : post.counts.likes > 0
          ? post.counts.likes - 1
          : 0,
      replies: post.counts.replies,
    ),
    currentPlayerReaction: active ? 'like' : null,
  );

  _ThreadOperation? _operation() {
    final scope = _scope;
    return scope == null ? null : _ThreadOperation(scope, _generation);
  }

  bool _isCurrent(_ThreadOperation operation) =>
      operation.generation == _generation &&
      operation.scope == _scopeFromSession(ref.read(sessionControllerProvider));

  static _ThreadScope? _scopeFromSession(SessionState session) {
    final userId = session.userId;
    final world = session.world;
    return userId == null || world == null
        ? null
        : _ThreadScope(
            userId,
            world.id,
            world.playerActorId,
            world.playerDisplayName,
          );
  }
}

class _ThreadScope {
  const _ThreadScope(
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
      other is _ThreadScope &&
      other.userId == userId &&
      other.worldId == worldId &&
      other.playerActorId == playerActorId &&
      other.playerDisplayName == playerDisplayName;

  @override
  int get hashCode =>
      Object.hash(userId, worldId, playerActorId, playerDisplayName);
}

class _ThreadOperation {
  const _ThreadOperation(this.scope, this.generation);

  final _ThreadScope scope;
  final int generation;
}
