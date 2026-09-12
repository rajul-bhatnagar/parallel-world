import 'package:parallel_world_app/features/characters/application/character_contracts.dart';
import 'package:parallel_world_app/features/characters/domain/character_models.dart';

class ApiCachedCharacterRepository implements CharacterRepository {
  ApiCachedCharacterRepository(this._gateway, this._cache);

  final CharacterGateway _gateway;
  final CharacterCache _cache;

  @override
  Future<CachedCharacterCatalogue?> readCachedCatalogue(
    String userId,
    String worldId,
  ) => _cache.readCatalogue(userId, worldId);

  @override
  Future<CharacterPage> fetchCatalogue({
    required String userId,
    required String worldId,
    int limit = 20,
    String? cursor,
  }) async {
    final page = await _gateway.list(
      worldId: worldId,
      limit: limit,
      cursor: cursor,
    );
    if (cursor == null) {
      await _cache.replaceCatalogue(userId, worldId, page.items);
    }
    return page;
  }

  @override
  Future<CachedCharacterDetails?> readCachedDetails(
    String userId,
    String worldId,
    String characterId,
  ) => _cache.readDetails(userId, worldId, characterId);

  @override
  Future<CharacterDetails> fetchDetails({
    required String userId,
    required String worldId,
    required String characterId,
  }) async {
    final details = await _gateway.get(
      worldId: worldId,
      characterId: characterId,
    );
    await _cache.writeDetails(userId, worldId, details);
    return details;
  }
}
