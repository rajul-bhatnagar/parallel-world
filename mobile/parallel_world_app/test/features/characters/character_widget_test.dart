import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/characters/application/character_dependencies.dart';
import 'package:parallel_world_app/features/characters/application/character_contracts.dart';
import 'package:parallel_world_app/features/characters/domain/character_models.dart';
import 'package:parallel_world_app/features/characters/presentation/character_catalogue_screen.dart';
import 'package:parallel_world_app/features/characters/presentation/character_profile_screen.dart';
import 'package:parallel_world_app/features/relationships/application/relationship_contracts.dart';
import 'package:parallel_world_app/features/relationships/application/relationship_provider.dart';
import 'package:parallel_world_app/features/relationships/domain/relationship_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

import '../../support/character_fakes.dart';
import '../../support/fakes.dart';

class _AuthenticatedSessionController extends SessionController {
  @override
  SessionState build() => SessionState(
    phase: SessionPhase.authenticated,
    userId: 'user-a',
    world: testWorld,
  );
}

Widget _app(
  Widget child,
  FakeCharacterRepository repository, {
  RelationshipGateway? relationships,
}) => ProviderScope(
  key: UniqueKey(),
  overrides: [
    sessionControllerProvider.overrideWith(_AuthenticatedSessionController.new),
    characterRepositoryProvider.overrideWithValue(repository),
    if (relationships != null)
      relationshipGatewayProvider.overrideWithValue(relationships),
  ],
  child: MaterialApp(home: child),
);

void main() {
  testWidgets('catalogue shows an accessible initial loading state', (
    tester,
  ) async {
    final completer = Completer<CharacterPage>();
    final repository = FakeCharacterRepository()
      ..pendingPage = completer.future;

    await tester.pumpWidget(_app(const CharacterCatalogueScreen(), repository));
    await tester.pump();

    expect(find.bySemanticsLabel('Loading characters'), findsOneWidget);
  });

  testWidgets('catalogue shows populated, empty, and error states', (
    tester,
  ) async {
    final repository = FakeCharacterRepository();
    await tester.pumpWidget(_app(const CharacterCatalogueScreen(), repository));
    await tester.pumpAndSettle();
    expect(find.text('Maya Chen'), findsOneWidget);
    expect(find.textContaining('Designer'), findsOneWidget);

    repository.page = const CharacterPage(
      items: [],
      nextCursor: null,
      hasMore: false,
    );
    await tester.pumpWidget(_app(const CharacterCatalogueScreen(), repository));
    await tester.pumpAndSettle();
    expect(find.textContaining('No characters'), findsOneWidget);

    repository.catalogueError = const ServerFailure();
    await tester.pumpWidget(_app(const CharacterCatalogueScreen(), repository));
    await tester.pumpAndSettle();
    expect(find.text('Retry'), findsOneWidget);
  });

  testWidgets('catalogue keeps cached summaries visible offline', (
    tester,
  ) async {
    final repository = FakeCharacterRepository()
      ..catalogueCache = CachedCharacterCatalogue([testCharacter], testNow)
      ..catalogueError = const NetworkFailure();

    await tester.pumpWidget(_app(const CharacterCatalogueScreen(), repository));
    await tester.pumpAndSettle();

    expect(find.text('Offline saved profiles'), findsOneWidget);
    expect(find.text('Maya Chen'), findsOneWidget);
  });

  testWidgets('profile shows approved fields and an offline cache indicator', (
    tester,
  ) async {
    final repository = FakeCharacterRepository()
      ..detailsCache = CachedCharacterDetails(testCharacterDetails, testNow)
      ..detailsError = const NetworkFailure();

    await tester.pumpWidget(
      _app(CharacterProfileScreen(characterId: testCharacter.id), repository),
    );
    await tester.pumpAndSettle();

    expect(find.text('Offline saved profile'), findsOneWidget);
    expect(find.text('Maya Chen'), findsOneWidget);
    expect(find.text('Interests'), findsOneWidget);
    expect(find.text('Design'), findsOneWidget);
    expect(find.text('Schedule'), findsOneWidget);
    expect(find.text('Studio work'), findsOneWidget);
    expect(find.textContaining('influence'), findsNothing);
    expect(find.textContaining('opinion'), findsNothing);
  });

  testWidgets('profile maps an ownership-safe resource failure', (
    tester,
  ) async {
    final repository = FakeCharacterRepository()
      ..detailsError = const NotFoundFailure();

    await tester.pumpWidget(
      _app(
        const CharacterProfileScreen(characterId: 'foreign-character'),
        repository,
      ),
    );
    await tester.pumpAndSettle();

    expect(
      find.text('The requested resource is not available.'),
      findsOneWidget,
    );
    expect(find.text('Retry'), findsOneWidget);
  });

  testWidgets('profile exposes one server-authoritative casual-date action', (
    tester,
  ) async {
    final relationships = _FakeRelationshipGateway();
    await tester.pumpWidget(
      _app(
        CharacterProfileScreen(characterId: testCharacter.id),
        FakeCharacterRepository(),
        relationships: relationships,
      ),
    );
    await tester.pumpAndSettle();

    await tester.scrollUntilVisible(
      find.text('Invite on a casual date'),
      300,
      scrollable: find.byType(Scrollable).first,
    );
    expect(find.text('Invite on a casual date'), findsOneWidget);
    expect(find.textContaining('score'), findsNothing);
    await tester.tap(find.text('Invite on a casual date'));
    await tester.pumpAndSettle();
    expect(relationships.inviteCalls, 1);
  });
}

class _FakeRelationshipGateway implements RelationshipGateway {
  int inviteCalls = 0;

  @override
  Future<RelationshipSummary?> get({
    required String worldId,
    required String actorId,
  }) async => null;

  @override
  Future<List<RelationshipHistoryItem>> history({
    required String worldId,
    required String actorId,
  }) async => [];

  @override
  Future<List<DatingInvitation>> invitations({required String worldId}) async =>
      [];

  @override
  Future<List<RomanticHistoryItem>> romanticHistory({
    required String worldId,
    required String characterId,
  }) async => [];

  @override
  Future<DatingInvitation> invite({
    required String worldId,
    required String characterId,
    required String idempotencyKey,
  }) async {
    inviteCalls++;
    return _invitation;
  }

  @override
  Future<DatingInvitation> resolve({
    required String worldId,
    required String invitationId,
    required String decision,
  }) async => _invitation;
}

final _invitation = DatingInvitation(
  id: 'invitation-1',
  episodeId: 'episode-1',
  characterId: testCharacter.id,
  initiatorActorId: testWorld.playerActorId,
  targetActorId: 'character-actor-1',
  dateType: 'CasualDate',
  status: 'accepted',
  romanticStatus: 'dating',
  reasonCode: 'character_accepted',
  createdAtWorldTime: testNow,
  expiresAtWorldTime: testNow.add(const Duration(hours: 24)),
);
