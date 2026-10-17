import 'dart:developer' as developer;

abstract interface class SafeLogSink {
  void record(SafeLogEvent event);
}

class SafeLogger implements SafeLogSink {
  const SafeLogger({this.enabled = false});

  final bool enabled;

  @override
  void record(SafeLogEvent event) {
    if (!enabled) {
      return;
    }

    developer.log(event.render(), name: 'parallel_world.network');
  }
}

class SafeLogEvent {
  const SafeLogEvent({
    required this.method,
    required this.route,
    required this.duration,
    this.statusCode,
    this.correlationId,
  });

  final String method;
  final String route;
  final Duration duration;
  final int? statusCode;
  final String? correlationId;

  String render() =>
      '$method $route ${statusCode ?? '-'} '
      '${duration.inMilliseconds}ms ${correlationId ?? '-'}';
}
