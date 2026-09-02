import 'dart:io';

import 'package:flutter_test/flutter_test.dart';

void main() {
  test('feature application does not import the app composition root', () {
    final controller = File(
      'lib/features/session/application/session_controller.dart',
    ).readAsStringSync();

    expect(controller, isNot(contains('package:parallel_world_app/app/')));
  });

  test('core does not import feature code', () {
    final violations = Directory('lib/core')
        .listSync(recursive: true)
        .whereType<File>()
        .where((file) => file.path.endsWith('.dart'))
        .where(
          (file) => file.readAsStringSync().contains(
            'package:parallel_world_app/features/',
          ),
        )
        .map((file) => file.path)
        .toList();

    expect(violations, isEmpty);
    expect(
      File('lib/features/world/data/cache/app_database.dart').existsSync(),
      isTrue,
    );
    expect(File('lib/core/cache/app_database.dart').existsSync(), isFalse);
  });
}
