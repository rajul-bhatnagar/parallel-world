import 'dart:io';

import 'package:flutter_test/flutter_test.dart';

void main() {
  String read(String path) => File(path).readAsStringSync();

  test(
    'release-effective manifest declares Internet without cleartext opt-in',
    () {
      final manifest = read('android/app/src/main/AndroidManifest.xml');

      expect(manifest, contains('android.permission.INTERNET'));
      expect(
        manifest,
        contains('android:fullBackupContent="@xml/backup_rules"'),
      );
      expect(
        manifest,
        contains('android:dataExtractionRules="@xml/data_extraction_rules"'),
      );
      expect(manifest, isNot(contains('usesCleartextTraffic')));
      expect(manifest, isNot(contains('networkSecurityConfig')));
    },
  );

  test('debug cleartext exception is restricted to the emulator host', () {
    final manifest = read('android/app/src/debug/AndroidManifest.xml');
    final security = read(
      'android/app/src/debug/res/xml/network_security_config.xml',
    );

    expect(
      manifest,
      contains('android:networkSecurityConfig="@xml/network_security_config"'),
    );
    expect(security, contains('cleartextTrafficPermitted="true"'));
    expect(security, contains('>10.0.2.2</domain>'));
    expect(RegExp(r'<domain ').allMatches(security), hasLength(1));
    expect(security, isNot(contains('<base-config')));
  });

  test('secure-storage preferences are excluded from backup and transfer', () {
    final legacy = read('android/app/src/main/res/xml/backup_rules.xml');
    final modern = read(
      'android/app/src/main/res/xml/data_extraction_rules.xml',
    );

    expect(modern, contains('<cloud-backup>'));
    expect(modern, contains('<device-transfer>'));
    for (final preference in [
      'FlutterSecureStorage.xml',
      'FlutterSecureStorageConfiguration.xml',
      'FlutterSecureKeyStorage.xml',
    ]) {
      expect(legacy, contains('domain="sharedpref" path="$preference"'));
      expect(modern.split(preference), hasLength(3));
    }
  });

  test('Android minimum SDK remains 24', () {
    final gradle = read('android/app/build.gradle.kts');
    expect(gradle, contains('minSdk = 24'));
  });
}
