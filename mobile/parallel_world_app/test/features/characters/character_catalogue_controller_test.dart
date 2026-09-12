import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/characters/application/character_catalogue_controller.dart';
import 'package:parallel_world_app/features/characters/application/character_contracts.dart';
import 'package:parallel_world_app/features/characters/application/character_dependencies.dart';
import 'package:parallel_world_app/features/characters/domain/character_models.dart';
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

ProviderContainer _container(FakeCharacterRepository repository) =>
    ProviderContainer(
      overrides: [
        sessionControllerProvider.overrideWith(
          _AuthenticatedSessionController.new,
        ),
        characterRepositoryProvider.overrideWithValue(repository),
      ],
    );

void main() {
  test('cached catalogue stays visible when refresh goes offline', () async {
    final repository = FakeCharacterRepository()
      ..catalogueCache = CachedCharacterCatalogue([testCharacter], testNow)
      ..catalogueError = const NetworkFailure();
    final container = _container(repository);
    addTearDown(container.dispose);

    await container.read(characterCatalogueProvider.notifier).load();

    final state = container.read(characterCatalogueProvider);
    expect(state.items.single.id, testCharacter.id);
    expect(state.isOffline, isTrue);
    expect(state.message, contains('Offline'));
  });

  test('first online load maps empty and populated states', () async {
    final repository = FakeCharacterRepository()
      ..page = const CharacterPage(items: [], nextCursor: null, hasMore: false);
    final container = _container(repository);
    addTearDown(container.dispose);

    await container.read(characterCatalogueProvider.notifier).load();
    expect(container.read(characterCatalogueProvider).items, isEmpty);
    expect(container.read(characterCatalogueProvider).message, isNull);

    repository.page = const CharacterPage(
      items: [testCharacter],
      nextCursor: null,
      hasMore: false,
    );
    await container
        .read(characterCatalogueProvider.notifier)
        .load(refresh: true);
    expect(container.read(characterCatalogueProvider).items, [testCharacter]);
  });

  test('next page deduplicates by server character id', () async {
    final repository = FakeCharacterRepository()
      ..page = const CharacterPage(
        items: [testCharacter],
        nextCursor: 'next',
        hasMore: true,
      );
    final container = _container(repository);
    addTearDown(container.dispose);
    await container.read(characterCatalogueProvider.notifier).load();

    await container.read(characterCatalogueProvider.notifier).loadNextPage();

    expect(container.read(characterCatalogueProvider).items, [testCharacter]);
  });
}
