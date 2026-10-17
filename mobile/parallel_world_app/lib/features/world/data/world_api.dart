import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/features/world/application/world_contracts.dart';
import 'package:parallel_world_app/features/world/domain/world_summary.dart';

class WorldApi implements WorldGateway {
  WorldApi(this._dio);

  final Dio _dio;

  @override
  Future<WorldSummary> getCurrent() async {
    try {
      final response = await _dio.get<dynamic>('/api/v1/worlds/current');
      return WorldSummary.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    } on FormatException {
      rethrow;
    }
  }

  @override
  Future<WorldSummary> create({
    required String name,
    required String idempotencyKey,
  }) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/worlds',
        data: {'name': name},
        options: Options(headers: {'Idempotency-Key': idempotencyKey}),
      );
      return WorldSummary.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    } on FormatException {
      rethrow;
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
