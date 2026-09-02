class WorldSummary {
  const WorldSummary({
    required this.id,
    required this.name,
    required this.status,
    required this.currentGameTimeUtc,
    required this.playerActorId,
    required this.playerDisplayName,
    required this.createdAtUtc,
  });

  factory WorldSummary.fromJson(Map<String, Object?> json) {
    final player = json['player'];
    if (player is! Map<String, Object?>) {
      throw const FormatException('World player is missing.');
    }

    return WorldSummary(
      id: _requiredString(json, 'id'),
      name: _requiredString(json, 'name'),
      status: _requiredString(json, 'status'),
      currentGameTimeUtc: DateTime.parse(
        _requiredString(json, 'currentGameTimeUtc'),
      ).toUtc(),
      playerActorId: _requiredString(player, 'actorId'),
      playerDisplayName: _requiredString(player, 'displayName'),
      createdAtUtc: DateTime.parse(_requiredString(json, 'createdAtUtc'))
          .toUtc(),
    );
  }

  final String id;
  final String name;
  final String status;
  final DateTime currentGameTimeUtc;
  final String playerActorId;
  final String playerDisplayName;
  final DateTime createdAtUtc;

  static String _requiredString(Map<String, Object?> json, String key) {
    final value = json[key];
    if (value is! String || value.isEmpty) {
      throw FormatException('$key is missing.');
    }
    return value;
  }
}
