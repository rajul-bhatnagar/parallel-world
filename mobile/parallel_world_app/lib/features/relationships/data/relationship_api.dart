import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/features/relationships/application/relationship_contracts.dart';
import 'package:parallel_world_app/features/relationships/domain/relationship_models.dart';

class RelationshipApi implements RelationshipGateway {
  RelationshipApi(this._dio);

  final Dio _dio;

  @override
  Future<RelationshipSummary?> get({
    required String worldId,
    required String actorId,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/relationships/$actorId',
      );
      return RelationshipSummary.fromJson(_object(response.data));
    } on DioException catch (error) {
      if (error.response?.statusCode == 404) return null;
      throw mapDioException(error);
    }
  }

  @override
  Future<List<RelationshipHistoryItem>> history({
    required String worldId,
    required String actorId,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/relationships/$actorId/history',
      );
      final root = _object(response.data);
      final items = root['items'];
      if (items is! List) {
        throw const FormatException('Expected relationship history items.');
      }
      return items
          .map((item) => RelationshipHistoryItem.fromJson(_object(item)))
          .toList(growable: false);
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  static Map<String, Object?> _object(Object? value) {
    if (value is! Map) {
      throw const FormatException('Expected a JSON object.');
    }
    return <String, Object?>{
      for (final entry in value.entries)
        if (entry.key is String) entry.key as String: entry.value,
    };
  }
}
