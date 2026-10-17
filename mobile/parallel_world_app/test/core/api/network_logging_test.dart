import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/core/logging/safe_logger.dart';

import '../../support/fakes.dart';

class CapturingLogSink implements SafeLogSink {
  final events = <SafeLogEvent>[];

  @override
  void record(SafeLogEvent event) => events.add(event);
}

void main() {
  test(
    'network diagnostics expose metadata but never request secrets',
    () async {
      final sink = CapturingLogSink();
      final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
        ..httpClientAdapter = TestHttpClientAdapter(
          (options, body) => ResponseBody.fromString(
            '{}',
            200,
            headers: {
              Headers.contentTypeHeader: [Headers.jsonContentType],
            },
          ),
        )
        ..interceptors.add(SanitizedNetworkLogInterceptor(sink));

      await dio.post<dynamic>(
        '/api/v1/auth/refresh',
        data: {
          'accessToken': 'access-secret',
          'refreshToken': 'refresh-secret',
          'guestBootstrapProof': 'proof-secret',
        },
        options: Options(
          headers: {
            'Authorization': 'Bearer authorization-secret',
            'X-Correlation-ID': 'correlation-123',
          },
        ),
      );

      expect(sink.events, hasLength(1));
      final event = sink.events.single;
      expect(event.method, 'POST');
      expect(event.route, '/api/v1/auth/refresh');
      expect(event.statusCode, 200);
      final rendered = event.render();
      expect(rendered, contains('POST'));
      expect(rendered, contains('/api/v1/auth/refresh'));
      expect(rendered, contains('200'));
      expect(rendered, contains('ms'));
      expect(rendered, contains('correlation-123'));
      for (final secret in [
        'authorization-secret',
        'access-secret',
        'refresh-secret',
        'proof-secret',
        'Authorization',
        'guestBootstrapProof',
      ]) {
        expect(rendered, isNot(contains(secret)));
      }
    },
  );
}
