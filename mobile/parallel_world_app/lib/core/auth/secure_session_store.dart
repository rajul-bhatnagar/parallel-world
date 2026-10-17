import 'dart:convert';

import 'package:parallel_world_app/core/auth/secret_generator.dart';
import 'package:parallel_world_app/core/auth/secure_key_value_store.dart';
import 'package:parallel_world_app/core/auth/session_credentials.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';

class SecureSessionStore {
  SecureSessionStore(this._store, this._secretGenerator);

  static const _installationKey = 'parallel_world.installation_id.v1';
  static const _credentialsKey = 'parallel_world.session_credentials.v1';
  static const _bootstrapProofKey = 'parallel_world.bootstrap_proof.v1';

  final SecureKeyValueStore _store;
  final SecretGenerator _secretGenerator;

  Future<String> loadOrCreateInstallationId() async {
    try {
      final existing = await _store.read(_installationKey);
      if (existing != null && existing.isNotEmpty) {
        return existing;
      }

      final installationId = _secretGenerator.newInstallationId();
      await _store.write(_installationKey, installationId);
      return installationId;
    } catch (_) {
      throw const SecureStorageFailure();
    }
  }

  Future<SessionCredentials?> readCredentials() async {
    try {
      final raw = await _store.read(_credentialsKey);
      if (raw == null) {
        return null;
      }
      final json = jsonDecode(raw);
      if (json is! Map<String, Object?>) {
        throw const FormatException('Invalid session data.');
      }
      return SessionCredentials.fromJson(json);
    } catch (_) {
      throw const SecureStorageFailure();
    }
  }

  Future<void> writeCredentials(SessionCredentials credentials) async {
    try {
      await _store.write(_credentialsKey, jsonEncode(credentials.toJson()));
    } catch (_) {
      throw const SecureStorageFailure();
    }
  }

  Future<BootstrapProof> loadOrCreateBootstrapProof() async {
    try {
      final raw = await _store.read(_bootstrapProofKey);
      if (raw != null) {
        final json = jsonDecode(raw);
        if (json is Map<String, Object?>) {
          return BootstrapProof.fromJson(json);
        }
      }

      final proof = BootstrapProof(
        value: _secretGenerator.newBootstrapProof(),
        attempts: 0,
      );
      await _store.write(_bootstrapProofKey, jsonEncode(proof.toJson()));
      return proof;
    } catch (_) {
      throw const SecureStorageFailure();
    }
  }

  Future<BootstrapProof> beginBootstrapAttempt() async {
    final proof = await loadOrCreateBootstrapProof();
    if (proof.attempts >= 2) {
      throw const SessionRecoveryFailure(
        'Guest session recovery was already attempted. Start a new session to continue.',
      );
    }

    final attempted = BootstrapProof(
      value: proof.value,
      attempts: proof.attempts + 1,
    );
    try {
      await _store.write(_bootstrapProofKey, jsonEncode(attempted.toJson()));
      return attempted;
    } catch (_) {
      throw const SecureStorageFailure();
    }
  }

  Future<void> discardBootstrapProof() async {
    try {
      await _store.delete(_bootstrapProofKey);
    } catch (_) {
      throw const SecureStorageFailure();
    }
  }

  Future<void> clearSession({bool clearInstallation = false}) async {
    try {
      await _store.delete(_credentialsKey);
      await _store.delete(_bootstrapProofKey);
      if (clearInstallation) {
        await _store.delete(_installationKey);
      }
    } catch (_) {
      throw const SecureStorageFailure();
    }
  }
}

class BootstrapProof {
  const BootstrapProof({required this.value, required this.attempts});

  factory BootstrapProof.fromJson(Map<String, Object?> json) {
    final value = json['value'];
    final attempts = json['attempts'];
    if (value is! String || value.isEmpty || attempts is! int) {
      throw const FormatException('Invalid bootstrap proof data.');
    }
    return BootstrapProof(value: value, attempts: attempts);
  }

  final String value;
  final int attempts;

  Map<String, Object?> toJson() => {'value': value, 'attempts': attempts};
}
