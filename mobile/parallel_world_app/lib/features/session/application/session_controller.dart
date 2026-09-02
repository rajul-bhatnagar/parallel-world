import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/auth/session_credentials.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/session/application/session_dependencies.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

final sessionControllerProvider =
    NotifierProvider<SessionController, SessionState>(SessionController.new);

class SessionController extends Notifier<SessionState> {
  Future<void>? _initialization;
  String? _worldCreationKey;

  @override
  SessionState build() => const SessionState.initial();

  Future<void> initialize() {
    final active = _initialization;
    if (active != null) {
      return active;
    }

    final initialization = _initialize();
    _initialization = initialization;
    return initialization.whenComplete(() {
      if (identical(_initialization, initialization)) {
        _initialization = null;
      }
    });
  }

  Future<void> retry() => initialize();

  Future<void> startNewGuestSession() async {
    state = const SessionState(phase: SessionPhase.initializing);
    try {
      await ref.read(sessionLifecycleProvider).clearLocalSession();
      await ref.read(worldCacheProvider).clear();
      await _bootstrapGuest();
    } on AppFailure catch (failure) {
      _setFailure(failure, firstLaunch: true);
    }
  }

  Future<void> createWorld(String name) async {
    final userId = state.userId;
    if (userId == null || state.isBusy) {
      return;
    }
    final normalizedName = name.trim();
    if (normalizedName.isEmpty || normalizedName.length > 80) {
      state = state.copyWith(
        message: 'World name must be between 1 and 80 characters.',
      );
      return;
    }

    state = state.copyWith(isBusy: true, clearMessage: true);
    _worldCreationKey ??= ref.read(secretGeneratorProvider).newIdempotencyKey();
    try {
      final world = await ref
          .read(worldGatewayProvider)
          .create(name: normalizedName, idempotencyKey: _worldCreationKey!);
      await ref.read(worldCacheProvider).write(userId, world);
      _worldCreationKey = null;
      state = SessionState(
        phase: SessionPhase.authenticated,
        userId: userId,
        world: world,
      );
    } on AuthenticationFailure catch (failure) {
      _setFailure(failure);
    } on AppFailure catch (failure) {
      state = state.copyWith(isBusy: false, message: failure.message);
    } on FormatException {
      state = state.copyWith(
        isBusy: false,
        message: const UnknownFailure().message,
      );
    }
  }

  Future<void> logout() async {
    if (state.isBusy) {
      return;
    }
    state = state.copyWith(isBusy: true, clearMessage: true);
    try {
      await ref.read(sessionLifecycleProvider).logout();
      await ref.read(worldCacheProvider).clear();
      state = const SessionState(
        phase: SessionPhase.sessionExpired,
        message: 'You are signed out. Start a new guest session to continue.',
      );
    } on SessionRecoveryFailure catch (failure) {
      state = SessionState(
        phase: SessionPhase.sessionExpired,
        userId: state.userId,
        world: state.world,
        message: failure.message,
      );
    } on AppFailure catch (failure) {
      state = state.copyWith(isBusy: false, message: failure.message);
    }
  }

  Future<void> _initialize() async {
    state = const SessionState(phase: SessionPhase.initializing);
    final cache = ref.read(worldCacheProvider);
    final manager = ref.read(sessionLifecycleProvider);
    try {
      await cache.initialize();
      await manager.ensureInstallationId();
      final credentials = await manager.restore();
      if (credentials == null) {
        await _bootstrapGuest();
        return;
      }
      await _restoreExistingSession(credentials);
    } on AppFailure catch (failure) {
      _setFailure(failure);
    } on FormatException {
      _setFailure(const UnknownFailure());
    } catch (_) {
      _setFailure(const UnknownFailure());
    }
  }

  Future<void> _bootstrapGuest() async {
    try {
      final response = await ref
          .read(sessionLifecycleProvider)
          .bootstrapGuest();
      await ref.read(worldCacheProvider).write(response.userId, response.world);
      state = SessionState(
        phase: SessionPhase.authenticated,
        userId: response.userId,
        world: response.world,
      );
    } on NetworkFailure catch (failure) {
      state = SessionState(
        phase: SessionPhase.recoverableError,
        message: failure.message,
        isFirstLaunchOffline: true,
      );
    } on AuthenticationFailure {
      throw const SessionRecoveryFailure(
        'Guest session recovery is exhausted. Start a new guest session to continue.',
      );
    }
  }

  Future<void> _restoreExistingSession(SessionCredentials credentials) async {
    final now = ref.read(utcNowProvider)();
    if (!credentials.hasUsableAccessToken(now)) {
      if (!credentials.hasUsableRefreshToken(now)) {
        await ref.read(sessionLifecycleProvider).clearLocalSession();
        await _useCachedWorldOrExpire(credentials.userId);
        return;
      }
      try {
        await ref.read(sessionLifecycleProvider).refreshSingleFlight();
      } on NetworkFailure {
        final cached = await ref
            .read(worldCacheProvider)
            .read(credentials.userId);
        if (cached != null) {
          state = SessionState(
            phase: SessionPhase.offlineAuthenticated,
            userId: credentials.userId,
            world: cached,
            message: 'Offline — showing the last synchronized world.',
          );
          return;
        }
        rethrow;
      } on AuthenticationFailure {
        await ref.read(sessionLifecycleProvider).clearLocalSession();
        await _useCachedWorldOrExpire(credentials.userId);
        return;
      }
    }

    await _resolveCurrentWorld(credentials.userId);
  }

  Future<void> _resolveCurrentWorld(String userId) async {
    try {
      final world = await ref.read(worldGatewayProvider).getCurrent();
      await ref.read(worldCacheProvider).write(userId, world);
      state = SessionState(
        phase: SessionPhase.authenticated,
        userId: userId,
        world: world,
      );
    } on NotFoundFailure {
      state = SessionState(phase: SessionPhase.missingWorld, userId: userId);
    } on NetworkFailure {
      final cached = await ref.read(worldCacheProvider).read(userId);
      if (cached == null) {
        rethrow;
      }
      state = SessionState(
        phase: SessionPhase.offlineAuthenticated,
        userId: userId,
        world: cached,
        message: 'Offline — showing the last synchronized world.',
      );
    }
  }

  Future<void> _useCachedWorldOrExpire(String userId) async {
    final cached = await ref.read(worldCacheProvider).read(userId);
    state = SessionState(
      phase: SessionPhase.sessionExpired,
      userId: userId,
      world: cached,
      message: cached == null
          ? 'Your session expired. Start a new guest session to continue.'
          : 'Your session needs online recovery. Cached data is not authorization.',
    );
  }

  void _setFailure(AppFailure failure, {bool firstLaunch = false}) {
    final phase = switch (failure) {
      SessionRecoveryFailure() => SessionPhase.bootstrapRecoveryExhausted,
      AuthenticationFailure() => SessionPhase.sessionExpired,
      _ => SessionPhase.recoverableError,
    };
    state = SessionState(
      phase: phase,
      message: failure.message,
      isFirstLaunchOffline: firstLaunch && failure is NetworkFailure,
    );
  }
}
