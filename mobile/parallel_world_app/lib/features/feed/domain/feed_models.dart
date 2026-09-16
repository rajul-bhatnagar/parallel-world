enum FeedPostLocalState { synced, pending, failed }

class FeedAuthor {
  const FeedAuthor({
    required this.actorId,
    required this.displayName,
    required this.handle,
    required this.actorType,
  });

  factory FeedAuthor.fromJson(Map<String, Object?> json) => FeedAuthor(
    actorId: _requiredString(json, 'actorId'),
    displayName: _requiredString(json, 'displayName'),
    handle: _requiredString(json, 'handle'),
    actorType: _requiredString(json, 'actorType'),
  );

  final String actorId;
  final String displayName;
  final String handle;
  final String actorType;
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
  final FeedPostLocalState localState;
  final String? clientPostId;
  final String? idempotencyKey;
  final String? failureMessage;

  FeedPost copyWith({
    FeedPostLocalState? localState,
    String? failureMessage,
    bool clearFailure = false,
  }) => FeedPost(
    id: id,
    worldId: worldId,
    author: author,
    content: content,
    createdAtUtc: createdAtUtc,
    parentPostId: parentPostId,
    counts: counts,
    visibility: visibility,
    localState: localState ?? this.localState,
    clientPostId: clientPostId,
    idempotencyKey: idempotencyKey,
    failureMessage: clearFailure ? null : failureMessage ?? this.failureMessage,
  );
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
