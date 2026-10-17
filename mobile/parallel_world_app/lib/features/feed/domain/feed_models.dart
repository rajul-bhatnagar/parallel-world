enum FeedPostLocalState { synced, pending, failed }

class FeedAuthor {
  const FeedAuthor({
    required this.actorId,
    required this.displayName,
    required this.handle,
    required this.actorType,
    this.isFollowed = false,
  });

  factory FeedAuthor.fromJson(Map<String, Object?> json) => FeedAuthor(
    actorId: _requiredString(json, 'actorId'),
    displayName: _requiredString(json, 'displayName'),
    handle: _requiredString(json, 'handle'),
    actorType: _requiredString(json, 'actorType'),
    isFollowed: _requiredBool(json, 'isFollowed'),
  );

  final String actorId;
  final String displayName;
  final String handle;
  final String actorType;
  final bool isFollowed;

  FeedAuthor copyWith({bool? isFollowed}) => FeedAuthor(
    actorId: actorId,
    displayName: displayName,
    handle: handle,
    actorType: actorType,
    isFollowed: isFollowed ?? this.isFollowed,
  );
}

class FeedCounts {
  const FeedCounts({required this.likes, required this.replies});

  factory FeedCounts.fromJson(Map<String, Object?> json) => FeedCounts(
    likes: _requiredInt(json, 'likes'),
    replies: _requiredInt(json, 'replies'),
  );

  final int likes;
  final int replies;
}

class FeedPost {
  const FeedPost({
    required this.id,
    required this.worldId,
    required this.author,
    required this.content,
    required this.createdAtUtc,
    required this.counts,
    required this.visibility,
    this.currentPlayerReaction,
    this.parentPostId,
    this.localState = FeedPostLocalState.synced,
    this.clientPostId,
    this.idempotencyKey,
    this.failureMessage,
  });

  factory FeedPost.fromJson(Map<String, Object?> json) {
    final author = json['author'];
    final counts = json['counts'];
    final parent = json['parent'];
    if (author is! Map || counts is! Map || parent != null && parent is! Map) {
      throw const FormatException('Expected feed post objects.');
    }
    return FeedPost(
      id: _requiredString(json, 'id'),
      worldId: _requiredString(json, 'worldId'),
      author: FeedAuthor.fromJson(_jsonObject(author)),
      content: _requiredString(json, 'content'),
      createdAtUtc: DateTime.parse(_requiredString(json, 'createdAtUtc'))
          .toUtc(),
      parentPostId: parent == null
          ? null
          : _requiredString(_jsonObject(parent), 'id'),
      counts: FeedCounts.fromJson(_jsonObject(counts)),
      currentPlayerReaction: json['currentPlayerReaction'] as String?,
      visibility: _requiredString(json, 'visibility'),
    );
  }

  final String id;
  final String worldId;
  final FeedAuthor author;
  final String content;
  final DateTime createdAtUtc;
  final String? parentPostId;
  final FeedCounts counts;
  final String visibility;
  final String? currentPlayerReaction;
  final FeedPostLocalState localState;
  final String? clientPostId;
  final String? idempotencyKey;
  final String? failureMessage;

  FeedPost copyWith({
    FeedAuthor? author,
    FeedCounts? counts,
    Object? currentPlayerReaction = _unchanged,
    FeedPostLocalState? localState,
    String? failureMessage,
    bool clearFailure = false,
  }) => FeedPost(
    id: id,
    worldId: worldId,
    author: author ?? this.author,
    content: content,
    createdAtUtc: createdAtUtc,
    parentPostId: parentPostId,
    counts: counts ?? this.counts,
    currentPlayerReaction: identical(currentPlayerReaction, _unchanged)
        ? this.currentPlayerReaction
        : currentPlayerReaction as String?,
    visibility: visibility,
    localState: localState ?? this.localState,
    clientPostId: clientPostId,
    idempotencyKey: idempotencyKey,
    failureMessage: clearFailure ? null : failureMessage ?? this.failureMessage,
  );
}

const Object _unchanged = Object();

class ReactionState {
  const ReactionState({
    required this.postId,
    required this.type,
    required this.active,
    required this.likeCount,
  });

  factory ReactionState.fromJson(Map<String, Object?> json) => ReactionState(
    postId: _requiredString(json, 'postId'),
    type: _requiredString(json, 'type'),
    active: _requiredBool(json, 'active'),
    likeCount: _requiredInt(json, 'likeCount'),
  );

  final String postId;
  final String type;
  final bool active;
  final int likeCount;
}

class FollowState {
  const FollowState({
    required this.actorId,
    required this.isFollowing,
    required this.followedAtUtc,
  });

  factory FollowState.fromJson(Map<String, Object?> json) => FollowState(
    actorId: _requiredString(json, 'actorId'),
    isFollowing: _requiredBool(json, 'isFollowing'),
    followedAtUtc: DateTime.parse(_requiredString(json, 'followedAtUtc'))
        .toUtc(),
  );

  final String actorId;
  final bool isFollowing;
  final DateTime followedAtUtc;
}

class FeedPage {
  const FeedPage({
    required this.items,
    required this.nextCursor,
    required this.hasMore,
  });

  factory FeedPage.fromJson(Map<String, Object?> json) {
    final items = json['items'];
    if (items is! List) {
      throw const FormatException('Expected feed items.');
    }
    return FeedPage(
      items: items
          .map((item) => FeedPost.fromJson(_jsonObject(item)))
          .toList(growable: false),
      nextCursor: json['nextCursor'] as String?,
      hasMore: _requiredBool(json, 'hasMore'),
    );
  }

  final List<FeedPost> items;
  final String? nextCursor;
  final bool hasMore;
}

Map<String, Object?> _jsonObject(Object? value) {
  if (value is! Map) {
    throw const FormatException('Expected a JSON object.');
  }
  return <String, Object?>{
    for (final entry in value.entries)
      if (entry.key is String) entry.key as String: entry.value,
  };
}

String _requiredString(Map<String, Object?> json, String key) {
  final value = json[key];
  if (value is! String || value.isEmpty) {
    throw FormatException('Expected $key.');
  }
  return value;
}

int _requiredInt(Map<String, Object?> json, String key) {
  final value = json[key];
  if (value is! int) {
    throw FormatException('Expected $key.');
  }
  return value;
}

bool _requiredBool(Map<String, Object?> json, String key) {
  final value = json[key];
  if (value is! bool) {
    throw FormatException('Expected $key.');
  }
  return value;
}
