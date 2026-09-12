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

Widget _app(Widget child, FakeCharacterRepository repository) => ProviderScope(
  key: UniqueKey(),
  overrides: [
    sessionControllerProvider.overrideWith(_AuthenticatedSessionController.new),
    characterRepositoryProvider.overrideWithValue(repository),
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
}
