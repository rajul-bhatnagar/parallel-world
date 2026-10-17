import 'package:drift/native.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';
import 'package:parallel_world_app/features/world/data/world_cache.dart';

import '../../support/fakes.dart';

void main() {
  late AppDatabase database;
  late DriftWorldCache cache;

  setUp(() {
    database = AppDatabase(NativeDatabase.memory());
    cache = DriftWorldCache(database, utcNow: () => testNow);
  });

  tearDown(() => database.close());

  test('persists only a user-scoped world projection', () async {
    await cache.initialize();
    await cache.write('user-1', testWorld);

    expect((await cache.read('user-1'))?.id, testWorld.id);
    expect(await cache.read('user-2'), isNull);

    final columns = await database
        .customSelect("PRAGMA table_info('cached_worlds')")
        .get();
    final names = columns.map((row) => row.read<String>('name')).toSet();
    expect(names, containsAll({'user_id', 'world_id', 'name'}));
    expect(
      names.any((name) => name.contains('token') || name.contains('proof')),
      isFalse,
    );
  });

  test('clear removes cached private data', () async {
    await cache.write('user-1', testWorld);
    await cache.clear();

    expect(await cache.read('user-1'), isNull);
  });
}
