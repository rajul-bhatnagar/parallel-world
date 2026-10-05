import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/relationships/data/relationship_api.dart';

import '../../support/fakes.dart';

void main() {
  test('invitation sends only an empty body and idempotency key', () async {
    late RequestOptions captured;
    late Map<String, Object?> body;
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, bytes) {
        captured = options;
        body = Map<String, Object?>.from(
          jsonDecode(utf8.decode(bytes!)) as Map,
        );
        return _json(_invitationJson, statusCode: 201);
      });

    final invitation = await RelationshipApi(dio).invite(
      worldId: testWorld.id,
      characterId: _characterId,
      idempotencyKey: 'stable-operation-key',
    );

    expect(
      captured.path,
      '/api/v1/worlds/${testWorld.id}/relationships/$_characterId/date-invitations',
    );
    expect(captured.headers['Idempotency-Key'], 'stable-operation-key');
    expect(body, isEmpty);
    expect(invitation.dateType, 'CasualDate');
    expect(invitation.romanticStatus, 'dating');
  });

  test('player outcome sends only the explicit decision', () async {
    late Map<String, Object?> body;
    final dio = Dio(BaseOptions(baseUrl: 'https://api.example.test'))
      ..httpClientAdapter = TestHttpClientAdapter((options, bytes) {
        body = Map<String, Object?>.from(
          jsonDecode(utf8.decode(bytes!)) as Map,
        );
        return _json({..._invitationJson, 'status': 'accepted'});
      });

    await RelationshipApi(dio).resolve(
      worldId: testWorld.id,
      invitationId: _invitationId,
      decision: 'accept',
    );

    expect(body, {'decision': 'accept'});
    expect(body, isNot(contains('score')));
    expect(body, isNot(contains('commitment')));
  });
}

const _characterId = '00000000-0000-0000-0000-000000000201';
const _invitationId = '00000000-0000-0000-0000-000000000601';
const _invitationJson = {
  'id': _invitationId,
  'episodeId': '00000000-0000-0000-0000-000000000602',
  'characterId': _characterId,
  'initiatorActorId': '00000000-0000-0000-0000-000000000102',
  'targetActorId': '00000000-0000-0000-0000-000000000202',
  'dateType': 'CasualDate',
  'status': 'accepted',
  'romanticStatus': 'dating',
  'reasonCode': 'character_accepted',
  'createdAtUtc': '2026-10-04T12:00:00Z',
  'createdAtWorldTime': '2026-10-04T12:00:00Z',
  'expiresAtWorldTime': '2026-10-05T12:00:00Z',
  'resolvedAtUtc': '2026-10-04T12:00:00Z',
  'resolvedAtWorldTime': '2026-10-04T12:00:00Z',
};

ResponseBody _json(Object value, {int statusCode = 200}) =>
    ResponseBody.fromString(
      jsonEncode(value),
      statusCode,
      headers: {
        Headers.contentTypeHeader: [Headers.jsonContentType],
      },
    );
