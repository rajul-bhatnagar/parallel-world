import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/app/router.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

import '../support/fakes.dart';

void main() {
  test('routes each session boundary to an allowed M04 destination', () {
    expect(routeForSession(const SessionState.initial(), '/home'), '/splash');
    expect(
      routeForSession(
        const SessionState(phase: SessionPhase.missingWorld, userId: 'user'),
        '/home',
      ),
      '/world/create',
    );
    expect(
      routeForSession(
        SessionState(
          phase: SessionPhase.authenticated,
          userId: 'user',
          world: testWorld,
        ),
        '/retry',
      ),
      '/home',
    );
    expect(
      routeForSession(
        const SessionState(phase: SessionPhase.sessionExpired),
        '/home',
      ),
      '/retry',
    );
  });

  test('does not redirect when already at the authorized destination', () {
    expect(
      routeForSession(
        SessionState(
          phase: SessionPhase.authenticated,
          userId: 'user',
          world: testWorld,
        ),
        '/characters/character-id',
      ),
      isNull,
    );
    expect(
      routeForSession(
        SessionState(
          phase: SessionPhase.offlineAuthenticated,
          world: testWorld,
        ),
        '/home',
      ),
      isNull,
    );
    expect(
      routeForSession(
        const SessionState(phase: SessionPhase.bootstrapRecoveryExhausted),
        '/retry',
      ),
      isNull,
    );
  });

  test('character deep links remain behind session and world guards', () {
    expect(
      routeForSession(const SessionState.initial(), '/characters/character-id'),
      '/splash',
    );
    expect(
      routeForSession(
        const SessionState(phase: SessionPhase.authenticated, userId: 'user'),
        '/characters/character-id',
      ),
      '/world/create',
    );
  });

  test('feed remains behind session and world guards', () {
    expect(routeForSession(const SessionState.initial(), '/feed'), '/splash');
    expect(
      routeForSession(
        const SessionState(phase: SessionPhase.authenticated, userId: 'user'),
        '/feed',
      ),
      '/world/create',
    );
    expect(
      routeForSession(
        SessionState(
          phase: SessionPhase.authenticated,
          userId: 'user',
          world: testWorld,
        ),
        '/feed',
      ),
      isNull,
    );
  });
}
