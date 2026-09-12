import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/features/characters/application/character_contracts.dart';
import 'package:parallel_world_app/features/characters/domain/character_models.dart';

class CharacterApi implements CharacterGateway {
  CharacterApi(this._dio);

  final Dio _dio;

  @override
  Future<CharacterPage> list({
    required String worldId,
    int limit = 20,
    String? cursor,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/characters',
        queryParameters: {'limit': limit, 'cursor': ?cursor},
      );
      return CharacterPage.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<CharacterDetails> get({
    required String worldId,
    required String characterId,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/characters/$characterId',
      );
      return CharacterDetails.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  static Map<String, Object?> _jsonObject(Object? value) {
    if (value is! Map) {
      throw const FormatException('Expected a JSON object.');
    }
    return <String, Object?>{
      for (final entry in value.entries)
        if (entry.key is String) entry.key as String: entry.value,
    };
  }
}
