class CharacterSummary {
  const CharacterSummary({
    required this.id,
    required this.displayName,
    required this.handle,
    required this.profession,
    required this.visibleMood,
    required this.isFollowed,
  });

  factory CharacterSummary.fromJson(Map<String, Object?> json) =>
      CharacterSummary(
        id: _requiredString(json, 'id'),
        displayName: _requiredString(json, 'displayName'),
        handle: _requiredString(json, 'handle'),
        profession: _requiredString(json, 'profession'),
        visibleMood: _requiredString(json, 'visibleMood'),
        isFollowed: _requiredBool(json, 'isFollowed'),
      );

  final String id;
  final String displayName;
  final String handle;
  final String profession;
  final String visibleMood;
  final bool isFollowed;
}

class CharacterDetails {
  const CharacterDetails({
    required this.id,
    required this.displayName,
    required this.handle,
    required this.bio,
    required this.age,
    required this.profession,
    required this.archetype,
    required this.visibleMood,
    required this.interests,
    required this.schedule,
  });

  factory CharacterDetails.fromJson(Map<String, Object?> json) {
    final interests = json['interests'];
    final schedule = json['schedule'];
    if (interests is! List || schedule is! List) {
      throw const FormatException('Expected character collections.');
    }
    return CharacterDetails(
      id: _requiredString(json, 'id'),
      displayName: _requiredString(json, 'displayName'),
      handle: _requiredString(json, 'handle'),
      bio: _requiredString(json, 'bio'),
      age: _requiredInt(json, 'age'),
      profession: _requiredString(json, 'profession'),
      archetype: _requiredString(json, 'archetype'),
      visibleMood: _requiredString(json, 'visibleMood'),
      interests: interests
          .map((item) => CharacterInterest.fromJson(_jsonObject(item)))
          .toList(growable: false),
      schedule: schedule
          .map((item) => CharacterSchedule.fromJson(_jsonObject(item)))
          .toList(growable: false),
    );
  }

  final String id;
  final String displayName;
  final String handle;
  final String bio;
  final int age;
  final String profession;
  final String archetype;
  final String visibleMood;
  final List<CharacterInterest> interests;
  final List<CharacterSchedule> schedule;
}

class CharacterInterest {
  const CharacterInterest(this.topicId);

  factory CharacterInterest.fromJson(Map<String, Object?> json) =>
      CharacterInterest(_requiredString(json, 'topicId'));

  final String topicId;
}

class CharacterSchedule {
  const CharacterSchedule({
    required this.dayOfWeek,
    required this.startLocalTime,
    required this.endLocalTime,
    required this.activity,
  });

  factory CharacterSchedule.fromJson(Map<String, Object?> json) =>
      CharacterSchedule(
        dayOfWeek: _requiredInt(json, 'dayOfWeek'),
        startLocalTime: _requiredString(json, 'startLocalTime'),
        endLocalTime: _requiredString(json, 'endLocalTime'),
        activity: _requiredString(json, 'activity'),
      );

  final int dayOfWeek;
  final String startLocalTime;
  final String endLocalTime;
  final String activity;
}

class CharacterPage {
  const CharacterPage({
    required this.items,
    required this.nextCursor,
    required this.hasMore,
  });

  factory CharacterPage.fromJson(Map<String, Object?> json) {
    final items = json['items'];
    if (items is! List) {
      throw const FormatException('Expected character items.');
    }
    return CharacterPage(
      items: items
          .map((item) => CharacterSummary.fromJson(_jsonObject(item)))
          .toList(growable: false),
      nextCursor: json['nextCursor'] as String?,
      hasMore: _requiredBool(json, 'hasMore'),
    );
  }

  final List<CharacterSummary> items;
  final String? nextCursor;
  final bool hasMore;
}

Map<String, Object?> _jsonObject(Object? value) {
  if (value is! Map) {
    throw const FormatException('Expected a JSON object.');
  }
  return <String, Object?>{
    for (final entry in value.entries)
      if (entry.key is String) entry.key as String: entry.value,
  };
}

String _requiredString(Map<String, Object?> json, String key) {
  final value = json[key];
  if (value is! String || value.isEmpty) {
    throw FormatException('Expected $key.');
  }
  return value;
}

int _requiredInt(Map<String, Object?> json, String key) {
  final value = json[key];
  if (value is! int) {
    throw FormatException('Expected $key.');
  }
  return value;
}

bool _requiredBool(Map<String, Object?> json, String key) {
  final value = json[key];
  if (value is! bool) {
    throw FormatException('Expected $key.');
  }
  return value;
}
