sealed class AppFailure implements Exception {
  const AppFailure(this.message, {this.code, this.traceId});

  final String message;
  final String? code;
  final String? traceId;

  @override
  String toString() => message;
}

final class NetworkFailure extends AppFailure {
  const NetworkFailure()
    : super(
        'The service could not be reached. Check your connection and retry.',
      );
}

final class AuthenticationFailure extends AppFailure {
  const AuthenticationFailure({super.code, super.traceId})
    : super('Your session is no longer available.');
}

final class AuthorizationFailure extends AppFailure {
  const AuthorizationFailure({super.code, super.traceId})
    : super('This resource is not available.');
}

final class NotFoundFailure extends AppFailure {
  const NotFoundFailure({super.code, super.traceId})
    : super('The requested resource is not available.');
}

final class ValidationFailure extends AppFailure {
  const ValidationFailure(this.errors, {super.code, super.traceId})
    : super('Some information needs attention.');

  final Map<String, List<String>> errors;
}

final class ConflictFailure extends AppFailure {
  const ConflictFailure({super.code, super.traceId})
    : super('The request conflicts with the current server state.');
}

final class RateLimitFailure extends AppFailure {
  const RateLimitFailure({this.retryAfter, super.code, super.traceId})
    : super('Too many requests were made. Wait before retrying.');

  final Duration? retryAfter;
}

final class ServerFailure extends AppFailure {
  const ServerFailure({super.code, super.traceId})
    : super('The service is temporarily unavailable.');
}

final class SecureStorageFailure extends AppFailure {
  const SecureStorageFailure()
    : super('Secure storage is unavailable. The app cannot start safely.');
}

final class SessionRecoveryFailure extends AppFailure {
  const SessionRecoveryFailure(super.message);
}

final class UnknownFailure extends AppFailure {
  const UnknownFailure()
    : super('Something unexpected happened. Please retry.');
}
