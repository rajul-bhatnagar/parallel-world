import 'package:dio/dio.dart';
import 'package:parallel_world_app/core/api/problem_details.dart';
import 'package:parallel_world_app/core/auth/secret_generator.dart';
import 'package:parallel_world_app/core/config/app_config.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/core/logging/safe_logger.dart';

Dio createApiDio({
  required AppConfig config,
  required SecretGenerator secretGenerator,
  required SafeLogSink logger,
}) {
  final dio = Dio(
    BaseOptions(
      baseUrl: config.apiBaseUrl,
      connectTimeout: config.connectTimeout,
      sendTimeout: config.sendTimeout,
      receiveTimeout: config.receiveTimeout,
      contentType: Headers.jsonContentType,
      responseType: ResponseType.json,
      headers: const {'Accept': 'application/json'},
    ),
  );
  dio.interceptors.addAll([
    CorrelationInterceptor(secretGenerator),
    SanitizedNetworkLogInterceptor(logger),
  ]);
  return dio;
}

AppFailure mapDioException(DioException exception) {
  final response = exception.response;
  if (response != null) {
    final data = response.data;
    if (data is Map) {
      final json = <String, Object?>{};
      for (final entry in data.entries) {
        if (entry.key is String) {
          json[entry.key as String] = entry.value;
        }
      }
      return ApiProblemDetails.fromJson(
        json,
        responseStatus: response.statusCode,
      ).toFailure();
    }

    if ((response.statusCode ?? 500) >= 500) {
      return const ServerFailure();
    }
  }

  return switch (exception.type) {
    DioExceptionType.connectionTimeout ||
    DioExceptionType.sendTimeout ||
    DioExceptionType.receiveTimeout ||
    DioExceptionType.connectionError => const NetworkFailure(),
    _ => const UnknownFailure(),
  };
}

class CorrelationInterceptor extends Interceptor {
  CorrelationInterceptor(this._secretGenerator);

  final SecretGenerator _secretGenerator;

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    options.headers.putIfAbsent(
      'X-Correlation-ID',
      _secretGenerator.newCorrelationId,
    );
    handler.next(options);
  }
}

class SanitizedNetworkLogInterceptor extends Interceptor {
  SanitizedNetworkLogInterceptor(this._logger);

  static const _startedAtKey = 'safeLogStartedAt';
  final SafeLogSink _logger;

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    options.extra[_startedAtKey] = DateTime.now();
    handler.next(options);
  }

  @override
  void onResponse(
    Response<dynamic> response,
    ResponseInterceptorHandler handler,
  ) {
    _record(response.requestOptions, response.statusCode);
    handler.next(response);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    _record(err.requestOptions, err.response?.statusCode);
    handler.next(err);
  }

  void _record(RequestOptions options, int? statusCode) {
    final startedAt = options.extra[_startedAtKey];
    final duration = startedAt is DateTime
        ? DateTime.now().difference(startedAt)
        : Duration.zero;
    _logger.record(
      SafeLogEvent(
        method: options.method,
        route: options.uri.path,
        statusCode: statusCode,
        duration: duration,
        correlationId: options.headers['X-Correlation-ID'] as String?,
      ),
    );
  }
}
