import 'package:parallel_world_app/core/errors/app_failure.dart';

class ApiProblemDetails {
  const ApiProblemDetails({
    required this.status,
    required this.code,
    required this.traceId,
    this.errors = const {},
    this.retryAfterSeconds,
  });

  factory ApiProblemDetails.fromJson(
    Map<String, Object?> json, {
    int? responseStatus,
  }) {
    final statusValue = json['status'];
    final errorsValue = json['errors'];
    final errors = <String, List<String>>{};
    if (errorsValue is Map) {
      for (final entry in errorsValue.entries) {
        final value = entry.value;
        if (entry.key is String && value is List) {
          errors[entry.key as String] = value.whereType<String>().toList();
        }
      }
    }

    return ApiProblemDetails(
      status: statusValue is int ? statusValue : responseStatus ?? 500,
      code: json['code'] is String ? json['code']! as String : 'unknown_error',
      traceId: json['traceId'] is String ? json['traceId']! as String : null,
      errors: errors,
      retryAfterSeconds: json['retryAfterSeconds'] is int
          ? json['retryAfterSeconds']! as int
          : null,
    );
  }

  final int status;
  final String code;
  final String? traceId;
  final Map<String, List<String>> errors;
  final int? retryAfterSeconds;

  AppFailure toFailure() => switch (status) {
    400 => ValidationFailure(errors, code: code, traceId: traceId),
    401 => AuthenticationFailure(code: code, traceId: traceId),
    403 => AuthorizationFailure(code: code, traceId: traceId),
    404 => NotFoundFailure(code: code, traceId: traceId),
    409 => ConflictFailure(code: code, traceId: traceId),
    429 => RateLimitFailure(
      retryAfter: retryAfterSeconds == null
          ? null
          : Duration(seconds: retryAfterSeconds!),
      code: code,
      traceId: traceId,
    ),
    >= 500 => ServerFailure(code: code, traceId: traceId),
    _ => UnknownFailure(),
  };
}
