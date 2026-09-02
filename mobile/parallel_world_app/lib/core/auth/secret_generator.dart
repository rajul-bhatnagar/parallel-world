import 'dart:convert';
import 'dart:math';

class SecretGenerator {
  SecretGenerator({Random? random}) : _random = random ?? Random.secure();

  final Random _random;

  String newBootstrapProof() =>
      base64Url.encode(_bytes(32)).replaceAll('=', '');

  String newCorrelationId() => _hex(_bytes(16));

  String newIdempotencyKey() => _hex(_bytes(16));

  String newInstallationId() {
    final bytes = _bytes(16);
    bytes[6] = (bytes[6] & 0x0f) | 0x40;
    bytes[8] = (bytes[8] & 0x3f) | 0x80;
    final value = _hex(bytes);
    return '${value.substring(0, 8)}-${value.substring(8, 12)}-'
        '${value.substring(12, 16)}-${value.substring(16, 20)}-'
        '${value.substring(20)}';
  }

  List<int> _bytes(int length) =>
      List<int>.generate(length, (_) => _random.nextInt(256));

  static String _hex(List<int> bytes) =>
      bytes.map((value) => value.toRadixString(16).padLeft(2, '0')).join();
}
