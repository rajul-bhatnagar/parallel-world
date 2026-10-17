import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/characters/data/character_api.dart';

import '../../support/character_fakes.dart';
import '../../support/fakes.dart';

void main() {
  test('maps the catalogue cursor contract without decoding it', () async {
    late RequestOptions captured;
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, body) {
        captured = options;
        return _jsonResponse({
          'items': [_summaryJson],
          'nextCursor': 'opaque-server-value',
          'hasMore': true,
        });
      });

    final page = await CharacterApi(dio)
        .list(worldId: testWorld.id, limit: 3, cursor: 'prior-opaque-value');

    expect(captured.path, '/api/v1/worlds/${testWorld.id}/characters');
    expect(captured.queryParameters, {
      'limit': 3,
      'cursor': 'prior-opaque-value',
    });
    expect(page.items.single.id, testCharacter.id);
    expect(page.nextCursor, 'opaque-server-value');
    expect(page.hasMore, isTrue);
  });

  test('maps only the approved character detail projection', () async {
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter(
        (options, body) => _jsonResponse({
          ..._summaryJson,
          'bio': testCharacterDetails.bio,
          'age': 29,
          'archetype': 'Creative observer',
          'interests': [
            {'topicId': 'design'},
          ],
          'schedule': [
            {
              'dayOfWeek': 1,
              'startLocalTime': '09:00',
              'endLocalTime': '17:00',
              'activity': 'Studio work',
            },
          ],
        }),
      );

    final details = await CharacterApi(dio)
        .get(worldId: testWorld.id, characterId: testCharacter.id);

    expect(details.interests.single.topicId, 'design');
    expect(details.schedule.single.activity, 'Studio work');
  });
}

const _summaryJson = {
  'id': '00000000-0000-0000-0000-000000000201',
  'displayName': 'Maya Chen',
  'handle': 'maya',
  'profession': 'Designer',
  'visibleMood': 'inspired',
  'isFollowed': false,
};

ResponseBody _jsonResponse(Object value) => ResponseBody.fromString(
  jsonEncode(value),
  200,
  headers: {
    Headers.contentTypeHeader: [Headers.jsonContentType],
  },
);
