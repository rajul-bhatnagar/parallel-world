import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/features/messaging/application/messaging_contracts.dart';

final messagingRepositoryProvider = Provider<MessagingRepository>(
  (ref) => throw StateError('MessagingRepository was not wired by the app.'),
);
