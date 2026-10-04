import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/messaging/application/messaging_contracts.dart';
import 'package:parallel_world_app/features/messaging/application/messaging_dependencies.dart';
import 'package:parallel_world_app/features/messaging/domain/messaging_models.dart';
import 'package:parallel_world_app/features/messaging/presentation/chat_screen.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

import '../../support/fakes.dart';

class _AuthenticatedSessionController extends SessionController {
  @override
  SessionState build() => SessionState(
    phase: SessionPhase.authenticated,
    userId: 'user-a',
    world: testWorld,
  );
}

class _FakeMessagingRepository implements MessagingRepository {
  Completer<SendMessageResponse>? pendingSend;
  Object? sendError;

  @override
  Future<CachedMessageHistory?> cachedMessages(
    String u,
    String w,
    String c,
  ) async => null;
  @override
  Future<MessagePage> fetchMessages(
    String u,
    String w,
    String c, {
    String? cursor,
    bool Function()? isCurrent,
  }) async => const MessagePage([], null, false);
  @override
  Future<SendMessageResponse> send(
    String u,
    String w,
    String c,
    ConversationMessage pending, {
    bool Function()? isCurrent,
  }) async {
    if (sendError case final error?) throw error;
    return pendingSend?.future ?? SendMessageResponse(pending, 'noresponse');
  }

  @override
  Future<void> markRead(String w, String c, String m) async {}
  @override
  Future<CachedConversationList?> cachedConversations(
    String u,
    String w,
  ) async => null;
  @override
  Future<ConversationPage> fetchConversations(
    String u,
    String w, {
    bool Function()? isCurrent,
  }) async => const ConversationPage([], null, false);
  @override
  Future<ConversationSummary> openDirect(String w, String c, String k) =>
      throw UnimplementedError();
}

Widget _app(_FakeMessagingRepository repository) => ProviderScope(
  overrides: [
    sessionControllerProvider.overrideWith(_AuthenticatedSessionController.new),
    messagingRepositoryProvider.overrideWithValue(repository),
  ],
  child: const MaterialApp(home: ChatScreen(conversationId: 'conversation-a')),
);

void main() {
  testWidgets(
    'chat exposes empty, optimistic pending, and failed retry states',
    (tester) async {
      final send = Completer<SendMessageResponse>();
      final repository = _FakeMessagingRepository()..pendingSend = send;
      await tester.pumpWidget(_app(repository));
      await tester.pumpAndSettle();
      expect(find.text('Send the first message.'), findsOneWidget);

      await tester.enterText(find.byType(TextField), 'Private hello');
      await tester.tap(find.byTooltip('Send message'));
      await tester.pump();
      expect(find.text('Private hello'), findsOneWidget);
      expect(find.text('Sending…'), findsOneWidget);

      send.completeError(const NetworkFailure());
      await tester.pumpAndSettle();
      expect(find.text('Retry'), findsOneWidget);
      expect(find.textContaining('could not be reached'), findsOneWidget);
    },
  );
}
