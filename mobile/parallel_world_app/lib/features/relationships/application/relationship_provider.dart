import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/relationships/application/relationship_contracts.dart';
import 'package:parallel_world_app/features/relationships/domain/relationship_models.dart';
import 'package:parallel_world_app/features/session/application/session_dependencies.dart';

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
        final history = summary == null
            ? <RelationshipHistoryItem>[]
            : await gateway.history(
                worldId: request.worldId,
                actorId: request.actorId,
              );
        final invitations = await gateway.invitations(worldId: request.worldId);
        final romanticHistory = await gateway.romanticHistory(
          worldId: request.worldId,
          characterId: request.actorId,
        );
        return RelationshipView(
          summary: summary,
          history: history,
          invitation: invitations
              .where((item) => item.characterId == request.actorId)
              .firstOrNull,
          romanticHistory: romanticHistory,
        );
      } on NetworkFailure {
        return const RelationshipView(isOffline: true);
      }
    });

final datingActionProvider = NotifierProvider.autoDispose
    .family<DatingActionController, DatingActionState, RelationshipRequest>(
      DatingActionController.new,
    );

class DatingActionState {
  const DatingActionState({this.isSubmitting = false, this.message});
  final bool isSubmitting;
  final String? message;
}

class DatingActionController extends Notifier<DatingActionState> {
  DatingActionController(this.request);
  final RelationshipRequest request;

  @override
  DatingActionState build() => const DatingActionState();

  Future<void> invite() async {
    if (state.isSubmitting) return;
    state = const DatingActionState(isSubmitting: true);
    try {
      await ref
          .read(relationshipGatewayProvider)
          .invite(
            worldId: request.worldId,
            characterId: request.actorId,
            idempotencyKey: ref
                .read(secretGeneratorProvider)
                .newIdempotencyKey(),
          );
      state = const DatingActionState(message: 'Invitation resolved.');
      ref.invalidate(relationshipProvider(request));
    } on AppFailure catch (error) {
      state = DatingActionState(message: error.message);
    }
  }

  Future<void> resolve(String invitationId, String decision) async {
    if (state.isSubmitting) return;
    state = const DatingActionState(isSubmitting: true);
    try {
      await ref
          .read(relationshipGatewayProvider)
          .resolve(
            worldId: request.worldId,
            invitationId: invitationId,
            decision: decision,
          );
      state = DatingActionState(message: 'Invitation ${decision}ed.');
      ref.invalidate(relationshipProvider(request));
    } on AppFailure catch (error) {
      state = DatingActionState(message: error.message);
    }
  }
}
