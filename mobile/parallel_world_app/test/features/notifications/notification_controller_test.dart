import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/notifications/application/notification_contracts.dart';
import 'package:parallel_world_app/features/notifications/application/notification_controller.dart';
import 'package:parallel_world_app/features/notifications/application/notification_dependencies.dart';
import 'package:parallel_world_app/features/notifications/domain/notification_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';
import 'package:parallel_world_app/features/world/domain/world_summary.dart';

import '../../support/fakes.dart';

class _MutableSessionController extends SessionController {
  @override
  SessionState build() => SessionState(
    phase: SessionPhase.authenticated,
    userId: 'user-a',
    world: testWorld,
  );

  void authenticate(String userId, WorldSummary world) {
    state = SessionState(
      phase: SessionPhase.authenticated,
      userId: userId,
      world: world,
    );
  }
}

class _DelayedRepository implements NotificationRepository {
  final requests = <Completer<NotificationPage>>[];

  @override
  Future<NotificationPage> load(NotificationScope scope, {String? cursor}) {
    final request = Completer<NotificationPage>();
    requests.add(request);
    return request.future;
  }

  @override
  Future<void> markAllRead(NotificationScope scope) async {}

  @override
  Future<void> markRead(NotificationScope scope, String notificationId) async {}

  @override
  Future<int> unreadCount(NotificationScope scope) async => 0;
}

void main() {
  test('world switch rejects a late notification response', () async {
    final repository = _DelayedRepository();
    final container = ProviderContainer(
      overrides: [
        sessionControllerProvider.overrideWith(_MutableSessionController.new),
        notificationRepositoryProvider.overrideWithValue(repository),
      ],
    );
    final subscription = container.listen(
      notificationControllerProvider,
      (_, _) {},
      fireImmediately: true,
    );
    addTearDown(() {
      subscription.close();
      container.dispose();
    });

    final firstLoad = container
        .read(notificationControllerProvider.notifier)
        .load();
    await _waitFor(() => repository.requests.length == 1);
    final session = container.read(
      sessionControllerProvider.notifier,
    ) as _MutableSessionController;
    session.authenticate('user-b', _secondWorld);

    repository.requests.single.complete(
      NotificationPage(
        items: [_notification('old-notification')],
        nextCursor: null,
        hasMore: false,
      ),
    );
    await firstLoad;

    expect(container.read(notificationControllerProvider).items, isEmpty);
  });

  test(
    'pagination reconciles duplicate notifications deterministically',
    () async {
      final repository = _DelayedRepository();
      final container = ProviderContainer(
        overrides: [
          sessionControllerProvider.overrideWith(_MutableSessionController.new),
          notificationRepositoryProvider.overrideWithValue(repository),
        ],
      );
      final subscription = container.listen(
        notificationControllerProvider,
        (_, _) {},
        fireImmediately: true,
      );
      addTearDown(() {
        subscription.close();
        container.dispose();
      });

      final load = container
          .read(notificationControllerProvider.notifier)
          .load();
      await _waitFor(() => repository.requests.length == 1);
      repository.requests[0].complete(
        NotificationPage(
          items: [_notification('b')],
          nextCursor: 'next',
          hasMore: true,
        ),
      );
      await load;

      final more = container
          .read(notificationControllerProvider.notifier)
          .loadMore();
      await _waitFor(() => repository.requests.length == 2);
      repository.requests[1].complete(
        NotificationPage(
          items: [_notification('b'), _notification('a', minute: 1)],
          nextCursor: null,
          hasMore: false,
        ),
      );
      await more;

      expect(
        container
            .read(notificationControllerProvider)
            .items
            .map((item) => item.id),
        ['a', 'b'],
      );
    },
  );
}

Future<void> _waitFor(bool Function() condition) async {
  for (var attempt = 0; attempt < 100 && !condition(); attempt++) {
    await Future<void>.delayed(Duration.zero);
  }
  expect(condition(), isTrue);
}

AppNotification _notification(String id, {int minute = 0}) => AppNotification(
  id: id,
  category: 'reply',
  title: 'New reply',
  resourceId: 'resource-$id',
  isRead: false,
  createdAtUtc: DateTime.utc(2026, 10, 11, 4, minute),
);

final _secondWorld = WorldSummary(
  id: '00000000-0000-0000-0000-000000000510',
  name: 'Second Parallel World',
  status: 'Active',
  currentGameTimeUtc: testNow,
  playerActorId: '00000000-0000-0000-0000-000000000511',
  playerDisplayName: 'Second Player',
  createdAtUtc: testNow,
);
