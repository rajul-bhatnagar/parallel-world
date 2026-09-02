import 'package:dio/dio.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/api/api_client.dart';
import 'package:parallel_world_app/core/api/auth_interceptor.dart';
import 'package:parallel_world_app/core/auth/secure_key_value_store.dart';
import 'package:parallel_world_app/core/auth/secure_session_store.dart';
import 'package:parallel_world_app/core/config/app_config.dart';
import 'package:parallel_world_app/core/logging/safe_logger.dart';
import 'package:parallel_world_app/features/session/application/session_contracts.dart';
import 'package:parallel_world_app/features/session/application/session_dependencies.dart';
import 'package:parallel_world_app/features/session/data/auth_api.dart';
import 'package:parallel_world_app/features/session/data/session_manager.dart';
import 'package:parallel_world_app/features/world/application/world_contracts.dart';
import 'package:parallel_world_app/features/world/data/cache/app_database.dart';
import 'package:parallel_world_app/features/world/data/world_api.dart';
import 'package:parallel_world_app/features/world/data/world_cache.dart';

final appConfigProvider = Provider<AppConfig>(
  (ref) => AppConfig.fromEnvironment(),
);

final safeLogSinkProvider = Provider<SafeLogSink>((ref) {
  final config = ref.watch(appConfigProvider);
  return SafeLogger(enabled: config.environment == 'local');
});

final secureKeyValueStoreProvider = Provider<SecureKeyValueStore>(
  (ref) => FlutterSecureKeyValueStore(),
);

final secureSessionStoreProvider = Provider<SecureSessionStore>(
  (ref) => SecureSessionStore(
    ref.watch(secureKeyValueStoreProvider),
    ref.watch(secretGeneratorProvider),
  ),
);

final publicDioProvider = Provider<Dio>((ref) {
  final dio = createApiDio(
    config: ref.watch(appConfigProvider),
    secretGenerator: ref.watch(secretGeneratorProvider),
    logger: ref.watch(safeLogSinkProvider),
  );
  ref.onDispose(() => dio.close(force: true));
  return dio;
});

final authApiProvider = Provider<AuthGateway>(
  (ref) => AuthApi(ref.watch(publicDioProvider)),
);

final _sessionManagerImplementationProvider = Provider<SessionLifecycle>(
  (ref) => SessionManager(
    sessionStore: ref.watch(secureSessionStoreProvider),
    authApi: ref.watch(authApiProvider),
    config: ref.watch(appConfigProvider),
    utcNow: ref.watch(utcNowProvider),
  ),
);

final authenticatedDioProvider = Provider<Dio>((ref) {
  final dio = createApiDio(
    config: ref.watch(appConfigProvider),
    secretGenerator: ref.watch(secretGeneratorProvider),
    logger: ref.watch(safeLogSinkProvider),
  );
  dio.interceptors.insert(
    1,
    AuthInterceptor(dio, ref.watch(sessionLifecycleProvider)),
  );
  ref.onDispose(() => dio.close(force: true));
  return dio;
});

final _worldApiImplementationProvider = Provider<WorldGateway>(
  (ref) => WorldApi(ref.watch(authenticatedDioProvider)),
);

final appDatabaseProvider = Provider<AppDatabase>((ref) {
  final database = AppDatabase();
  ref.onDispose(database.close);
  return database;
});

final _worldCacheImplementationProvider = Provider<WorldCache>(
  (ref) => DriftWorldCache(
    ref.watch(appDatabaseProvider),
    utcNow: ref.watch(utcNowProvider),
  ),
);

Widget buildAppScope({required AppConfig config, required Widget child}) =>
    ProviderScope(
      overrides: [
        appConfigProvider.overrideWithValue(config),
        sessionLifecycleProvider.overrideWith(
          (ref) => ref.watch(_sessionManagerImplementationProvider),
        ),
        worldGatewayProvider.overrideWith(
          (ref) => ref.watch(_worldApiImplementationProvider),
        ),
        worldCacheProvider.overrideWith(
          (ref) => ref.watch(_worldCacheImplementationProvider),
        ),
      ],
      child: child,
    );
