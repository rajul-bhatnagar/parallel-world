import 'package:parallel_world_app/core/api/auth_interceptor.dart';
import 'package:parallel_world_app/core/auth/session_credentials.dart';
import 'package:parallel_world_app/features/world/domain/world_summary.dart';

abstract interface class AuthGateway {
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

  Future<void> logout();

  Future<void> clearLocalSession();
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
