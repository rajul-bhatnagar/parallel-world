import 'dart:io';

import 'package:drift/native.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/characters/data/character_cache.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';

import '../../support/character_fakes.dart';
import '../../support/fakes.dart';

void main() {
  late AppDatabase database;
  late DriftCharacterCache cache;

  setUp(() {
    database = AppDatabase(NativeDatabase.memory());
    cache = DriftCharacterCache(database, utcNow: () => testNow);
  });

  tearDown(() => database.close());

  test('catalogue and detail cache are isolated by user and world', () async {
    await cache.replaceCatalogue('user-a', testWorld.id, [testCharacter]);
    await cache.writeDetails('user-a', testWorld.id, testCharacterDetails);

    expect(
      (await cache.readCatalogue('user-a', testWorld.id))!.items.single.id,
      testCharacter.id,
    );
    expect(
      (await cache.readDetails(
        'user-a',
        testWorld.id,
        testCharacter.id,
      ))!.value.bio,
      testCharacterDetails.bio,
    );
    expect(await cache.readCatalogue('user-b', testWorld.id), isNull);
    expect(await cache.readCatalogue('user-a', 'foreign-world'), isNull);
    expect(
      await cache.readDetails('user-b', testWorld.id, testCharacter.id),
      isNull,
    );
  });

  test('empty catalogue remains a cached offline-readable result', () async {
    await cache.replaceCatalogue('user-a', testWorld.id, []);

    final cached = await cache.readCatalogue('user-a', testWorld.id);

    expect(cached, isNotNull);
    expect(cached!.items, isEmpty);
    expect(cached.cachedAtUtc, testNow);
  });

  test(
    'private-data clear removes character rows with the world cache',
    () async {
      await cache.replaceCatalogue('user-a', testWorld.id, [testCharacter]);
      await cache.writeDetails('user-a', testWorld.id, testCharacterDetails);

      await database.clearPrivateData();

      expect(await cache.readCatalogue('user-a', testWorld.id), isNull);
      expect(
        await cache.readDetails('user-a', testWorld.id, testCharacter.id),
        isNull,
      );
    },
  );

  test('schema version one upgrades with M05 character cache tables', () async {
    await database.close();
    final directory = await Directory.systemTemp.createTemp('m05_drift_');
    final file = File('${directory.path}/cache.sqlite');
    database = AppDatabase(NativeDatabase(file));
    await database.initialize();
    await database.customStatement('DROP TABLE cached_character_catalogues');
    await database.customStatement('DROP TABLE cached_character_summaries');
    await database.customStatement('DROP TABLE cached_character_details');
    await database.customStatement('PRAGMA user_version = 1');
    await database.close();
    database = AppDatabase(NativeDatabase(file));

    await database.initialize();

    final rows = await database
        .customSelect("SELECT name FROM sqlite_master WHERE type = 'table'")
        .get();
    final names = rows.map((row) => row.read<String>('name')).toSet();
    expect(
      names,
      containsAll({
        'cached_character_catalogues',
        'cached_character_summaries',
        'cached_character_details',
      }),
    );
    await database.close();
    await directory.delete(recursive: true);
    database = AppDatabase(NativeDatabase.memory());
  });
}
