import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';
import 'package:parallel_world_app/features/session/presentation/retry_screen.dart';
import 'package:parallel_world_app/features/session/presentation/splash_screen.dart';
import 'package:parallel_world_app/features/world/presentation/home_screen.dart';
import 'package:parallel_world_app/features/world/presentation/world_create_screen.dart';
import 'package:parallel_world_app/features/characters/presentation/character_catalogue_screen.dart';
import 'package:parallel_world_app/features/characters/presentation/character_profile_screen.dart';
import 'package:parallel_world_app/features/feed/presentation/feed_screen.dart';

final appRouterProvider = Provider<GoRouter>((ref) {
  final refresh = _RouterRefreshNotifier();
  ref.listen<SessionState>(sessionControllerProvider, (_, _) => refresh.ping());
  ref.onDispose(refresh.dispose);

  return GoRouter(
    initialLocation: '/splash',
    refreshListenable: refresh,
    redirect: (context, state) => routeForSession(
      ref.read(sessionControllerProvider),
      state.matchedLocation,
    ),
    routes: [
      GoRoute(path: '/', redirect: (_, _) => '/splash'),
      GoRoute(path: '/splash', builder: (_, _) => const SplashScreen()),
      GoRoute(path: '/retry', builder: (_, _) => const RetryScreen()),
      GoRoute(
        path: '/world/create',
        builder: (_, _) => const WorldCreateScreen(),
      ),
      GoRoute(path: '/home', builder: (_, _) => const HomeScreen()),
      GoRoute(path: '/feed', builder: (_, _) => const FeedScreen()),
      GoRoute(
        path: '/characters',
        builder: (_, _) => const CharacterCatalogueScreen(),
      ),
      GoRoute(
        path: '/characters/:characterId',
        builder: (_, state) => CharacterProfileScreen(
          characterId: state.pathParameters['characterId']!,
        ),
      ),
    ],
  );
});

String? routeForSession(SessionState session, String location) {
  final authenticatedLocation =
      location == '/home' ||
      location == '/feed' ||
      location == '/characters' ||
      location.startsWith('/characters/');
  final target = switch (session.phase) {
    SessionPhase.initial || SessionPhase.initializing => '/splash',
    SessionPhase.authenticated || SessionPhase.offlineAuthenticated =>
      session.world == null
          ? '/world/create'
          : authenticatedLocation
          ? location
          : '/home',
    SessionPhase.missingWorld => '/world/create',
    SessionPhase.recoverableError ||
    SessionPhase.bootstrapRecoveryExhausted ||
    SessionPhase.sessionExpired => '/retry',
  };
  return location == target ? null : target;
}

class _RouterRefreshNotifier extends ChangeNotifier {
  void ping() => notifyListeners();
}
