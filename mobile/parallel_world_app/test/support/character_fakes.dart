import 'package:parallel_world_app/features/characters/application/character_contracts.dart';
import 'package:parallel_world_app/features/characters/domain/character_models.dart';

const testCharacter = CharacterSummary(
  id: '00000000-0000-0000-0000-000000000201',
  displayName: 'Maya Chen',
  handle: 'maya',
  profession: 'Designer',
  visibleMood: 'inspired',
  isFollowed: false,
);

const testCharacterDetails = CharacterDetails(
  id: '00000000-0000-0000-0000-000000000201',
  displayName: 'Maya Chen',
  handle: 'maya',
  bio: 'A thoughtful designer who notices small details.',
  age: 29,
  profession: 'Designer',
  archetype: 'Creative observer',
  visibleMood: 'inspired',
  interests: [CharacterInterest('design'), CharacterInterest('technology')],
  schedule: [
    CharacterSchedule(
      dayOfWeek: 1,
      startLocalTime: '09:00',
      endLocalTime: '17:00',
      activity: 'Studio work',
    ),
  ],
);

class FakeCharacterRepository implements CharacterRepository {
  CachedCharacterCatalogue? catalogueCache;
  CachedCharacterDetails? detailsCache;
  CharacterPage page = const CharacterPage(
    items: [testCharacter],
    nextCursor: null,
    hasMore: false,
  );
  CharacterDetails details = testCharacterDetails;
  Object? catalogueError;
  Object? detailsError;
  Future<CharacterPage>? pendingPage;
  int catalogueCalls = 0;
  int detailsCalls = 0;

  @override
  Future<CachedCharacterCatalogue?> readCachedCatalogue(
    String userId,
    String worldId,
  ) async => catalogueCache;

  @override
  Future<CharacterPage> fetchCatalogue({
    required String userId,
    required String worldId,
    int limit = 20,
    String? cursor,
  }) async {
    catalogueCalls++;
    if (catalogueError case final error?) {
      throw error;
    }
    return pendingPage ?? page;
  }

  @override
  Future<CachedCharacterDetails?> readCachedDetails(
    String userId,
    String worldId,
    String characterId,
  ) async => detailsCache;

  @override
  Future<CharacterDetails> fetchDetails({
    required String userId,
    required String worldId,
    required String characterId,
  }) async {
    detailsCalls++;
    if (detailsError case final error?) {
      throw error;
    }
    return details;
  }
}
