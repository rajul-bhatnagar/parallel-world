import 'package:parallel_world_app/core/api/auth_interceptor.dart';
import 'package:parallel_world_app/core/auth/session_credentials.dart';
import 'package:parallel_world_app/features/world/domain/world_summary.dart';

abstract interface class AuthGateway {
  Future<AccountResponse> register({
    required String email,
    required String password,
  });
  Future<GuestSessionResponse> login({
    required String email,
    required String password,
    required String installationId,
    required String appVersion,
  });
  Future<AccountResponse> upgrade({
    required String email,
    required String password,
    required String accessToken,
    required String idempotencyKey,
  });
  Future<String> recover({
    required String email,
    required String recoveryCode,
    required String newPassword,
  });
  Future<void> changePassword({
    required String accessToken,
    required String currentPassword,
    required String newPassword,
  });
  Future<String> regenerateRecoveryCode(String accessToken);
  Future<List<AccountSession>> listSessions(String accessToken);
  Future<void> revokeSession(String accessToken, String sessionId);
  Future<void> logoutAll(String accessToken);
  Future<GuestSessionResponse> bootstrapGuest({
    required String installationId,
    required String appVersion,
    required String bootstrapProof,
    required String worldName,
  });

  Future<TokenPair> refresh(String refreshToken);

  Future<void> logout({
    required String accessToken,
    required String refreshToken,
  });
}

abstract interface class SessionLifecycle implements AccessTokenCoordinator {
  Future<String> ensureInstallationId();

  Future<SessionCredentials?> restore();

  Future<GuestSessionResponse> bootstrapGuest();
  Future<AccountResponse> register(String email, String password);
  Future<GuestSessionResponse> login(String email, String password);
  Future<AccountResponse> upgrade(
    String email,
    String password,
    String idempotencyKey,
  );
  Future<String> recover(String email, String recoveryCode, String newPassword);
  Future<void> changePassword(String currentPassword, String newPassword);
  Future<String> regenerateRecoveryCode();
  Future<List<AccountSession>> listSessions();
  Future<void> revokeSession(String sessionId);
  Future<void> logoutAll();

  Future<void> logout();

  Future<void> clearLocalSession();
}

class AccountSession {
  const AccountSession({
    required this.id,
    required this.platform,
    required this.appVersion,
    required this.lastSeenAt,
    required this.isCurrent,
  });
  factory AccountSession.fromJson(Map<String, Object?> json) => AccountSession(
    id: json['id']! as String,
    platform: json['platform']! as String,
    appVersion: json['appVersion']! as String,
    lastSeenAt: DateTime.parse(json['lastSeenAt']! as String).toUtc(),
    isCurrent: json['isCurrent']! as bool,
  );
  final String id;
  final String platform;
  final String appVersion;
  final DateTime lastSeenAt;
  final bool isCurrent;
}

class AccountResponse {
  const AccountResponse({
    required this.userId,
    required this.world,
    this.recoveryCode,
  });
  factory AccountResponse.fromJson(Map<String, Object?> json) {
    final user = json['user'];
    final world = json['world'];
    if (user is! Map || world is! Map) {
      throw const FormatException('Account identity is missing.');
    }
    final id = user['id'];
    if (id is! String) {
      throw const FormatException('User ID is missing.');
    }
    return AccountResponse(
      userId: id,
      world: WorldSummary.fromJson(Map<String, Object?>.from(world)),
      recoveryCode: json['recoveryCode'] as String?,
    );
  }
  final String userId;
  final WorldSummary world;
  final String? recoveryCode;
}

class GuestSessionResponse {
  const GuestSessionResponse({
    required this.tokens,
    required this.userId,
    required this.world,
  });

  factory GuestSessionResponse.fromJson(Map<String, Object?> json) {
    final user = json['user'];
    final world = json['world'];
    if (user is! Map || world is! Map) {
      throw const FormatException('Guest session identity is missing.');
    }
    final userJson = <String, Object?>{
      for (final entry in user.entries)
        if (entry.key is String) entry.key as String: entry.value,
    };
    final worldJson = <String, Object?>{
      for (final entry in world.entries)
        if (entry.key is String) entry.key as String: entry.value,
    };
    final userId = userJson['id'];
    if (userId is! String || userId.isEmpty) {
      throw const FormatException('Guest user ID is missing.');
    }

    return GuestSessionResponse(
      tokens: TokenPair.fromJson(json),
      userId: userId,
      world: WorldSummary.fromJson(worldJson),
    );
  }

  final TokenPair tokens;
  final String userId;
  final WorldSummary world;
}
