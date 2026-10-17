import 'package:parallel_world_app/features/world/domain/world_summary.dart';

abstract interface class WorldGateway {
  Future<WorldSummary> getCurrent();

  Future<WorldSummary> create({
    required String name,
    required String idempotencyKey,
  });
}

abstract interface class WorldCache {
  Future<void> initialize();

  Future<WorldSummary?> read(String userId);

  Future<void> write(String userId, WorldSummary world);

  Future<void> clear();
}
