import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/catch_up/application/catch_up_provider.dart';
import 'package:parallel_world_app/features/catch_up/domain/catch_up_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';
import 'package:parallel_world_app/features/world/presentation/home_screen.dart';

import '../../support/fakes.dart';

class _SessionController extends SessionController {
  @override
  SessionState build() =>
      SessionState(phase: SessionPhase.authenticated, userId: 'user', world: testWorld);
}

void main() {
  testWidgets('home renders partial progress and only server summary facts', (
    tester,
  ) async {
    const summary = CatchUpSummary(
      status: 'Partial',
      text: 'Your world moved forward while you were away.',
      items: [
        CatchUpSummaryItem(
          itemType: 'follow',
          wording: 'Ava followed Rowan.',
        ),
      ],
    );
    const result = CatchUpResult(
      disposition: 'partial',
      processedIntervals: 8,
      remainingIntervals: 4,
      summary: summary,
    );
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          sessionControllerProvider.overrideWith(_SessionController.new),
          catchUpProvider(testWorld.id).overrideWith(
            (ref) async =>
                const CatchUpView(result: result, summary: summary),
          ),
        ],
        child: const MaterialApp(home: HomeScreen()),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('World catch-up in progress'), findsOneWidget);
    expect(find.text('• Ava followed Rowan.'), findsOneWidget);
    expect(find.text('4 intervals remain.'), findsOneWidget);
    expect(find.text('Continue catch-up'), findsOneWidget);
    expect(find.widgetWithText(TextButton, 'View characters'), findsOneWidget);
  });
}
