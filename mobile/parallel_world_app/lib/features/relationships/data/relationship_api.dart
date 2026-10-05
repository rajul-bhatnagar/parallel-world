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

  @override
  Future<DatingInvitation> invite({
    required String worldId,
    required String characterId,
    required String idempotencyKey,
  }) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/worlds/$worldId/relationships/$characterId/date-invitations',
        data: const <String, Object?>{},
        options: Options(headers: {'Idempotency-Key': idempotencyKey}),
      );
      return DatingInvitation.fromJson(_object(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<List<DatingInvitation>> invitations({required String worldId}) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/date-invitations',
      );
      return _items(response.data)
          .map((item) => DatingInvitation.fromJson(_object(item)))
          .toList(growable: false);
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<List<RomanticHistoryItem>> romanticHistory({
    required String worldId,
    required String characterId,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/relationships/$characterId/romantic-history',
      );
      return _items(response.data)
          .map((item) => RomanticHistoryItem.fromJson(_object(item)))
          .toList(growable: false);
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<DatingInvitation> resolve({
    required String worldId,
    required String invitationId,
    required String decision,
  }) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/worlds/$worldId/date-invitations/$invitationId/outcome',
        data: {'decision': decision},
      );
      return DatingInvitation.fromJson(_object(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  static List<Object?> _items(Object? value) {
    final items = _object(value)['items'];
    if (items is! List) throw const FormatException('Expected items.');
    return items;
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
