import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/auth/secret_generator.dart';
import 'package:parallel_world_app/features/session/application/session_contracts.dart';
import 'package:parallel_world_app/features/world/application/world_contracts.dart';

final sessionLifecycleProvider = Provider<SessionLifecycle>(
  (ref) => throw StateError('SessionLifecycle was not wired by the app.'),
);

final worldGatewayProvider = Provider<WorldGateway>(
  (ref) => throw StateError('WorldGateway was not wired by the app.'),
);

final worldCacheProvider = Provider<WorldCache>(
  (ref) => throw StateError('WorldCache was not wired by the app.'),
);

final secretGeneratorProvider = Provider<SecretGenerator>(
  (ref) => SecretGenerator(),
);

final utcNowProvider = Provider<DateTime Function()>(
  (ref) =>
      () => DateTime.now().toUtc(),
);
