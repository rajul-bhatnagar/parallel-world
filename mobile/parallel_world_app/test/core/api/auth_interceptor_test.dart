import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/api/auth_interceptor.dart';

import '../../support/fakes.dart';

class FakeAccessTokenCoordinator implements AccessTokenCoordinator {
  int refreshCalls = 0;
  String currentToken = 'old-token';

  @override
  Future<String?> readAccessToken() async => currentToken;

  @override
  Future<String> refreshSingleFlight() async {
    refreshCalls++;
    return currentToken = 'new-token';
  }
}

void main() {
  test('adds bearer auth and retries one GET after a 401', () async {
    var calls = 0;
    final adapter = TestHttpClientAdapter((options, body) {
      calls++;
      final status = calls == 1 ? 401 : 200;
      return ResponseBody.fromString(
        '{}',
        status,
        headers: {
          Headers.contentTypeHeader: [Headers.jsonContentType],
        },
      );
    });
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = adapter;
    final coordinator = FakeAccessTokenCoordinator();
    dio.interceptors.add(AuthInterceptor(dio, coordinator));

    final response = await dio.get<dynamic>('/api/v1/worlds/current');

    expect(response.statusCode, 200);
    expect(calls, 2);
    expect(coordinator.refreshCalls, 1);
    expect(adapter.requests.last.headers['Authorization'], 'Bearer new-token');
  });

  test('staggered old-token 401 responses rotate refresh only once', () async {
    final bothOldRequestsArrived = Completer<void>();
    final firstRetryCompleted = Completer<void>();
    var oldRequests = 0;
    final adapter = TestHttpClientAdapter((options, body) async {
      final token = options.headers['Authorization'];
      if (token == 'Bearer old-token') {
        oldRequests++;
        if (oldRequests == 2) {
          bothOldRequestsArrived.complete();
        }
        await bothOldRequestsArrived.future;
        if (options.path.endsWith('/second')) {
          await firstRetryCompleted.future;
        }
        return ResponseBody.fromString('{}', 401);
      }

      if (options.path.endsWith('/first') && !firstRetryCompleted.isCompleted) {
        firstRetryCompleted.complete();
      }
      return ResponseBody.fromString('{}', 200);
    });
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = adapter;
    final coordinator = FakeAccessTokenCoordinator();
    dio.interceptors.add(AuthInterceptor(dio, coordinator));

    final responses = await Future.wait([
      dio.get<dynamic>('/first'),
      dio.get<dynamic>('/second'),
    ]);

    expect(responses.map((response) => response.statusCode), everyElement(200));
    expect(coordinator.refreshCalls, 1);
    expect(adapter.requests, hasLength(4));
    expect(
      adapter.requests.where(
        (request) => request.headers['Authorization'] == 'Bearer new-token',
      ),
      hasLength(2),
    );
  });

  test('does not replay an unsafe POST without an idempotency key', () async {
    final adapter = TestHttpClientAdapter(
      (options, body) => ResponseBody.fromString(
        '{}',
        401,
        headers: {
          Headers.contentTypeHeader: [Headers.jsonContentType],
        },
      ),
    );
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = adapter;
    final coordinator = FakeAccessTokenCoordinator();
    dio.interceptors.add(AuthInterceptor(dio, coordinator));

    await expectLater(
      dio.post<dynamic>('/unsafe'),
      throwsA(isA<DioException>()),
    );

    expect(adapter.requests, hasLength(1));
    expect(coordinator.refreshCalls, 0);
  });
}
