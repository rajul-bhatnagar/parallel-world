import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/notifications/application/notification_contracts.dart';
import 'package:parallel_world_app/features/notifications/application/notification_dependencies.dart';
import 'package:parallel_world_app/features/notifications/domain/notification_models.dart';
import 'package:parallel_world_app/features/notifications/presentation/notification_screen.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

import '../../support/fakes.dart';

class _SessionController extends SessionController {
  @override
  SessionState build() => SessionState(
    phase: SessionPhase.authenticated,
    userId: 'user',
    world: testWorld,
  );
}

class _Repository implements NotificationRepository {
  _Repository(this.page);
  final NotificationPage page;
  var markAllCalls = 0;

  @override
  Future<NotificationPage> load(
    NotificationScope scope, {
    String? cursor,
  }) async => page;

  @override
  Future<void> markAllRead(NotificationScope scope) async {
    markAllCalls++;
  }

  @override
  Future<void> markRead(NotificationScope scope, String notificationId) async {}

  @override
  Future<int> unreadCount(NotificationScope scope) async =>
      page.items.where((item) => !item.isRead).length;
}

void main() {
  testWidgets('renders the empty state', (tester) async {
    await _pump(
      tester,
      _Repository(
        const NotificationPage(items: [], nextCursor: null, hasMore: false),
      ),
    );

    expect(find.text('No notifications yet'), findsOneWidget);
    expect(
      tester
          .widget<TextButton>(find.widgetWithText(TextButton, 'Read all'))
          .onPressed,
      isNull,
    );
  });

  testWidgets('renders cached unread state and marks the list read', (
    tester,
  ) async {
    final repository = _Repository(
      NotificationPage(
        items: [
          AppNotification(
            id: 'notification',
            category: 'catchUpSummary',
            title: 'While you were away',
            resourceId: 'summary',
            isRead: false,
            createdAtUtc: DateTime.utc(2026, 10, 11, 4),
          ),
        ],
        nextCursor: null,
        hasMore: false,
        isOffline: true,
      ),
    );
    await _pump(tester, repository);

    expect(find.text('Offline cached notifications'), findsOneWidget);
    expect(find.text('While you were away'), findsOneWidget);
    expect(find.byIcon(Icons.circle), findsOneWidget);

    await tester.tap(find.widgetWithText(TextButton, 'Read all'));
    await tester.pumpAndSettle();

    expect(repository.markAllCalls, 1);
    expect(find.byIcon(Icons.circle), findsNothing);
  });
}

Future<void> _pump(WidgetTester tester, _Repository repository) async {
  await tester.pumpWidget(
    ProviderScope(
      overrides: [
        sessionControllerProvider.overrideWith(_SessionController.new),
        notificationRepositoryProvider.overrideWithValue(repository),
      ],
      child: const MaterialApp(home: NotificationScreen()),
    ),
  );
  await tester.pumpAndSettle();
}
