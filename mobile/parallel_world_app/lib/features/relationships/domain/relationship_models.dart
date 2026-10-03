class RelationshipSummary {
  const RelationshipSummary({required this.state, required this.updatedAtUtc});

  final String state;
  final DateTime updatedAtUtc;

  factory RelationshipSummary.fromJson(Map<String, Object?> json) =>
      RelationshipSummary(
        state: json['state']! as String,
        updatedAtUtc: DateTime.parse(json['updatedAtUtc']! as String).toUtc(),
      );
}

class RelationshipHistoryItem {
  const RelationshipHistoryItem({
    required this.id,
    required this.eventType,
    required this.occurredAtUtc,
  });

  final String id;
  final String eventType;
  final DateTime occurredAtUtc;

  factory RelationshipHistoryItem.fromJson(Map<String, Object?> json) =>
      RelationshipHistoryItem(
        id: json['id']! as String,
        eventType: json['eventType']! as String,
        occurredAtUtc: DateTime.parse(json['occurredAtUtc']! as String).toUtc(),
      );
}

class RelationshipView {
  const RelationshipView({
    this.summary,
    this.history = const [],
    this.isOffline = false,
  });

  final RelationshipSummary? summary;
  final List<RelationshipHistoryItem> history;
  final bool isOffline;
}
