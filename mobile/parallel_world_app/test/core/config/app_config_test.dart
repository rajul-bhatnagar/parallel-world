import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/config/app_config.dart';

void main() {
  group('AppConfig', () {
    test('normalizes valid values', () {
      final config = AppConfig.fromValues(
        apiBaseUrl: 'https://api.example.test/',
        environment: 'STAGING',
      );

      expect(config.apiBaseUrl, 'https://api.example.test');
      expect(config.environment, 'staging');
    });

    test('rejects a missing API base URL', () {
      expect(
        () => AppConfig.fromValues(apiBaseUrl: ''),
        throwsA(isA<AppConfigException>()),
      );
    });

    test('requires HTTPS in production', () {
      expect(
        () => AppConfig.fromValues(
          apiBaseUrl: 'http://api.example.test',
          environment: 'production',
        ),
        throwsA(isA<AppConfigException>()),
      );
    });

    test(
      'release configuration requires an explicit production environment',
      () {
        for (final environment in ['', 'local']) {
          expect(
            () => AppConfig.fromValues(
              apiBaseUrl: 'https://api.example.test',
              environment: environment,
              releaseMode: true,
            ),
            throwsA(isA<AppConfigException>()),
          );
        }
      },
    );

    test(
      'release environment loading fails closed when defines are absent',
      () {
        expect(
          () => AppConfig.fromEnvironment(forceReleaseMode: true),
          throwsA(isA<AppConfigException>()),
        );
      },
    );

    test(
      'release configuration rejects HTTP even when production is explicit',
      () {
        expect(
          () => AppConfig.fromValues(
            apiBaseUrl: 'http://api.example.test',
            environment: 'production',
            releaseMode: true,
          ),
          throwsA(isA<AppConfigException>()),
        );
      },
    );

    test('release configuration accepts explicit production HTTPS', () {
      final config = AppConfig.fromValues(
        apiBaseUrl: 'https://api.parallelworld.app',
        environment: 'production',
        releaseMode: true,
      );

      expect(config.environment, 'production');
      expect(config.apiBaseUrl, 'https://api.parallelworld.app');
    });

    test('production rejects local, Development, and Staging endpoints', () {
      for (final endpoint in [
        'https://localhost:8080',
        'https://localhost.',
        'https://127.0.0.1',
        'https://127.0.0.2',
        'https://[::1]',
        'https://api.example.test',
        'https://api.example.test.',
        'https://api.example',
        'https://api.invalid',
        'https://api.localhost',
        'https://api-staging.parallelworld.app',
        'https://staging-api.parallelworld.app',
        'https://api.stage.parallelworld.app',
        'https://dev-api.parallelworld.app',
        'https://api.development.parallelworld.app',
      ]) {
        expect(
          () => AppConfig.fromValues(
            apiBaseUrl: endpoint,
            environment: 'production',
          ),
          throwsA(isA<AppConfigException>()),
        );
      }
    });

    test('debug configuration defaults an omitted environment to local', () {
      final config = AppConfig.fromValues(
        apiBaseUrl: 'http://10.0.2.2:8080',
        environment: '',
      );

      expect(config.environment, 'local');
    });

    test('rejects API URLs containing embedded user info', () {
      expect(
        () => AppConfig.fromValues(
          apiBaseUrl: 'https://username:password@example.test',
        ),
        throwsA(isA<AppConfigException>()),
      );
    });

    test('accepts timeout values within the documented bounds', () {
      final config = AppConfig.fromValues(
        apiBaseUrl: 'https://api.example.test',
        connectTimeoutMilliseconds: AppConfig.minimumTimeoutMilliseconds,
        sendTimeoutMilliseconds: 15000,
        receiveTimeoutMilliseconds: AppConfig.maximumTimeoutMilliseconds,
      );

      expect(
        config.connectTimeout.inMilliseconds,
        AppConfig.minimumTimeoutMilliseconds,
      );
      expect(
        config.receiveTimeout.inMilliseconds,
        AppConfig.maximumTimeoutMilliseconds,
      );
    });

    test('rejects non-positive and excessive timeout values', () {
      for (final timeout in [0, -1, AppConfig.maximumTimeoutMilliseconds + 1]) {
        expect(
          () => AppConfig.fromValues(
            apiBaseUrl: 'https://api.example.test',
            connectTimeoutMilliseconds: timeout,
          ),
          throwsA(isA<AppConfigException>()),
        );
      }
    });
  });
}
