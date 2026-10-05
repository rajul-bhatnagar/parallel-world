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

  Future<DatingInvitation> invite({
    required String worldId,
    required String characterId,
    required String idempotencyKey,
  });

  Future<List<DatingInvitation>> invitations({required String worldId});

  Future<List<RomanticHistoryItem>> romanticHistory({
    required String worldId,
    required String characterId,
  });

  Future<DatingInvitation> resolve({
    required String worldId,
    required String invitationId,
    required String decision,
  });
}
