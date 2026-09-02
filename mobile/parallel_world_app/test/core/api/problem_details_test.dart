import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/core/api/problem_details.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';

void main() {
  test('maps validation ProblemDetails without exposing server detail', () {
    final failure = ApiProblemDetails.fromJson({
      'status': 400,
      'code': 'validation_failed',
      'traceId': 'trace-1',
      'detail': 'sensitive internal detail',
      'errors': {
        'name': ['Name is required.'],
      },
    }).toFailure();

    expect(failure, isA<ValidationFailure>());
    expect((failure as ValidationFailure).errors['name'], [
      'Name is required.',
    ]);
    expect(failure.message, isNot(contains('sensitive')));
    expect(failure.traceId, 'trace-1');
  });

  test('maps retry metadata for rate limiting', () {
    final failure = ApiProblemDetails.fromJson({
      'status': 429,
      'code': 'rate_limited',
      'retryAfterSeconds': 17,
    }).toFailure();

    expect(failure, isA<RateLimitFailure>());
    expect(
      (failure as RateLimitFailure).retryAfter,
      const Duration(seconds: 17),
    );
  });
}
