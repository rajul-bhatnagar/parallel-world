import 'package:drift/drift.dart';
import 'package:drift_flutter/drift_flutter.dart';
import 'package:parallel_world_app/features/world/domain/world_summary.dart';

part 'app_database.g.dart';

class CachedWorlds extends Table {
  TextColumn get userId => text()();

  TextColumn get worldId => text()();

  TextColumn get name => text().withLength(min: 1, max: 80)();

  TextColumn get status => text()();

  DateTimeColumn get currentGameTimeUtc => dateTime()();

  TextColumn get playerActorId => text()();

  TextColumn get playerDisplayName => text()();

  DateTimeColumn get createdAtUtc => dateTime()();

  DateTimeColumn get cachedAtUtc => dateTime()();

  @override
  Set<Column<Object>> get primaryKey => {userId};
}

class CachedCharacterCatalogues extends Table {
  TextColumn get userId => text()();
  TextColumn get worldId => text()();
  DateTimeColumn get cachedAtUtc => dateTime()();

  @override
  Set<Column<Object>> get primaryKey => {userId, worldId};
}

class CachedCharacterSummaries extends Table {
  TextColumn get userId => text()();
  TextColumn get worldId => text()();
  TextColumn get characterId => text()();
  TextColumn get displayName => text().withLength(min: 1, max: 60)();
  TextColumn get handle => text().withLength(min: 1, max: 30)();
  TextColumn get profession => text().withLength(min: 1, max: 80)();
  TextColumn get visibleMood => text()();
  BoolColumn get isFollowed => boolean()();

  @override
  Set<Column<Object>> get primaryKey => {userId, worldId, characterId};
}

class CachedCharacterDetails extends Table {
  TextColumn get userId => text()();
  TextColumn get worldId => text()();
  TextColumn get characterId => text()();
  TextColumn get displayName => text().withLength(min: 1, max: 60)();
  TextColumn get handle => text().withLength(min: 1, max: 30)();
  TextColumn get bio => text().withLength(max: 300)();
  IntColumn get age => integer()();
  TextColumn get profession => text().withLength(min: 1, max: 80)();
  TextColumn get archetype => text().withLength(min: 1, max: 80)();
  TextColumn get visibleMood => text()();
  TextColumn get interestsJson => text()();
  TextColumn get scheduleJson => text()();
  DateTimeColumn get cachedAtUtc => dateTime()();

  @override
  Set<Column<Object>> get primaryKey => {userId, worldId, characterId};
}

class CachedFeedMetadata extends Table {
  TextColumn get userId => text()();
  TextColumn get worldId => text()();
  DateTimeColumn get cachedAtUtc => dateTime()();
  TextColumn get nextCursor => text().nullable()();
  BoolColumn get hasMore => boolean()();

  @override
  Set<Column<Object>> get primaryKey => {userId, worldId};
}

class CachedFeedPosts extends Table {
  TextColumn get userId => text()();
  TextColumn get worldId => text()();
  TextColumn get postId => text()();
  TextColumn get authorActorId => text()();
  TextColumn get authorDisplayName => text()();
  TextColumn get authorHandle => text()();
  TextColumn get authorActorType => text()();
  TextColumn get content => text().withLength(min: 1, max: 500)();
  DateTimeColumn get createdAtUtc => dateTime()();
  TextColumn get parentPostId => text().nullable()();
  IntColumn get likeCount => integer()();
  IntColumn get replyCount => integer()();
  TextColumn get visibility => text()();
  TextColumn get localState => text()();
  TextColumn get clientPostId => text().nullable()();
  TextColumn get idempotencyKey => text().nullable()();
  TextColumn get failureMessage => text().nullable()();

  @override
  Set<Column<Object>> get primaryKey => {userId, worldId, postId};
}

@DriftDatabase(
  tables: [
    CachedWorlds,
    CachedCharacterCatalogues,
    CachedCharacterSummaries,
    CachedCharacterDetails,
    CachedFeedMetadata,
    CachedFeedPosts,
  ],
)
class AppDatabase extends _$AppDatabase {
  AppDatabase([QueryExecutor? executor])
    : super(executor ?? driftDatabase(name: 'parallel_world_cache'));

  @override
  int get schemaVersion => 3;

  @override
  MigrationStrategy get migration => MigrationStrategy(
    onCreate: (migrator) => migrator.createAll(),
    onUpgrade: (migrator, from, to) async {
      if (from < 2) {
        await migrator.createTable(cachedCharacterCatalogues);
        await migrator.createTable(cachedCharacterSummaries);
        await migrator.createTable(cachedCharacterDetails);
      }
      if (from < 3) {
        await migrator.createTable(cachedFeedMetadata);
        await migrator.createTable(cachedFeedPosts);
      }
    },
  );

  Future<void> initialize() async {
    await customSelect('SELECT 1').getSingle();
  }

  Future<void> replaceWorld({
    required String userId,
    required WorldSummary world,
    required DateTime cachedAtUtc,
  }) => into(cachedWorlds).insertOnConflictUpdate(
    CachedWorldsCompanion.insert(
      userId: userId,
      worldId: world.id,
      name: world.name,
      status: world.status,
      currentGameTimeUtc: world.currentGameTimeUtc,
      playerActorId: world.playerActorId,
      playerDisplayName: world.playerDisplayName,
      createdAtUtc: world.createdAtUtc,
      cachedAtUtc: cachedAtUtc,
    ),
  );

  Future<WorldSummary?> readWorld(String ownerUserId) async {
    final row = await (select(
      cachedWorlds,
    )..where((table) => table.userId.equals(ownerUserId))).getSingleOrNull();
    if (row == null) {
      return null;
    }
    return WorldSummary(
      id: row.worldId,
      name: row.name,
      status: row.status,
      currentGameTimeUtc: row.currentGameTimeUtc,
      playerActorId: row.playerActorId,
      playerDisplayName: row.playerDisplayName,
      createdAtUtc: row.createdAtUtc,
    );
  }

  Future<void> clearPrivateData() => transaction(() async {
    await delete(cachedFeedPosts).go();
    await delete(cachedFeedMetadata).go();
    await delete(cachedCharacterDetails).go();
    await delete(cachedCharacterSummaries).go();
    await delete(cachedCharacterCatalogues).go();
    await delete(cachedWorlds).go();
  });
}
