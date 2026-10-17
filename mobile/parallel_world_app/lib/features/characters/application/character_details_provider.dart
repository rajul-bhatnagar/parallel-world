import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/characters/application/character_contracts.dart';
import 'package:parallel_world_app/features/characters/application/character_dependencies.dart';
import 'package:parallel_world_app/features/characters/domain/character_models.dart';

class CharacterDetailsRequest {
  const CharacterDetailsRequest({
    required this.userId,
    required this.worldId,
    required this.characterId,
  });

  final String userId;
  final String worldId;
  final String characterId;

  @override
  bool operator ==(Object other) =>
      other is CharacterDetailsRequest &&
      other.userId == userId &&
      other.worldId == worldId &&
      other.characterId == characterId;

  @override
  int get hashCode => Object.hash(userId, worldId, characterId);
}

class CharacterDetailsView {
  const CharacterDetailsView(
    this.details, {
    this.isOffline = false,
    this.isStale = false,
  });

  final CharacterDetails details;
  final bool isOffline;
  final bool isStale;
}

final characterDetailsProvider = StreamProvider.autoDispose
    .family<CharacterDetailsView, CharacterDetailsRequest>((
      ref,
      request,
    ) async* {
      final repository = ref.read(characterRepositoryProvider);
      CachedCharacterDetails? cached;
      try {
        cached = await repository.readCachedDetails(
          request.userId,
          request.worldId,
          request.characterId,
        );
      } on FormatException {
        cached = null;
      } catch (_) {
        cached = null;
      }
      if (cached != null) {
        yield CharacterDetailsView(cached.value, isStale: true);
      }
      try {
        final details = await repository.fetchDetails(
          userId: request.userId,
          worldId: request.worldId,
          characterId: request.characterId,
        );
        yield CharacterDetailsView(details);
      } on NetworkFailure {
        if (cached != null) {
          yield CharacterDetailsView(cached.value, isOffline: true);
          return;
        }
        rethrow;
      }
    });
