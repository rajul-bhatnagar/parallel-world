import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/core/auth/session_credentials.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/session/application/session_contracts.dart';

class AuthApi implements AuthGateway {
  AuthApi(this._dio);

  final Dio _dio;

  @override
  Future<AccountResponse> register({
    required String email,
    required String password,
  }) =>
      _account('/api/v1/auth/register', {'email': email, 'password': password});

  @override
  Future<GuestSessionResponse> login({
    required String email,
    required String password,
    required String installationId,
    required String appVersion,
  }) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/auth/login',
        data: {
          'email': email,
          'password': password,
          'installationId': installationId,
          'platform': 'android',
          'appVersion': appVersion,
        },
      );
      return GuestSessionResponse.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    } on FormatException {
      throw const UnknownFailure();
    }
  }

  @override
  Future<AccountResponse> upgrade({
    required String email,
    required String password,
    required String accessToken,
    required String idempotencyKey,
  }) => _account(
    '/api/v1/auth/upgrade',
    {'email': email, 'password': password},
    options: Options(
      headers: {
        'Authorization': 'Bearer $accessToken',
        'Idempotency-Key': idempotencyKey,
      },
    ),
  );

  @override
  Future<String> recover({
    required String email,
    required String recoveryCode,
    required String newPassword,
  }) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/auth/recovery',
        data: {
          'email': email,
          'recoveryCode': recoveryCode,
          'newPassword': newPassword,
        },
      );
      final code = _jsonObject(response.data)['recoveryCode'];
      if (code is! String) throw const FormatException();
      return code;
    } on DioException catch (error) {
      throw mapDioException(error);
    } on FormatException {
      throw const UnknownFailure();
    }
  }

  @override
  Future<void> changePassword({
    required String accessToken,
    required String currentPassword,
    required String newPassword,
  }) => _authorizedPost('/api/v1/auth/password/change', accessToken, {
    'currentPassword': currentPassword,
    'newPassword': newPassword,
  });

  @override
  Future<String> regenerateRecoveryCode(String accessToken) async {
    final data = await _authorizedJson(
      '/api/v1/auth/recovery-code/regenerate',
      accessToken,
    );
    final code = data['recoveryCode'];
    if (code is! String) throw const UnknownFailure();
    return code;
  }

  @override
  Future<List<AccountSession>> listSessions(String accessToken) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/auth/sessions',
        options: Options(headers: {'Authorization': 'Bearer $accessToken'}),
      );
      final items = _jsonObject(response.data)['items'];
      if (items is! List) throw const FormatException();
      return items
          .map(
            (item) =>
                AccountSession.fromJson(Map<String, Object?>.from(item as Map)),
          )
          .toList();
    } on DioException catch (error) {
      throw mapDioException(error);
    } on FormatException {
      throw const UnknownFailure();
    }
  }

  @override
  Future<void> revokeSession(String accessToken, String sessionId) async {
    try {
      await _dio.delete<void>(
        '/api/v1/auth/sessions/$sessionId',
        options: Options(headers: {'Authorization': 'Bearer $accessToken'}),
      );
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<void> logoutAll(String accessToken) =>
      _authorizedPost('/api/v1/auth/logout-all', accessToken, null);

  Future<void> _authorizedPost(String path, String token, Object? data) async {
    try {
      await _dio.post<void>(
        path,
        data: data,
        options: Options(headers: {'Authorization': 'Bearer $token'}),
      );
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  Future<Map<String, Object?>> _authorizedJson(
    String path,
    String token,
  ) async {
    try {
      final response = await _dio.post<dynamic>(
        path,
        options: Options(headers: {'Authorization': 'Bearer $token'}),
      );
      return _jsonObject(response.data);
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  Future<AccountResponse> _account(
    String path,
    Map<String, Object?> data, {
    Options? options,
  }) async {
    try {
      final response = await _dio.post<dynamic>(
        path,
        data: data,
        options: options,
      );
      return AccountResponse.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    } on FormatException {
      throw const UnknownFailure();
    }
  }

  @override
  Future<GuestSessionResponse> bootstrapGuest({
    required String installationId,
    required String appVersion,
    required String bootstrapProof,
    required String worldName,
  }) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/auth/guest',
        data: {
          'installationId': installationId,
          'platform': 'android',
          'appVersion': appVersion,
          'guestBootstrapProof': bootstrapProof,
          'worldName': worldName,
        },
      );
      return GuestSessionResponse.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    } on FormatException {
      throw const UnknownFailure();
    }
  }

  @override
  Future<TokenPair> refresh(String refreshToken) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/auth/refresh',
        data: {'refreshToken': refreshToken},
      );
      return TokenPair.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    } on FormatException {
      throw const UnknownFailure();
    }
  }

  @override
  Future<void> logout({
    required String accessToken,
    required String refreshToken,
  }) async {
    try {
      await _dio.post<void>(
        '/api/v1/auth/logout',
        data: {'refreshToken': refreshToken},
        options: Options(headers: {'Authorization': 'Bearer $accessToken'}),
      );
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  static Map<String, Object?> _jsonObject(Object? data) {
    if (data is! Map) {
      throw const FormatException('Expected a JSON object.');
    }
    return <String, Object?>{
      for (final entry in data.entries)
        if (entry.key is String) entry.key as String: entry.value,
    };
  }
}
