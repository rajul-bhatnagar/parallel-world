class CatchUpSummaryItem {
  const CatchUpSummaryItem({required this.itemType, required this.wording});

  final String itemType;
  final String wording;

  factory CatchUpSummaryItem.fromJson(Map<String, Object?> json) =>
      CatchUpSummaryItem(
        itemType: json['itemType']! as String,
        wording: json['wording']! as String,
      );
}

class CatchUpSummary {
  const CatchUpSummary({
    required this.status,
    required this.text,
    required this.items,
  });

  final String status;
  final String text;
  final List<CatchUpSummaryItem> items;

  factory CatchUpSummary.fromJson(Map<String, Object?> json) => CatchUpSummary(
    status: json['status']! as String,
    text: json['text']! as String,
    items: (json['items']! as List<Object?>)
        .map((item) => CatchUpSummaryItem.fromJson(_object(item)))
        .toList(growable: false),
  );
}

class CatchUpResult {
  const CatchUpResult({
    required this.disposition,
    required this.processedIntervals,
    required this.remainingIntervals,
    this.summary,
    this.errorCode,
  });

  final String disposition;
  final int processedIntervals;
  final int remainingIntervals;
  final CatchUpSummary? summary;
  final String? errorCode;

  bool get isPartial => disposition == 'partial' || disposition == 'processing';

  factory CatchUpResult.fromJson(Map<String, Object?> json) => CatchUpResult(
    disposition: json['disposition']! as String,
    processedIntervals: json['processedIntervals']! as int,
    remainingIntervals: json['remainingIntervals']! as int,
    summary: json['summary'] == null
        ? null
        : CatchUpSummary.fromJson(_object(json['summary'])),
    errorCode: json['errorCode'] as String?,
  );
}

Map<String, Object?> _object(Object? value) {
  if (value is! Map) throw const FormatException('Expected a JSON object.');
  return <String, Object?>{
    for (final entry in value.entries)
      if (entry.key is String) entry.key as String: entry.value,
  };
}
