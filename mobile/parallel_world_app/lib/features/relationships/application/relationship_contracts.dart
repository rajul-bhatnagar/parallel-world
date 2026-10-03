import 'package:parallel_world_app/features/relationships/domain/relationship_models.dart';

abstract interface class RelationshipGateway {
  Future<RelationshipSummary?> get({
    required String worldId,
    required String actorId,
  });

  Future<List<RelationshipHistoryItem>> history({
    required String worldId,
    required String actorId,
  });
}
