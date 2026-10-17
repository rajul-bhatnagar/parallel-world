import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/features/feed/application/feed_contracts.dart';

final feedRepositoryProvider = Provider<FeedRepository>(
  (ref) => throw StateError('FeedRepository was not wired by the app.'),
);
