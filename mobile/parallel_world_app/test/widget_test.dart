import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';
import 'package:parallel_world_app/features/session/presentation/retry_screen.dart';
import 'package:parallel_world_app/features/session/presentation/splash_screen.dart';
import 'package:parallel_world_app/features/world/presentation/home_screen.dart';
import 'package:parallel_world_app/features/world/presentation/world_create_screen.dart';

import 'support/fakes.dart';

class TestSessionController extends SessionController {
  TestSessionController(this.initialState);

  final SessionState initialState;

  @override
  SessionState build() => initialState;
}

Widget testApp(Widget child, SessionState state) => ProviderScope(
  overrides: [
    sessionControllerProvider.overrideWith(() => TestSessionController(state)),
  ],
  child: MaterialApp(home: child),
);

void main() {
  testWidgets('splash screen presents deterministic startup progress', (
    tester,
  ) async {
    await tester.pumpWidget(
      testApp(const SplashScreen(), const SessionState.initial()),
    );

    expect(find.text('Preparing your private world…'), findsOneWidget);
    expect(find.byType(CircularProgressIndicator), findsOneWidget);
  });

  testWidgets(
    'first-launch offline state explains that a connection is needed',
    (tester) async {
      await tester.pumpWidget(
        testApp(
          const RetryScreen(),
          const SessionState(
            phase: SessionPhase.recoverableError,
            isFirstLaunchOffline: true,
            message: 'Check your connection and retry.',
          ),
        ),
      );

      expect(find.text('Connect to create your private world'), findsOneWidget);
      expect(find.text('Retry'), findsOneWidget);
    },
  );

  testWidgets('home exposes an honest offline cached state', (tester) async {
    await tester.pumpWidget(
      testApp(
        const HomeScreen(),
        SessionState(
          phase: SessionPhase.offlineAuthenticated,
          userId: 'user',
          world: testWorld,
          message: 'Offline — showing the last synchronized world.',
        ),
      ),
    );

    expect(find.text('Offline cached view'), findsOneWidget);
    expect(find.text('My Parallel World'), findsOneWidget);
    expect(find.textContaining('later milestones'), findsOneWidget);
  });

  testWidgets('bootstrap exhaustion offers an explicit new-session action', (
    tester,
  ) async {
    await tester.pumpWidget(
      testApp(
        const RetryScreen(),
        const SessionState(
          phase: SessionPhase.bootstrapRecoveryExhausted,
          message: 'Guest session recovery is exhausted.',
        ),
      ),
    );

    expect(find.text('Session recovery needed'), findsOneWidget);
    expect(find.text('Start new session'), findsOneWidget);
    expect(find.text('Retry'), findsNothing);
  });

  testWidgets('world creation screen validates an empty name', (tester) async {
    await tester.pumpWidget(
      testApp(
        const WorldCreateScreen(),
        const SessionState(phase: SessionPhase.missingWorld, userId: 'user'),
      ),
    );

    await tester.enterText(find.byType(TextFormField), '   ');
    await tester.tap(find.widgetWithText(FilledButton, 'Create world'));
    await tester.pump();

    expect(find.text('Enter a name up to 80 characters.'), findsOneWidget);
  });
}
