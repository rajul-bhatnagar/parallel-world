import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/relationships/application/relationship_contracts.dart';
import 'package:parallel_world_app/features/relationships/domain/relationship_models.dart';

final relationshipGatewayProvider = Provider<RelationshipGateway>(
  (ref) => throw UnimplementedError('RelationshipGateway must be overridden.'),
);

class RelationshipRequest {
  const RelationshipRequest(this.worldId, this.actorId);

  final String worldId;
  final String actorId;

  @override
  bool operator ==(Object other) =>
      other is RelationshipRequest &&
      other.worldId == worldId &&
      other.actorId == actorId;

  @override
  int get hashCode => Object.hash(worldId, actorId);
}

final relationshipProvider = FutureProvider.autoDispose
    .family<RelationshipView, RelationshipRequest>((ref, request) async {
      final gateway = ref.read(relationshipGatewayProvider);
      try {
        final summary = await gateway.get(
          worldId: request.worldId,
          actorId: request.actorId,
        );
        if (summary == null) return const RelationshipView();
        final history = await gateway.history(
          worldId: request.worldId,
          actorId: request.actorId,
        );
        return RelationshipView(summary: summary, history: history);
      } on NetworkFailure {
        return const RelationshipView(isOffline: true);
      }
    });
