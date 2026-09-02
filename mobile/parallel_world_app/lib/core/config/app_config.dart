import 'package:flutter/foundation.dart';

class AppConfig {
  const AppConfig({
    required this.apiBaseUrl,
    required this.environment,
    required this.appVersion,
    required this.initialWorldName,
    required this.connectTimeout,
    required this.sendTimeout,
    required this.receiveTimeout,
  });

  static const minimumTimeoutMilliseconds = 100;
  static const maximumTimeoutMilliseconds = 60000;

  factory AppConfig.fromEnvironment({bool forceReleaseMode = false}) =>
      AppConfig.fromValues(
        apiBaseUrl: const String.fromEnvironment('API_BASE_URL'),
        environment: const String.fromEnvironment('APP_ENVIRONMENT'),
        appVersion: const String.fromEnvironment(
          'APP_VERSION',
          defaultValue: '0.1.0',
        ),
        initialWorldName: const String.fromEnvironment(
          'INITIAL_WORLD_NAME',
          defaultValue: 'My Parallel World',
        ),
        connectTimeoutMilliseconds: const int.fromEnvironment(
          'API_CONNECT_TIMEOUT_MS',
          defaultValue: 10000,
        ),
        sendTimeoutMilliseconds: const int.fromEnvironment(
          'API_SEND_TIMEOUT_MS',
          defaultValue: 15000,
        ),
        receiveTimeoutMilliseconds: const int.fromEnvironment(
          'API_RECEIVE_TIMEOUT_MS',
          defaultValue: 15000,
        ),
        releaseMode: kReleaseMode || forceReleaseMode,
      );

  factory AppConfig.fromValues({
    required String apiBaseUrl,
    String environment = 'local',
    String appVersion = '0.1.0',
    String initialWorldName = 'My Parallel World',
    int connectTimeoutMilliseconds = 10000,
    int sendTimeoutMilliseconds = 15000,
    int receiveTimeoutMilliseconds = 15000,
    bool releaseMode = false,
  }) {
    final suppliedEnvironment = environment.trim().toLowerCase();
    final normalizedEnvironment = suppliedEnvironment.isEmpty && !releaseMode
        ? 'local'
        : suppliedEnvironment;
    const environments = {'local', 'test', 'staging', 'production'};
    if (!environments.contains(normalizedEnvironment)) {
      throw const AppConfigException('APP_ENVIRONMENT is invalid.');
    }
    if (releaseMode && normalizedEnvironment != 'production') {
      throw const AppConfigException(
        'Release builds require APP_ENVIRONMENT=production.',
      );
    }

    final uri = Uri.tryParse(apiBaseUrl.trim());
    if (uri == null ||
        !uri.hasScheme ||
        !uri.hasAuthority ||
        uri.userInfo.isNotEmpty ||
        (uri.scheme != 'http' && uri.scheme != 'https')) {
      throw const AppConfigException('API_BASE_URL is missing or invalid.');
    }
    if ((releaseMode || normalizedEnvironment == 'production') &&
        uri.scheme != 'https') {
      throw const AppConfigException(
        'API_BASE_URL must use HTTPS in production.',
      );
    }
    if (uri.hasQuery || uri.hasFragment) {
      throw const AppConfigException(
        'API_BASE_URL must not contain a query or fragment.',
      );
    }
    if (appVersion.trim().isEmpty || appVersion.trim().length > 32) {
      throw const AppConfigException('APP_VERSION is missing or invalid.');
    }
    if (initialWorldName.trim().isEmpty ||
        initialWorldName.trim().length > 80) {
      throw const AppConfigException(
        'INITIAL_WORLD_NAME is missing or invalid.',
      );
    }
    if (!_isValidTimeout(connectTimeoutMilliseconds) ||
        !_isValidTimeout(sendTimeoutMilliseconds) ||
        !_isValidTimeout(receiveTimeoutMilliseconds)) {
      throw const AppConfigException(
        'API timeout values must be between 100 and 60000 milliseconds.',
      );
    }

    final normalizedBaseUrl = uri.toString().replaceFirst(RegExp(r'/+$'), '');
    return AppConfig(
      apiBaseUrl: normalizedBaseUrl,
      environment: normalizedEnvironment,
      appVersion: appVersion.trim(),
      initialWorldName: initialWorldName.trim(),
      connectTimeout: Duration(milliseconds: connectTimeoutMilliseconds),
      sendTimeout: Duration(milliseconds: sendTimeoutMilliseconds),
      receiveTimeout: Duration(milliseconds: receiveTimeoutMilliseconds),
    );
  }

  final String apiBaseUrl;
  final String environment;
  final String appVersion;
  final String initialWorldName;
  final Duration connectTimeout;
  final Duration sendTimeout;
  final Duration receiveTimeout;

  static bool _isValidTimeout(int milliseconds) =>
      milliseconds >= minimumTimeoutMilliseconds &&
      milliseconds <= maximumTimeoutMilliseconds;
}

class AppConfigException implements Exception {
  const AppConfigException(this.message);

  final String message;

  @override
  String toString() => message;
}
