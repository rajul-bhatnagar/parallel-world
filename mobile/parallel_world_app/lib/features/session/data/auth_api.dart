import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/core/auth/session_credentials.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/session/application/session_contracts.dart';

class AuthApi implements AuthGateway {
  AuthApi(this._dio);

  final Dio _dio;

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
