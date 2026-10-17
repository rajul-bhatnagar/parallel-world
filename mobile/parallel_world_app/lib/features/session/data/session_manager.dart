import 'package:parallel_world_app/core/auth/secure_session_store.dart';
import 'package:parallel_world_app/core/auth/session_credentials.dart';
import 'package:parallel_world_app/core/config/app_config.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/session/application/session_contracts.dart';

class SessionManager implements SessionLifecycle {
  factory SessionManager({
    required SecureSessionStore sessionStore,
    required AuthGateway authApi,
    required AppConfig config,
    DateTime Function()? utcNow,
  }) =>
      SessionManager._(sessionStore, authApi, config, utcNow ?? _defaultUtcNow);

  SessionManager._(
    this._sessionStore,
    this._authApi,
    this._config,
    this._utcNow,
  );

  final SecureSessionStore _sessionStore;
  final AuthGateway _authApi;
  final AppConfig _config;
  final DateTime Function() _utcNow;
  Future<String>? _refreshFuture;

  @override
  Future<String> ensureInstallationId() =>
      _sessionStore.loadOrCreateInstallationId();

  @override
  Future<SessionCredentials?> restore() => _sessionStore.readCredentials();

  @override
  Future<GuestSessionResponse> bootstrapGuest() async {
    final installationId = await _sessionStore.loadOrCreateInstallationId();
    final proof = await _sessionStore.beginBootstrapAttempt();
    final response = await _authApi.bootstrapGuest(
      installationId: installationId,
      appVersion: _config.appVersion,
      bootstrapProof: proof.value,
      worldName: _config.initialWorldName,
    );
    final credentials = SessionCredentials(
      userId: response.userId,
      accessToken: response.tokens.accessToken,
      accessTokenExpiresAtUtc: response.tokens.accessTokenExpiresAtUtc,
      refreshToken: response.tokens.refreshToken,
      refreshTokenExpiresAtUtc: response.tokens.refreshTokenExpiresAtUtc,
    );
    await _sessionStore.writeCredentials(credentials);
    await _sessionStore.discardBootstrapProof();
    return response;
  }

  @override
  Future<AccountResponse> register(String email, String password) =>
      _authApi.register(email: email, password: password);

  @override
  Future<GuestSessionResponse> login(String email, String password) async {
    final previous = await _sessionStore.readCredentials();
    final response = await _authApi.login(
      email: email,
      password: password,
      installationId: await _sessionStore.loadOrCreateInstallationId(),
      appVersion: _config.appVersion,
    );
    await _sessionStore.writeCredentials(
      SessionCredentials(
        userId: response.userId,
        accessToken: response.tokens.accessToken,
        accessTokenExpiresAtUtc: response.tokens.accessTokenExpiresAtUtc,
        refreshToken: response.tokens.refreshToken,
        refreshTokenExpiresAtUtc: response.tokens.refreshTokenExpiresAtUtc,
      ),
    );
    if (previous?.userId != response.userId) {
      await _sessionStore.discardBootstrapProof();
    }
    return response;
  }

  @override
  Future<AccountResponse> upgrade(
    String email,
    String password,
    String idempotencyKey,
  ) async {
    final credentials = await _sessionStore.readCredentials();
    if (credentials == null) {
      throw const AuthenticationFailure(code: 'authentication_required');
    }
    return _authApi.upgrade(
      email: email,
      password: password,
      accessToken: credentials.accessToken,
      idempotencyKey: idempotencyKey,
    );
  }

  @override
  Future<String> recover(
    String email,
    String recoveryCode,
    String newPassword,
  ) => _authApi.recover(
    email: email,
    recoveryCode: recoveryCode,
    newPassword: newPassword,
  );

  Future<SessionCredentials> _requiredCredentials() async =>
      await _sessionStore.readCredentials() ??
      (throw const AuthenticationFailure(code: 'authentication_required'));

  @override
  Future<void> changePassword(
    String currentPassword,
    String newPassword,
  ) async {
    final credentials = await _requiredCredentials();
    await _authApi.changePassword(
      accessToken: credentials.accessToken,
      currentPassword: currentPassword,
      newPassword: newPassword,
    );
    await _sessionStore.clearSession();
  }

  @override
  Future<String> regenerateRecoveryCode() async => _authApi
      .regenerateRecoveryCode((await _requiredCredentials()).accessToken);

  @override
  Future<List<AccountSession>> listSessions() async =>
      _authApi.listSessions((await _requiredCredentials()).accessToken);

  @override
  Future<void> revokeSession(String sessionId) async => _authApi.revokeSession(
    (await _requiredCredentials()).accessToken,
    sessionId,
  );

  @override
  Future<void> logoutAll() async {
    final credentials = await _requiredCredentials();
    await _authApi.logoutAll(credentials.accessToken);
    await _sessionStore.clearSession();
  }

  @override
  Future<String?> readAccessToken() async =>
      (await _sessionStore.readCredentials())?.accessToken;

  @override
  Future<String> refreshSingleFlight() {
    final active = _refreshFuture;
    if (active != null) {
      return active;
    }

    final refresh = _refresh();
    _refreshFuture = refresh;
    return refresh.whenComplete(() {
      if (identical(_refreshFuture, refresh)) {
        _refreshFuture = null;
      }
    });
  }

  @override
  Future<void> logout() async {
    var credentials = await _sessionStore.readCredentials();
    if (credentials == null) {
      await _sessionStore.clearSession();
      return;
    }

    var refreshedForLogout = false;
    if (!credentials.hasUsableAccessToken(_utcNow())) {
      credentials = await _refreshForLogout();
      if (credentials == null) {
        return;
      }
      refreshedForLogout = true;
    }

    try {
      await _authApi.logout(
        accessToken: credentials.accessToken,
        refreshToken: credentials.refreshToken,
      );
    } on AuthenticationFailure {
      if (refreshedForLogout) {
        rethrow;
      }

      credentials = await _refreshForLogout();
      if (credentials == null) {
        return;
      }
      await _authApi.logout(
        accessToken: credentials.accessToken,
        refreshToken: credentials.refreshToken,
      );
    }
    await _sessionStore.clearSession();
  }

  @override
  Future<void> clearLocalSession() => _sessionStore.clearSession();

  Future<String> _refresh() async {
    final credentials = await _sessionStore.readCredentials();
    if (credentials == null || !credentials.hasUsableRefreshToken(_utcNow())) {
      throw const AuthenticationFailure(code: 'refresh_token_expired');
    }

    // Persist this before transmission. If the response is lost or the process
    // exits, this refresh token is never replayed automatically.
    await _sessionStore.writeCredentials(
      credentials.markRefreshMayBeConsumed(),
    );
    final tokens = await _authApi.refresh(credentials.refreshToken);
    final replacement = SessionCredentials(
      userId: credentials.userId,
      accessToken: tokens.accessToken,
      accessTokenExpiresAtUtc: tokens.accessTokenExpiresAtUtc,
      refreshToken: tokens.refreshToken,
      refreshTokenExpiresAtUtc: tokens.refreshTokenExpiresAtUtc,
    );
    await _sessionStore.writeCredentials(replacement);
    return replacement.accessToken;
  }

  Future<SessionCredentials?> _refreshForLogout() async {
    final credentials = await _sessionStore.readCredentials();
    if (credentials == null) {
      return null;
    }
    if (!credentials.hasUsableRefreshToken(_utcNow())) {
      throw const SessionRecoveryFailure(
        'This session cannot be revoked automatically. Start a new guest session to continue.',
      );
    }

    try {
      await refreshSingleFlight();
    } on AuthenticationFailure {
      // A rejected refresh token cannot mint another access token. The family
      // is expired, revoked, or replay-contained, so no active local session
      // remains to revoke.
      await _sessionStore.clearSession();
      return null;
    }

    final replacement = await _sessionStore.readCredentials();
    if (replacement == null) {
      throw const SecureStorageFailure();
    }
    return replacement;
  }

  static DateTime _defaultUtcNow() => DateTime.now().toUtc();
}
