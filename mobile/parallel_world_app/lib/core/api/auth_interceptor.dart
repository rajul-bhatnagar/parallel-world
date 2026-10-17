import 'package:dio/dio.dart';

abstract interface class AccessTokenCoordinator {
  Future<String?> readAccessToken();

  Future<String> refreshSingleFlight();
}

class AuthInterceptor extends Interceptor {
  AuthInterceptor(this._dio, this._coordinator);

  static const _retriedKey = 'authRetried';
  final Dio _dio;
  final AccessTokenCoordinator _coordinator;

  @override
  void onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final accessToken = await _coordinator.readAccessToken();
    if (accessToken != null) {
      options.headers.putIfAbsent('Authorization', () => 'Bearer $accessToken');
    }
    handler.next(options);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    final request = err.requestOptions;
    if (err.response?.statusCode != 401 ||
        request.extra[_retriedKey] == true ||
        !_isSafeToRetry(request)) {
      handler.next(err);
      return;
    }

    try {
      final requestAccessToken = _bearerToken(request);
      final currentAccessToken = await _coordinator.readAccessToken();
      final accessToken =
          currentAccessToken != null &&
              requestAccessToken != null &&
              currentAccessToken != requestAccessToken
          ? currentAccessToken
          : await _coordinator.refreshSingleFlight();
      final response = await _dio.fetch<dynamic>(
        request.copyWith(
          headers: {...request.headers, 'Authorization': 'Bearer $accessToken'},
          extra: {...request.extra, _retriedKey: true},
        ),
      );
      handler.resolve(response);
    } catch (_) {
      handler.next(err);
    }
  }

  static bool _isSafeToRetry(RequestOptions request) =>
      request.method == 'GET' || request.headers.containsKey('Idempotency-Key');

  static String? _bearerToken(RequestOptions request) {
    final authorization = request.headers['Authorization'];
    if (authorization is! String || !authorization.startsWith('Bearer ')) {
      return null;
    }
    return authorization.substring('Bearer '.length);
  }
}
