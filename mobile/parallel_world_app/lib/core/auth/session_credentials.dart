class SessionCredentials {
  const SessionCredentials({
    required this.userId,
    required this.accessToken,
    required this.accessTokenExpiresAtUtc,
    required this.refreshToken,
    required this.refreshTokenExpiresAtUtc,
    this.refreshMayBeConsumed = false,
  });

  factory SessionCredentials.fromJson(Map<String, Object?> json) =>
      SessionCredentials(
        userId: _requiredString(json, 'userId'),
        accessToken: _requiredString(json, 'accessToken'),
        accessTokenExpiresAtUtc: DateTime.parse(
          _requiredString(json, 'accessTokenExpiresAtUtc'),
        ).toUtc(),
        refreshToken: _requiredString(json, 'refreshToken'),
        refreshTokenExpiresAtUtc: DateTime.parse(
          _requiredString(json, 'refreshTokenExpiresAtUtc'),
        ).toUtc(),
        refreshMayBeConsumed: json['refreshMayBeConsumed'] == true,
      );

  final String userId;
  final String accessToken;
  final DateTime accessTokenExpiresAtUtc;
  final String refreshToken;
  final DateTime refreshTokenExpiresAtUtc;
  final bool refreshMayBeConsumed;

  bool hasUsableAccessToken(DateTime now) =>
      accessTokenExpiresAtUtc.isAfter(now.add(const Duration(seconds: 30)));

  bool hasUsableRefreshToken(DateTime now) =>
      refreshTokenExpiresAtUtc.isAfter(now) && !refreshMayBeConsumed;

  SessionCredentials markRefreshMayBeConsumed() => SessionCredentials(
    userId: userId,
    accessToken: accessToken,
    accessTokenExpiresAtUtc: accessTokenExpiresAtUtc,
    refreshToken: refreshToken,
    refreshTokenExpiresAtUtc: refreshTokenExpiresAtUtc,
    refreshMayBeConsumed: true,
  );

  Map<String, Object?> toJson() => {
    'userId': userId,
    'accessToken': accessToken,
    'accessTokenExpiresAtUtc': accessTokenExpiresAtUtc.toIso8601String(),
    'refreshToken': refreshToken,
    'refreshTokenExpiresAtUtc': refreshTokenExpiresAtUtc.toIso8601String(),
    'refreshMayBeConsumed': refreshMayBeConsumed,
  };

  static String _requiredString(Map<String, Object?> json, String key) {
    final value = json[key];
    if (value is! String || value.isEmpty) {
      throw FormatException('$key is missing.');
    }
    return value;
  }
}

class TokenPair {
  const TokenPair({
    required this.accessToken,
    required this.accessTokenExpiresAtUtc,
    required this.refreshToken,
    required this.refreshTokenExpiresAtUtc,
  });

  factory TokenPair.fromJson(Map<String, Object?> json) => TokenPair(
    accessToken: _requiredString(json, 'accessToken'),
    accessTokenExpiresAtUtc: DateTime.parse(
      _requiredString(json, 'accessTokenExpiresAtUtc'),
    ).toUtc(),
    refreshToken: _requiredString(json, 'refreshToken'),
    refreshTokenExpiresAtUtc: DateTime.parse(
      _requiredString(json, 'refreshTokenExpiresAtUtc'),
    ).toUtc(),
  );

  final String accessToken;
  final DateTime accessTokenExpiresAtUtc;
  final String refreshToken;
  final DateTime refreshTokenExpiresAtUtc;

  static String _requiredString(Map<String, Object?> json, String key) {
    final value = json[key];
    if (value is! String || value.isEmpty) {
      throw FormatException('$key is missing.');
    }
    return value;
  }
}
