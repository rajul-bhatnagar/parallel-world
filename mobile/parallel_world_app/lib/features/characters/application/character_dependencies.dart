import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/features/characters/application/character_contracts.dart';

final characterRepositoryProvider = Provider<CharacterRepository>(
  (ref) => throw StateError('CharacterRepository was not wired by the app.'),
);
