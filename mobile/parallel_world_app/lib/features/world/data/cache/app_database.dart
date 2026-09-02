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

@DriftDatabase(tables: [CachedWorlds])
class AppDatabase extends _$AppDatabase {
  AppDatabase([QueryExecutor? executor])
    : super(executor ?? driftDatabase(name: 'parallel_world_cache'));

  @override
  int get schemaVersion => 1;

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

  Future<void> clearPrivateData() => delete(cachedWorlds).go();
}
