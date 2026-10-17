import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/features/catch_up/application/catch_up_contracts.dart';
import 'package:parallel_world_app/features/catch_up/domain/catch_up_models.dart';

class CatchUpApi implements CatchUpGateway {
  CatchUpApi(this._dio);

  final Dio _dio;

  @override
  Future<CatchUpResult> process({required String worldId}) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/worlds/$worldId/catch-up',
      );
      return CatchUpResult.fromJson(_object(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<CatchUpSummary?> latest({required String worldId}) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/catch-up/latest',
      );
      if (response.statusCode == 204 || response.data == null) return null;
      return CatchUpSummary.fromJson(_object(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  static Map<String, Object?> _object(Object? value) {
    if (value is! Map) throw const FormatException('Expected a JSON object.');
    return <String, Object?>{
      for (final entry in value.entries)
        if (entry.key is String) entry.key as String: entry.value,
    };
  }
}
