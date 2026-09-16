import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/feed/application/feed_contracts.dart';
import 'package:parallel_world_app/features/feed/application/feed_dependencies.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';
import 'package:parallel_world_app/features/feed/presentation/feed_screen.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

import '../../support/fakes.dart';
import '../../support/feed_fakes.dart';

class _AuthenticatedSessionController extends SessionController {
  @override
  SessionState build() => SessionState(
    phase: SessionPhase.authenticated,
    userId: 'user-a',
    world: testWorld,
  );
}

Widget _app(FakeFeedRepository repository) => ProviderScope(
  key: UniqueKey(),
  overrides: [
    sessionControllerProvider.overrideWith(_AuthenticatedSessionController.new),
    feedRepositoryProvider.overrideWithValue(repository),
  ],
  child: const MaterialApp(home: FeedScreen()),
);

void main() {
  testWidgets('feed exposes loading, populated, and empty states', (
    tester,
  ) async {
    final completer = Completer<FeedPage>();
    final repository = FakeFeedRepository()..pendingPage = completer.future;
    await tester.pumpWidget(_app(repository));
    await tester.pump();
    expect(find.bySemanticsLabel('Loading feed'), findsOneWidget);

    completer.complete(repository.page);
    await tester.pumpAndSettle();
    expect(find.text(testFeedPost.author.displayName), findsOneWidget);
    expect(find.text(testFeedPost.content), findsOneWidget);

    repository
      ..pendingPage = null
      ..page = const FeedPage(items: [], nextCursor: null, hasMore: false);
    await tester.pumpWidget(_app(repository));
    await tester.pumpAndSettle();
    expect(find.textContaining('No posts'), findsOneWidget);
  });

  testWidgets('feed exposes error retry and offline stale cache', (
    tester,
  ) async {
    final repository = FakeFeedRepository()..fetchError = const ServerFailure();
    await tester.pumpWidget(_app(repository));
    await tester.pumpAndSettle();
    expect(find.text('Retry'), findsOneWidget);

    repository
      ..cached = CachedFeed(
        items: [testFeedPost],
        cachedAtUtc: testNow,
        nextCursor: null,
        hasMore: false,
      )
      ..fetchError = const NetworkFailure();
    await tester.pumpWidget(_app(repository));
    await tester.pumpAndSettle();
    expect(find.text('Offline saved feed'), findsOneWidget);
    expect(find.text(testFeedPost.content), findsOneWidget);
  });

  testWidgets('composer displays optimistic pending and failed retry state', (
    tester,
  ) async {
    final createCompleter = Completer<FeedPost>();
    final repository = FakeFeedRepository()
      ..pendingCreate = createCompleter.future;
    await tester.pumpWidget(_app(repository));
    await tester.pumpAndSettle();

    await tester.tap(find.text('Post'));
    await tester.pumpAndSettle();
    await tester.enterText(find.byType(TextField), 'A player post');
    await tester.pump();
    await tester.tap(find.text('Publish'));
    await tester.pump(const Duration(milliseconds: 500));

    expect(find.text('A player post'), findsWidgets);
    expect(find.text('Sending…'), findsOneWidget);

    createCompleter.completeError(const NetworkFailure());
    await tester.pumpAndSettle();
    expect(find.text('Not sent'), findsOneWidget);
    expect(find.text('Retry post'), findsOneWidget);
  });

  testWidgets(
    'feed shows explicit load-more progress and preserves pagination',
    (tester) async {
      final repository = FakeFeedRepository()
        ..page = FeedPage(
          items: [testFeedPost],
          nextCursor: 'opaque-next',
          hasMore: true,
        );
      await tester.pumpWidget(_app(repository));
      await tester.pumpAndSettle();
      expect(find.text('Load more'), findsOneWidget);

      final nextCompleter = Completer<FeedPage>();
      repository.pendingPage = nextCompleter.future;
      await tester.tap(find.text('Load more'));
      await tester.pump();
      expect(find.byType(CircularProgressIndicator), findsOneWidget);

      nextCompleter.complete(
        const FeedPage(items: [], nextCursor: null, hasMore: false),
      );
      await tester.pumpAndSettle();
      expect(repository.lastCursor, 'opaque-next');
    },
  );
}
