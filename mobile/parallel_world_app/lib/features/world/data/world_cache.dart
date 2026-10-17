import 'package:parallel_world_app/features/world/application/world_contracts.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';
import 'package:parallel_world_app/features/world/domain/world_summary.dart';

class DriftWorldCache implements WorldCache {
  DriftWorldCache(this._database, {DateTime Function()? utcNow})
    : _utcNow = utcNow ?? _defaultUtcNow;

  final AppDatabase _database;
  final DateTime Function() _utcNow;

  @override
  Future<void> initialize() => _database.initialize();

  @override
  Future<WorldSummary?> read(String userId) => _database.readWorld(userId);

  @override
  Future<void> write(String userId, WorldSummary world) => _database
      .replaceWorld(userId: userId, world: world, cachedAtUtc: _utcNow());

  @override
  Future<void> clear() => _database.clearPrivateData();

  static DateTime _defaultUtcNow() => DateTime.now().toUtc();
}
