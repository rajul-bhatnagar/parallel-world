import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/feed/application/feed_dependencies.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';
import 'package:parallel_world_app/features/feed/presentation/post_thread_screen.dart';
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
  overrides: [
    sessionControllerProvider.overrideWith(_AuthenticatedSessionController.new),
    feedRepositoryProvider.overrideWithValue(repository),
  ],
  child: MaterialApp(home: PostThreadScreen(postId: testFeedPost.id)),
);

void main() {
  testWidgets('thread renders root and direct replies and composes a reply', (
    tester,
  ) async {
    final reply = FeedPost(
      id: 'reply-1',
      worldId: testWorld.id,
      author: testFeedAuthor,
      content: 'Direct reply',
      createdAtUtc: testNow.add(const Duration(minutes: 1)),
      parentPostId: testFeedPost.id,
      counts: const FeedCounts(likes: 0, replies: 0),
      visibility: 'world',
    );
    final repository = FakeFeedRepository()
      ..replyPage = FeedPage(items: [reply], nextCursor: null, hasMore: false);
    await tester.pumpWidget(_app(repository));
    await tester.pumpAndSettle();

    expect(find.text(testFeedPost.content), findsOneWidget);
    expect(find.text('Direct reply'), findsOneWidget);
    expect(find.text('0 Replies'), findsOneWidget);
    await tester.tap(find.text('Reply'));
    await tester.pumpAndSettle();
    await tester.enterText(find.byType(TextField), 'New direct reply');
    await tester.pump();
    await tester.tap(find.widgetWithText(FilledButton, 'Reply'));
    await tester.pumpAndSettle();
    expect(find.text('New direct reply'), findsOneWidget);
  });
}
