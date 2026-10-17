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
    this.invitation,
    this.romanticHistory = const [],
    this.isOffline = false,
  });

  final RelationshipSummary? summary;
  final List<RelationshipHistoryItem> history;
  final DatingInvitation? invitation;
  final List<RomanticHistoryItem> romanticHistory;
  final bool isOffline;
}

class DatingInvitation {
  const DatingInvitation({
    required this.id,
    required this.episodeId,
    required this.characterId,
    required this.initiatorActorId,
    required this.targetActorId,
    required this.dateType,
    required this.status,
    required this.romanticStatus,
    required this.reasonCode,
    required this.createdAtWorldTime,
    required this.expiresAtWorldTime,
  });

  final String id;
  final String episodeId;
  final String characterId;
  final String initiatorActorId;
  final String targetActorId;
  final String dateType;
  final String status;
  final String romanticStatus;
  final String reasonCode;
  final DateTime createdAtWorldTime;
  final DateTime expiresAtWorldTime;

  factory DatingInvitation.fromJson(
    Map<String, Object?> json,
  ) => DatingInvitation(
    id: json['id']! as String,
    episodeId: json['episodeId']! as String,
    characterId: json['characterId']! as String,
    initiatorActorId: json['initiatorActorId']! as String,
    targetActorId: json['targetActorId']! as String,
    dateType: json['dateType']! as String,
    status: json['status']! as String,
    romanticStatus: json['romanticStatus']! as String,
    reasonCode: json['reasonCode']! as String,
    createdAtWorldTime: DateTime.parse(json['createdAtWorldTime']! as String)
        .toUtc(),
    expiresAtWorldTime: DateTime.parse(json['expiresAtWorldTime']! as String)
        .toUtc(),
  );
}

class RomanticHistoryItem {
  const RomanticHistoryItem({
    required this.id,
    required this.episodeId,
    required this.fromStatus,
    required this.toStatus,
    required this.reasonCode,
    required this.occurredAtWorldTime,
  });

  final String id;
  final String episodeId;
  final String fromStatus;
  final String toStatus;
  final String reasonCode;
  final DateTime occurredAtWorldTime;

  factory RomanticHistoryItem.fromJson(Map<String, Object?> json) =>
      RomanticHistoryItem(
        id: json['id']! as String,
        episodeId: json['episodeId']! as String,
        fromStatus: json['fromStatus']! as String,
        toStatus: json['toStatus']! as String,
        reasonCode: json['reasonCode']! as String,
        occurredAtWorldTime: DateTime.parse(
          json['occurredAtWorldTime']! as String,
        ).toUtc(),
      );
}
