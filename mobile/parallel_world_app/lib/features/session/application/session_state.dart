import 'package:parallel_world_app/features/world/domain/world_summary.dart';

enum SessionPhase {
  initial,
  initializing,
  authenticated,
  offlineAuthenticated,
  missingWorld,
  recoverableError,
  bootstrapRecoveryExhausted,
  sessionExpired,
}

class SessionState {
  const SessionState({
    required this.phase,
    this.userId,
    this.world,
    this.message,
    this.isBusy = false,
    this.isFirstLaunchOffline = false,
  });

  const SessionState.initial() : this(phase: SessionPhase.initial);

  final SessionPhase phase;
  final String? userId;
  final WorldSummary? world;
  final String? message;
  final bool isBusy;
  final bool isFirstLaunchOffline;

  SessionState copyWith({
    SessionPhase? phase,
    String? userId,
    WorldSummary? world,
    String? message,
    bool? isBusy,
    bool? isFirstLaunchOffline,
    bool clearMessage = false,
  }) => SessionState(
    phase: phase ?? this.phase,
    userId: userId ?? this.userId,
    world: world ?? this.world,
    message: clearMessage ? null : message ?? this.message,
    isBusy: isBusy ?? this.isBusy,
    isFirstLaunchOffline: isFirstLaunchOffline ?? this.isFirstLaunchOffline,
  );
}
