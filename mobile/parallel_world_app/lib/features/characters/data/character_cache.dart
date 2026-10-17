import 'dart:convert';

import 'package:drift/drift.dart';
import 'package:parallel_world_app/features/characters/application/character_contracts.dart'
    as contracts;
import 'package:parallel_world_app/features/characters/domain/character_models.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';

class DriftCharacterCache implements contracts.CharacterCache {
  DriftCharacterCache(this._database, {DateTime Function()? utcNow})
    : _utcNow = utcNow ?? _defaultUtcNow;

  final AppDatabase _database;
  final DateTime Function() _utcNow;

  @override
  Future<contracts.CachedCharacterCatalogue?> readCatalogue(
    String userId,
    String worldId,
  ) async {
    final metadata =
        await (_database.select(_database.cachedCharacterCatalogues)..where(
              (table) =>
                  table.userId.equals(userId) & table.worldId.equals(worldId),
            ))
            .getSingleOrNull();
    if (metadata == null) {
      return null;
    }
    final rows =
        await (_database.select(_database.cachedCharacterSummaries)..where(
              (table) =>
                  table.userId.equals(userId) & table.worldId.equals(worldId),
            ))
            .get();
    return contracts.CachedCharacterCatalogue(
      rows
          .map(
            (row) => CharacterSummary(
              id: row.characterId,
              displayName: row.displayName,
              handle: row.handle,
              profession: row.profession,
              visibleMood: row.visibleMood,
              isFollowed: row.isFollowed,
            ),
          )
          .toList(growable: false),
      metadata.cachedAtUtc.toUtc(),
    );
  }

  @override
  Future<void> replaceCatalogue(
    String userId,
    String worldId,
    List<CharacterSummary> items,
  ) => _database.transaction(() async {
    await (_database.delete(_database.cachedCharacterSummaries)..where(
          (table) =>
              table.userId.equals(userId) & table.worldId.equals(worldId),
        ))
        .go();
    for (final item in items) {
      await _database
          .into(_database.cachedCharacterSummaries)
          .insert(
            CachedCharacterSummariesCompanion.insert(
              userId: userId,
              worldId: worldId,
              characterId: item.id,
              displayName: item.displayName,
              handle: item.handle,
              profession: item.profession,
              visibleMood: item.visibleMood,
              isFollowed: item.isFollowed,
            ),
          );
    }
    await _database
        .into(_database.cachedCharacterCatalogues)
        .insertOnConflictUpdate(
          CachedCharacterCataloguesCompanion.insert(
            userId: userId,
            worldId: worldId,
            cachedAtUtc: _utcNow(),
          ),
        );
  });

  @override
  Future<contracts.CachedCharacterDetails?> readDetails(
    String userId,
    String worldId,
    String characterId,
  ) async {
    final row =
        await (_database.select(_database.cachedCharacterDetails)..where(
              (table) =>
                  table.userId.equals(userId) &
                  table.worldId.equals(worldId) &
                  table.characterId.equals(characterId),
            ))
            .getSingleOrNull();
    if (row == null) {
      return null;
    }
    return contracts.CachedCharacterDetails(
      CharacterDetails(
        id: row.characterId,
        displayName: row.displayName,
        handle: row.handle,
        bio: row.bio,
        age: row.age,
        profession: row.profession,
        archetype: row.archetype,
        visibleMood: row.visibleMood,
        interests: (jsonDecode(row.interestsJson) as List)
            .map((value) => CharacterInterest(value as String))
            .toList(growable: false),
        schedule: (jsonDecode(row.scheduleJson) as List)
            .map(
              (value) => CharacterSchedule.fromJson(
                Map<String, Object?>.from(value as Map),
              ),
            )
            .toList(growable: false),
      ),
      row.cachedAtUtc.toUtc(),
    );
  }

  @override
  Future<void> writeDetails(
    String userId,
    String worldId,
    CharacterDetails details,
  ) => _database
      .into(_database.cachedCharacterDetails)
      .insertOnConflictUpdate(
        CachedCharacterDetailsCompanion.insert(
          userId: userId,
          worldId: worldId,
          characterId: details.id,
          displayName: details.displayName,
          handle: details.handle,
          bio: details.bio,
          age: details.age,
          profession: details.profession,
          archetype: details.archetype,
          visibleMood: details.visibleMood,
          interestsJson: jsonEncode(
            details.interests.map((interest) => interest.topicId).toList(),
          ),
          scheduleJson: jsonEncode(
            details.schedule
                .map(
                  (item) => {
                    'dayOfWeek': item.dayOfWeek,
                    'startLocalTime': item.startLocalTime,
                    'endLocalTime': item.endLocalTime,
                    'activity': item.activity,
                  },
                )
                .toList(),
          ),
          cachedAtUtc: _utcNow(),
        ),
      );

  static DateTime _defaultUtcNow() => DateTime.now().toUtc();
}
