import 'package:parallel_world_app/features/characters/domain/character_models.dart';

abstract interface class CharacterGateway {
  Future<CharacterPage> list({
    required String worldId,
    int limit = 20,
    String? cursor,
  });

  Future<CharacterDetails> get({
    required String worldId,
    required String characterId,
  });
}

class CachedCharacterCatalogue {
  const CachedCharacterCatalogue(this.items, this.cachedAtUtc);

  final List<CharacterSummary> items;
  final DateTime cachedAtUtc;
}

class CachedCharacterDetails {
  const CachedCharacterDetails(this.value, this.cachedAtUtc);

  final CharacterDetails value;
  final DateTime cachedAtUtc;
}

abstract interface class CharacterCache {
  Future<CachedCharacterCatalogue?> readCatalogue(
    String userId,
    String worldId,
  );

  Future<void> replaceCatalogue(
    String userId,
    String worldId,
    List<CharacterSummary> items,
  );

  Future<CachedCharacterDetails?> readDetails(
    String userId,
    String worldId,
    String characterId,
  );

  Future<void> writeDetails(
    String userId,
    String worldId,
    CharacterDetails details,
  );
}

abstract interface class CharacterRepository {
  Future<CachedCharacterCatalogue?> readCachedCatalogue(
    String userId,
    String worldId,
  );

  Future<CharacterPage> fetchCatalogue({
    required String userId,
    required String worldId,
    int limit = 20,
    String? cursor,
  });

  Future<CachedCharacterDetails?> readCachedDetails(
    String userId,
    String worldId,
    String characterId,
  );

  Future<CharacterDetails> fetchDetails({
    required String userId,
    required String worldId,
    required String characterId,
  });
}
