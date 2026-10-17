import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/features/feed/application/feed_contracts.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';

class FeedApi implements FeedGateway {
  FeedApi(this._dio);

  final Dio _dio;

  @override
  Future<FeedPage> getFeed({
    required String worldId,
    int limit = 20,
    String? cursor,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/feed',
        queryParameters: {'limit': limit, 'cursor': ?cursor},
      );
      return FeedPage.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<FeedPost> createPost({
    required String worldId,
    required String content,
    required String clientPostId,
    required String idempotencyKey,
  }) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/worlds/$worldId/posts',
        data: {'content': content, 'clientPostId': clientPostId},
        options: Options(headers: {'Idempotency-Key': idempotencyKey}),
      );
      return FeedPost.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<FeedPost> getPost({
    required String worldId,
    required String postId,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/posts/$postId',
      );
      return FeedPost.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<FeedPage> getReplies({
    required String worldId,
    required String parentPostId,
    int limit = 20,
    String? cursor,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        '/api/v1/worlds/$worldId/posts/$parentPostId/replies',
        queryParameters: {'limit': limit, 'cursor': ?cursor},
      );
      return FeedPage.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<FeedPost> createReply({
    required String worldId,
    required String parentPostId,
    required String content,
    required String clientPostId,
    required String idempotencyKey,
  }) async {
    try {
      final response = await _dio.post<dynamic>(
        '/api/v1/worlds/$worldId/posts/$parentPostId/replies',
        data: {'content': content, 'clientPostId': clientPostId},
        options: Options(headers: {'Idempotency-Key': idempotencyKey}),
      );
      return FeedPost.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<ReactionState> setLike({
    required String worldId,
    required String postId,
  }) async {
    try {
      final response = await _dio.put<dynamic>(
        '/api/v1/worlds/$worldId/posts/$postId/reaction',
        data: {'type': 'like'},
      );
      return ReactionState.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<void> removeLike({
    required String worldId,
    required String postId,
  }) async {
    try {
      await _dio.delete<void>('/api/v1/worlds/$worldId/posts/$postId/reaction');
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<FollowState> follow({
    required String worldId,
    required String actorId,
  }) async {
    try {
      final response = await _dio.put<dynamic>(
        '/api/v1/worlds/$worldId/actors/$actorId/follow',
      );
      return FollowState.fromJson(_jsonObject(response.data));
    } on DioException catch (error) {
      throw mapDioException(error);
    }
  }

  @override
  Future<void> unfollow({
    required String worldId,
    required String actorId,
  }) async {
    try {
      await _dio.delete<void>('/api/v1/worlds/$worldId/actors/$actorId/follow');
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
