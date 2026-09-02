import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/auth/secure_key_value_store.dart';
import 'package:parallel_world_app/core/auth/session_credentials.dart';
import 'package:parallel_world_app/features/session/application/session_contracts.dart';
import 'package:parallel_world_app/features/world/application/world_contracts.dart';
import 'package:parallel_world_app/features/world/domain/world_summary.dart';

final testNow = DateTime.utc(2026, 9, 2, 12);

final testWorld = WorldSummary(
  id: '00000000-0000-0000-0000-000000000101',
  name: 'My Parallel World',
  status: 'Active',
  currentGameTimeUtc: testNow,
  playerActorId: '00000000-0000-0000-0000-000000000102',
  playerDisplayName: 'Player',
  createdAtUtc: testNow,
);

SessionCredentials testCredentials({
  bool accessExpired = false,
  bool refreshExpired = false,
  bool refreshMayBeConsumed = false,
}) => SessionCredentials(
  userId: '00000000-0000-0000-0000-000000000100',
  accessToken: 'access-token',
  accessTokenExpiresAtUtc: accessExpired
      ? testNow.subtract(const Duration(minutes: 1))
      : testNow.add(const Duration(minutes: 10)),
  refreshToken: 'refresh-token',
  refreshTokenExpiresAtUtc: refreshExpired
      ? testNow.subtract(const Duration(minutes: 1))
      : testNow.add(const Duration(days: 1)),
  refreshMayBeConsumed: refreshMayBeConsumed,
);

TokenPair get testTokenPair => TokenPair(
  accessToken: 'replacement-access-token',
  accessTokenExpiresAtUtc: testNow.add(const Duration(minutes: 10)),
  refreshToken: 'replacement-refresh-token',
  refreshTokenExpiresAtUtc: testNow.add(const Duration(days: 1)),
);

GuestSessionResponse get testGuestResponse => GuestSessionResponse(
  tokens: testTokenPair,
  userId: '00000000-0000-0000-0000-000000000100',
  world: testWorld,
);

class MemorySecureKeyValueStore implements SecureKeyValueStore {
  final values = <String, String>{};
  bool failReads = false;
  bool failWrites = false;

  @override
  Future<String?> read(String key) async {
    if (failReads) {
      throw StateError('read failed');
    }
    return values[key];
  }

  @override
  Future<void> write(String key, String value) async {
    if (failWrites) {
      throw StateError('write failed');
    }
    values[key] = value;
  }

  @override
  Future<void> delete(String key) async => values.remove(key);
}

class FakeAuthGateway implements AuthGateway {
  GuestSessionResponse guestResponse = testGuestResponse;
  TokenPair refreshResponse = testTokenPair;
  Object? guestError;
  Object? refreshError;
  Object? logoutError;
  Completer<TokenPair>? refreshCompleter;
  int guestCalls = 0;
  int refreshCalls = 0;
  int logoutCalls = 0;
  String? lastProof;
  String? lastInstallationId;
  String? lastLogoutAccessToken;
  String? lastLogoutRefreshToken;
  final proofs = <String>[];
  final logoutErrors = <Object?>[];

  @override
  Future<GuestSessionResponse> bootstrapGuest({
    required String installationId,
    required String appVersion,
    required String bootstrapProof,
    required String worldName,
  }) async {
    guestCalls++;
    lastProof = bootstrapProof;
    lastInstallationId = installationId;
    proofs.add(bootstrapProof);
    if (guestError case final error?) {
      throw error;
    }
    return guestResponse;
  }

  @override
  Future<TokenPair> refresh(String refreshToken) async {
    refreshCalls++;
    if (refreshError case final error?) {
      throw error;
    }
    return refreshCompleter?.future ?? refreshResponse;
  }

  @override
  Future<void> logout({
    required String accessToken,
    required String refreshToken,
  }) async {
    logoutCalls++;
    lastLogoutAccessToken = accessToken;
    lastLogoutRefreshToken = refreshToken;
    if (logoutErrors.isNotEmpty) {
      final error = logoutErrors.removeAt(0);
      if (error != null) {
        throw error;
      }
    }
    if (logoutError case final error?) {
      throw error;
    }
  }
}

class FakeSessionLifecycle implements SessionLifecycle {
  SessionCredentials? restored;
  GuestSessionResponse guestResponse = testGuestResponse;
  Object? restoreError;
  Object? guestError;
  Object? refreshError;
  Object? logoutError;
  int refreshCalls = 0;
  int clearCalls = 0;
  int installationCalls = 0;
  int guestCalls = 0;

  @override
  Future<String> ensureInstallationId() async {
    installationCalls++;
    return '00000000-0000-4000-8000-000000000001';
  }

  @override
  Future<SessionCredentials?> restore() async {
    if (restoreError case final error?) {
      throw error;
    }
    return restored;
  }

  @override
  Future<GuestSessionResponse> bootstrapGuest() async {
    guestCalls++;
    if (guestError case final error?) {
      throw error;
    }
    return guestResponse;
  }

  @override
  Future<String?> readAccessToken() async => restored?.accessToken;

  @override
  Future<String> refreshSingleFlight() async {
    refreshCalls++;
    if (refreshError case final error?) {
      throw error;
    }
    return 'replacement-access-token';
  }

  @override
  Future<void> logout() async {
    if (logoutError case final error?) {
      throw error;
    }
  }

  @override
  Future<void> clearLocalSession() async {
    clearCalls++;
    restored = null;
  }
}

class FakeWorldGateway implements WorldGateway {
  WorldSummary current = testWorld;
  Object? currentError;
  Object? createError;
  int currentCalls = 0;
  int createCalls = 0;
  String? lastIdempotencyKey;

  @override
  Future<WorldSummary> getCurrent() async {
    currentCalls++;
    if (currentError case final error?) {
      throw error;
    }
    return current;
  }

  @override
  Future<WorldSummary> create({
    required String name,
    required String idempotencyKey,
  }) async {
    createCalls++;
    lastIdempotencyKey = idempotencyKey;
    if (createError case final error?) {
      throw error;
    }
    return current;
  }
}

class FakeWorldCache implements WorldCache {
  WorldSummary? value;
  bool initialized = false;
  int clearCalls = 0;

  @override
  Future<void> initialize() async => initialized = true;

  @override
  Future<WorldSummary?> read(String userId) async => value;

  @override
  Future<void> write(String userId, WorldSummary world) async => value = world;

  @override
  Future<void> clear() async {
    clearCalls++;
    value = null;
  }
}

typedef AdapterHandler = FutureOr<ResponseBody> Function(
  RequestOptions options,
  Uint8List? requestBody,
);

class TestHttpClientAdapter implements HttpClientAdapter {
  TestHttpClientAdapter(this.handler);

  final AdapterHandler handler;
  final requests = <RequestOptions>[];

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    requests.add(options);
    final chunks = requestStream == null
        ? <Uint8List>[]
        : await requestStream.toList();
    final body = chunks.isEmpty
        ? null
        : Uint8List.fromList(chunks.expand((chunk) => chunk).toList());
    return handler(options, body);
  }

  @override
  void close({bool force = false}) {}
}
