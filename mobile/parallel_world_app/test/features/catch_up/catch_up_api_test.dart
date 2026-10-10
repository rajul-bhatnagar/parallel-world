import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/catch_up/data/catch_up_api.dart';

import '../../support/fakes.dart';

void main() {
  test('process uses the authoritative route without a client range', () async {
    late RequestOptions captured;
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, body) {
        captured = options;
        return _json(_resultJson);
      });

    final result = await CatchUpApi(dio).process(worldId: testWorld.id);

    expect(captured.method, 'POST');
    expect(captured.path, '/api/v1/worlds/${testWorld.id}/catch-up');
    expect(captured.queryParameters, isEmpty);
    expect(captured.data, isNull);
    expect(result.remainingIntervals, 4);
    expect(result.summary?.items.single.wording, 'Ava followed Rowan.');
  });

  test('latest accepts the authoritative empty response', () async {
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter(
        (options, body) => ResponseBody.fromString('', 204),
      );

    expect(await CatchUpApi(dio).latest(worldId: testWorld.id), isNull);
  });
}

const _resultJson = {
  'disposition': 'partial',
  'processedIntervals': 8,
  'remainingIntervals': 4,
  'summary': {
    'status': 'Partial',
    'text': 'Your world moved forward while you were away.',
    'items': [
      {'itemType': 'follow', 'wording': 'Ava followed Rowan.'},
    ],
  },
  'errorCode': null,
};

ResponseBody _json(Object value) => ResponseBody.fromString(
  jsonEncode(value),
  202,
  headers: {
    Headers.contentTypeHeader: [Headers.jsonContentType],
  },
);
