// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_database.dart';

// ignore_for_file: type=lint
class $CachedWorldsTable extends CachedWorlds
    with TableInfo<$CachedWorldsTable, CachedWorld> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $CachedWorldsTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _userIdMeta = const VerificationMeta('userId');
  @override
  late final GeneratedColumn<String> userId = GeneratedColumn<String>(
    'user_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _worldIdMeta = const VerificationMeta(
    'worldId',
  );
  @override
  late final GeneratedColumn<String> worldId = GeneratedColumn<String>(
    'world_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _nameMeta = const VerificationMeta('name');
  @override
  late final GeneratedColumn<String> name = GeneratedColumn<String>(
    'name',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 80,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _statusMeta = const VerificationMeta('status');
  @override
  late final GeneratedColumn<String> status = GeneratedColumn<String>(
    'status',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _currentGameTimeUtcMeta =
      const VerificationMeta('currentGameTimeUtc');
  @override
  late final GeneratedColumn<DateTime> currentGameTimeUtc =
      GeneratedColumn<DateTime>(
        'current_game_time_utc',
        aliasedName,
        false,
        type: DriftSqlType.dateTime,
        requiredDuringInsert: true,
      );
  static const VerificationMeta _playerActorIdMeta = const VerificationMeta(
    'playerActorId',
  );
  @override
  late final GeneratedColumn<String> playerActorId = GeneratedColumn<String>(
    'player_actor_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _playerDisplayNameMeta = const VerificationMeta(
    'playerDisplayName',
  );
  @override
  late final GeneratedColumn<String> playerDisplayName =
      GeneratedColumn<String>(
        'player_display_name',
        aliasedName,
        false,
        type: DriftSqlType.string,
        requiredDuringInsert: true,
      );
  static const VerificationMeta _createdAtUtcMeta = const VerificationMeta(
    'createdAtUtc',
  );
  @override
  late final GeneratedColumn<DateTime> createdAtUtc = GeneratedColumn<DateTime>(
    'created_at_utc',
    aliasedName,
    false,
    type: DriftSqlType.dateTime,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _cachedAtUtcMeta = const VerificationMeta(
    'cachedAtUtc',
  );
  @override
  late final GeneratedColumn<DateTime> cachedAtUtc = GeneratedColumn<DateTime>(
    'cached_at_utc',
    aliasedName,
    false,
    type: DriftSqlType.dateTime,
    requiredDuringInsert: true,
  );
  @override
  List<GeneratedColumn> get $columns => [
    userId,
    worldId,
    name,
    status,
    currentGameTimeUtc,
    playerActorId,
    playerDisplayName,
    createdAtUtc,
    cachedAtUtc,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'cached_worlds';
  @override
  VerificationContext validateIntegrity(
    Insertable<CachedWorld> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('user_id')) {
      context.handle(
        _userIdMeta,
        userId.isAcceptableOrUnknown(data['user_id']!, _userIdMeta),
      );
    } else if (isInserting) {
      context.missing(_userIdMeta);
    }
    if (data.containsKey('world_id')) {
      context.handle(
        _worldIdMeta,
        worldId.isAcceptableOrUnknown(data['world_id']!, _worldIdMeta),
      );
    } else if (isInserting) {
      context.missing(_worldIdMeta);
    }
    if (data.containsKey('name')) {
      context.handle(
        _nameMeta,
        name.isAcceptableOrUnknown(data['name']!, _nameMeta),
      );
    } else if (isInserting) {
      context.missing(_nameMeta);
    }
    if (data.containsKey('status')) {
      context.handle(
        _statusMeta,
        status.isAcceptableOrUnknown(data['status']!, _statusMeta),
      );
    } else if (isInserting) {
      context.missing(_statusMeta);
    }
    if (data.containsKey('current_game_time_utc')) {
      context.handle(
        _currentGameTimeUtcMeta,
        currentGameTimeUtc.isAcceptableOrUnknown(
          data['current_game_time_utc']!,
          _currentGameTimeUtcMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_currentGameTimeUtcMeta);
    }
    if (data.containsKey('player_actor_id')) {
      context.handle(
        _playerActorIdMeta,
        playerActorId.isAcceptableOrUnknown(
          data['player_actor_id']!,
          _playerActorIdMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_playerActorIdMeta);
    }
    if (data.containsKey('player_display_name')) {
      context.handle(
        _playerDisplayNameMeta,
        playerDisplayName.isAcceptableOrUnknown(
          data['player_display_name']!,
          _playerDisplayNameMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_playerDisplayNameMeta);
    }
    if (data.containsKey('created_at_utc')) {
      context.handle(
        _createdAtUtcMeta,
        createdAtUtc.isAcceptableOrUnknown(
          data['created_at_utc']!,
          _createdAtUtcMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_createdAtUtcMeta);
    }
    if (data.containsKey('cached_at_utc')) {
      context.handle(
        _cachedAtUtcMeta,
        cachedAtUtc.isAcceptableOrUnknown(
          data['cached_at_utc']!,
          _cachedAtUtcMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_cachedAtUtcMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {userId};
  @override
  CachedWorld map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return CachedWorld(
      userId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}user_id'],
      )!,
      worldId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}world_id'],
      )!,
      name: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name'],
      )!,
      status: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}status'],
      )!,
      currentGameTimeUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}current_game_time_utc'],
      )!,
      playerActorId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}player_actor_id'],
      )!,
      playerDisplayName: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}player_display_name'],
      )!,
      createdAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}created_at_utc'],
      )!,
      cachedAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}cached_at_utc'],
      )!,
    );
  }

  @override
  $CachedWorldsTable createAlias(String alias) {
    return $CachedWorldsTable(attachedDatabase, alias);
  }
}

class CachedWorld extends DataClass implements Insertable<CachedWorld> {
  final String userId;
  final String worldId;
  final String name;
  final String status;
  final DateTime currentGameTimeUtc;
  final String playerActorId;
  final String playerDisplayName;
  final DateTime createdAtUtc;
  final DateTime cachedAtUtc;
  const CachedWorld({
    required this.userId,
    required this.worldId,
    required this.name,
    required this.status,
    required this.currentGameTimeUtc,
    required this.playerActorId,
    required this.playerDisplayName,
    required this.createdAtUtc,
    required this.cachedAtUtc,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['user_id'] = Variable<String>(userId);
    map['world_id'] = Variable<String>(worldId);
    map['name'] = Variable<String>(name);
    map['status'] = Variable<String>(status);
    map['current_game_time_utc'] = Variable<DateTime>(currentGameTimeUtc);
    map['player_actor_id'] = Variable<String>(playerActorId);
    map['player_display_name'] = Variable<String>(playerDisplayName);
    map['created_at_utc'] = Variable<DateTime>(createdAtUtc);
    map['cached_at_utc'] = Variable<DateTime>(cachedAtUtc);
    return map;
  }

  CachedWorldsCompanion toCompanion(bool nullToAbsent) {
    return CachedWorldsCompanion(
      userId: Value(userId),
      worldId: Value(worldId),
      name: Value(name),
      status: Value(status),
      currentGameTimeUtc: Value(currentGameTimeUtc),
      playerActorId: Value(playerActorId),
      playerDisplayName: Value(playerDisplayName),
      createdAtUtc: Value(createdAtUtc),
      cachedAtUtc: Value(cachedAtUtc),
    );
  }

  factory CachedWorld.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return CachedWorld(
      userId: serializer.fromJson<String>(json['userId']),
      worldId: serializer.fromJson<String>(json['worldId']),
      name: serializer.fromJson<String>(json['name']),
      status: serializer.fromJson<String>(json['status']),
      currentGameTimeUtc: serializer.fromJson<DateTime>(
        json['currentGameTimeUtc'],
      ),
      playerActorId: serializer.fromJson<String>(json['playerActorId']),
      playerDisplayName: serializer.fromJson<String>(json['playerDisplayName']),
      createdAtUtc: serializer.fromJson<DateTime>(json['createdAtUtc']),
      cachedAtUtc: serializer.fromJson<DateTime>(json['cachedAtUtc']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'userId': serializer.toJson<String>(userId),
      'worldId': serializer.toJson<String>(worldId),
      'name': serializer.toJson<String>(name),
      'status': serializer.toJson<String>(status),
      'currentGameTimeUtc': serializer.toJson<DateTime>(currentGameTimeUtc),
      'playerActorId': serializer.toJson<String>(playerActorId),
      'playerDisplayName': serializer.toJson<String>(playerDisplayName),
      'createdAtUtc': serializer.toJson<DateTime>(createdAtUtc),
      'cachedAtUtc': serializer.toJson<DateTime>(cachedAtUtc),
    };
  }

  CachedWorld copyWith({
    String? userId,
    String? worldId,
    String? name,
    String? status,
    DateTime? currentGameTimeUtc,
    String? playerActorId,
    String? playerDisplayName,
    DateTime? createdAtUtc,
    DateTime? cachedAtUtc,
  }) => CachedWorld(
    userId: userId ?? this.userId,
    worldId: worldId ?? this.worldId,
    name: name ?? this.name,
    status: status ?? this.status,
    currentGameTimeUtc: currentGameTimeUtc ?? this.currentGameTimeUtc,
    playerActorId: playerActorId ?? this.playerActorId,
    playerDisplayName: playerDisplayName ?? this.playerDisplayName,
    createdAtUtc: createdAtUtc ?? this.createdAtUtc,
    cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
  );
  CachedWorld copyWithCompanion(CachedWorldsCompanion data) {
    return CachedWorld(
      userId: data.userId.present ? data.userId.value : this.userId,
      worldId: data.worldId.present ? data.worldId.value : this.worldId,
      name: data.name.present ? data.name.value : this.name,
      status: data.status.present ? data.status.value : this.status,
      currentGameTimeUtc: data.currentGameTimeUtc.present
          ? data.currentGameTimeUtc.value
          : this.currentGameTimeUtc,
      playerActorId: data.playerActorId.present
          ? data.playerActorId.value
          : this.playerActorId,
      playerDisplayName: data.playerDisplayName.present
          ? data.playerDisplayName.value
          : this.playerDisplayName,
      createdAtUtc: data.createdAtUtc.present
          ? data.createdAtUtc.value
          : this.createdAtUtc,
      cachedAtUtc: data.cachedAtUtc.present
          ? data.cachedAtUtc.value
          : this.cachedAtUtc,
    );
  }

  @override
  String toString() {
    return (StringBuffer('CachedWorld(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('name: $name, ')
          ..write('status: $status, ')
          ..write('currentGameTimeUtc: $currentGameTimeUtc, ')
          ..write('playerActorId: $playerActorId, ')
          ..write('playerDisplayName: $playerDisplayName, ')
          ..write('createdAtUtc: $createdAtUtc, ')
          ..write('cachedAtUtc: $cachedAtUtc')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(
    userId,
    worldId,
    name,
    status,
    currentGameTimeUtc,
    playerActorId,
    playerDisplayName,
    createdAtUtc,
    cachedAtUtc,
  );
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is CachedWorld &&
          other.userId == this.userId &&
          other.worldId == this.worldId &&
          other.name == this.name &&
          other.status == this.status &&
          other.currentGameTimeUtc == this.currentGameTimeUtc &&
          other.playerActorId == this.playerActorId &&
          other.playerDisplayName == this.playerDisplayName &&
          other.createdAtUtc == this.createdAtUtc &&
          other.cachedAtUtc == this.cachedAtUtc);
}

class CachedWorldsCompanion extends UpdateCompanion<CachedWorld> {
  final Value<String> userId;
  final Value<String> worldId;
  final Value<String> name;
  final Value<String> status;
  final Value<DateTime> currentGameTimeUtc;
  final Value<String> playerActorId;
  final Value<String> playerDisplayName;
  final Value<DateTime> createdAtUtc;
  final Value<DateTime> cachedAtUtc;
  final Value<int> rowid;
  const CachedWorldsCompanion({
    this.userId = const Value.absent(),
    this.worldId = const Value.absent(),
    this.name = const Value.absent(),
    this.status = const Value.absent(),
    this.currentGameTimeUtc = const Value.absent(),
    this.playerActorId = const Value.absent(),
    this.playerDisplayName = const Value.absent(),
    this.createdAtUtc = const Value.absent(),
    this.cachedAtUtc = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  CachedWorldsCompanion.insert({
    required String userId,
    required String worldId,
    required String name,
    required String status,
    required DateTime currentGameTimeUtc,
    required String playerActorId,
    required String playerDisplayName,
    required DateTime createdAtUtc,
    required DateTime cachedAtUtc,
    this.rowid = const Value.absent(),
  }) : userId = Value(userId),
       worldId = Value(worldId),
       name = Value(name),
       status = Value(status),
       currentGameTimeUtc = Value(currentGameTimeUtc),
       playerActorId = Value(playerActorId),
       playerDisplayName = Value(playerDisplayName),
       createdAtUtc = Value(createdAtUtc),
       cachedAtUtc = Value(cachedAtUtc);
  static Insertable<CachedWorld> custom({
    Expression<String>? userId,
    Expression<String>? worldId,
    Expression<String>? name,
    Expression<String>? status,
    Expression<DateTime>? currentGameTimeUtc,
    Expression<String>? playerActorId,
    Expression<String>? playerDisplayName,
    Expression<DateTime>? createdAtUtc,
    Expression<DateTime>? cachedAtUtc,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (userId != null) 'user_id': userId,
      if (worldId != null) 'world_id': worldId,
      if (name != null) 'name': name,
      if (status != null) 'status': status,
      if (currentGameTimeUtc != null)
        'current_game_time_utc': currentGameTimeUtc,
      if (playerActorId != null) 'player_actor_id': playerActorId,
      if (playerDisplayName != null) 'player_display_name': playerDisplayName,
      if (createdAtUtc != null) 'created_at_utc': createdAtUtc,
      if (cachedAtUtc != null) 'cached_at_utc': cachedAtUtc,
      if (rowid != null) 'rowid': rowid,
    });
  }

  CachedWorldsCompanion copyWith({
    Value<String>? userId,
    Value<String>? worldId,
    Value<String>? name,
    Value<String>? status,
    Value<DateTime>? currentGameTimeUtc,
    Value<String>? playerActorId,
    Value<String>? playerDisplayName,
    Value<DateTime>? createdAtUtc,
    Value<DateTime>? cachedAtUtc,
    Value<int>? rowid,
  }) {
    return CachedWorldsCompanion(
      userId: userId ?? this.userId,
      worldId: worldId ?? this.worldId,
      name: name ?? this.name,
      status: status ?? this.status,
      currentGameTimeUtc: currentGameTimeUtc ?? this.currentGameTimeUtc,
      playerActorId: playerActorId ?? this.playerActorId,
      playerDisplayName: playerDisplayName ?? this.playerDisplayName,
      createdAtUtc: createdAtUtc ?? this.createdAtUtc,
      cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (userId.present) {
      map['user_id'] = Variable<String>(userId.value);
    }
    if (worldId.present) {
      map['world_id'] = Variable<String>(worldId.value);
    }
    if (name.present) {
      map['name'] = Variable<String>(name.value);
    }
    if (status.present) {
      map['status'] = Variable<String>(status.value);
    }
    if (currentGameTimeUtc.present) {
      map['current_game_time_utc'] = Variable<DateTime>(
        currentGameTimeUtc.value,
      );
    }
    if (playerActorId.present) {
      map['player_actor_id'] = Variable<String>(playerActorId.value);
    }
    if (playerDisplayName.present) {
      map['player_display_name'] = Variable<String>(playerDisplayName.value);
    }
    if (createdAtUtc.present) {
      map['created_at_utc'] = Variable<DateTime>(createdAtUtc.value);
    }
    if (cachedAtUtc.present) {
      map['cached_at_utc'] = Variable<DateTime>(cachedAtUtc.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('CachedWorldsCompanion(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('name: $name, ')
          ..write('status: $status, ')
          ..write('currentGameTimeUtc: $currentGameTimeUtc, ')
          ..write('playerActorId: $playerActorId, ')
          ..write('playerDisplayName: $playerDisplayName, ')
          ..write('createdAtUtc: $createdAtUtc, ')
          ..write('cachedAtUtc: $cachedAtUtc, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $CachedCharacterCataloguesTable extends CachedCharacterCatalogues
    with TableInfo<$CachedCharacterCataloguesTable, CachedCharacterCatalogue> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $CachedCharacterCataloguesTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _userIdMeta = const VerificationMeta('userId');
  @override
  late final GeneratedColumn<String> userId = GeneratedColumn<String>(
    'user_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _worldIdMeta = const VerificationMeta(
    'worldId',
  );
  @override
  late final GeneratedColumn<String> worldId = GeneratedColumn<String>(
    'world_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _cachedAtUtcMeta = const VerificationMeta(
    'cachedAtUtc',
  );
  @override
  late final GeneratedColumn<DateTime> cachedAtUtc = GeneratedColumn<DateTime>(
    'cached_at_utc',
    aliasedName,
    false,
    type: DriftSqlType.dateTime,
    requiredDuringInsert: true,
  );
  @override
  List<GeneratedColumn> get $columns => [userId, worldId, cachedAtUtc];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'cached_character_catalogues';
  @override
  VerificationContext validateIntegrity(
    Insertable<CachedCharacterCatalogue> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('user_id')) {
      context.handle(
        _userIdMeta,
        userId.isAcceptableOrUnknown(data['user_id']!, _userIdMeta),
      );
    } else if (isInserting) {
      context.missing(_userIdMeta);
    }
    if (data.containsKey('world_id')) {
      context.handle(
        _worldIdMeta,
        worldId.isAcceptableOrUnknown(data['world_id']!, _worldIdMeta),
      );
    } else if (isInserting) {
      context.missing(_worldIdMeta);
    }
    if (data.containsKey('cached_at_utc')) {
      context.handle(
        _cachedAtUtcMeta,
        cachedAtUtc.isAcceptableOrUnknown(
          data['cached_at_utc']!,
          _cachedAtUtcMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_cachedAtUtcMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {userId, worldId};
  @override
  CachedCharacterCatalogue map(
    Map<String, dynamic> data, {
    String? tablePrefix,
  }) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return CachedCharacterCatalogue(
      userId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}user_id'],
      )!,
      worldId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}world_id'],
      )!,
      cachedAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}cached_at_utc'],
      )!,
    );
  }

  @override
  $CachedCharacterCataloguesTable createAlias(String alias) {
    return $CachedCharacterCataloguesTable(attachedDatabase, alias);
  }
}

class CachedCharacterCatalogue extends DataClass
    implements Insertable<CachedCharacterCatalogue> {
  final String userId;
  final String worldId;
  final DateTime cachedAtUtc;
  const CachedCharacterCatalogue({
    required this.userId,
    required this.worldId,
    required this.cachedAtUtc,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['user_id'] = Variable<String>(userId);
    map['world_id'] = Variable<String>(worldId);
    map['cached_at_utc'] = Variable<DateTime>(cachedAtUtc);
    return map;
  }

  CachedCharacterCataloguesCompanion toCompanion(bool nullToAbsent) {
    return CachedCharacterCataloguesCompanion(
      userId: Value(userId),
      worldId: Value(worldId),
      cachedAtUtc: Value(cachedAtUtc),
    );
  }

  factory CachedCharacterCatalogue.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return CachedCharacterCatalogue(
      userId: serializer.fromJson<String>(json['userId']),
      worldId: serializer.fromJson<String>(json['worldId']),
      cachedAtUtc: serializer.fromJson<DateTime>(json['cachedAtUtc']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'userId': serializer.toJson<String>(userId),
      'worldId': serializer.toJson<String>(worldId),
      'cachedAtUtc': serializer.toJson<DateTime>(cachedAtUtc),
    };
  }

  CachedCharacterCatalogue copyWith({
    String? userId,
    String? worldId,
    DateTime? cachedAtUtc,
  }) => CachedCharacterCatalogue(
    userId: userId ?? this.userId,
    worldId: worldId ?? this.worldId,
    cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
  );
  CachedCharacterCatalogue copyWithCompanion(
    CachedCharacterCataloguesCompanion data,
  ) {
    return CachedCharacterCatalogue(
      userId: data.userId.present ? data.userId.value : this.userId,
      worldId: data.worldId.present ? data.worldId.value : this.worldId,
      cachedAtUtc: data.cachedAtUtc.present
          ? data.cachedAtUtc.value
          : this.cachedAtUtc,
    );
  }

  @override
  String toString() {
    return (StringBuffer('CachedCharacterCatalogue(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('cachedAtUtc: $cachedAtUtc')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(userId, worldId, cachedAtUtc);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is CachedCharacterCatalogue &&
          other.userId == this.userId &&
          other.worldId == this.worldId &&
          other.cachedAtUtc == this.cachedAtUtc);
}

class CachedCharacterCataloguesCompanion
    extends UpdateCompanion<CachedCharacterCatalogue> {
  final Value<String> userId;
  final Value<String> worldId;
  final Value<DateTime> cachedAtUtc;
  final Value<int> rowid;
  const CachedCharacterCataloguesCompanion({
    this.userId = const Value.absent(),
    this.worldId = const Value.absent(),
    this.cachedAtUtc = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  CachedCharacterCataloguesCompanion.insert({
    required String userId,
    required String worldId,
    required DateTime cachedAtUtc,
    this.rowid = const Value.absent(),
  }) : userId = Value(userId),
       worldId = Value(worldId),
       cachedAtUtc = Value(cachedAtUtc);
  static Insertable<CachedCharacterCatalogue> custom({
    Expression<String>? userId,
    Expression<String>? worldId,
    Expression<DateTime>? cachedAtUtc,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (userId != null) 'user_id': userId,
      if (worldId != null) 'world_id': worldId,
      if (cachedAtUtc != null) 'cached_at_utc': cachedAtUtc,
      if (rowid != null) 'rowid': rowid,
    });
  }

  CachedCharacterCataloguesCompanion copyWith({
    Value<String>? userId,
    Value<String>? worldId,
    Value<DateTime>? cachedAtUtc,
    Value<int>? rowid,
  }) {
    return CachedCharacterCataloguesCompanion(
      userId: userId ?? this.userId,
      worldId: worldId ?? this.worldId,
      cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (userId.present) {
      map['user_id'] = Variable<String>(userId.value);
    }
    if (worldId.present) {
      map['world_id'] = Variable<String>(worldId.value);
    }
    if (cachedAtUtc.present) {
      map['cached_at_utc'] = Variable<DateTime>(cachedAtUtc.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('CachedCharacterCataloguesCompanion(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('cachedAtUtc: $cachedAtUtc, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $CachedCharacterSummariesTable extends CachedCharacterSummaries
    with TableInfo<$CachedCharacterSummariesTable, CachedCharacterSummary> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $CachedCharacterSummariesTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _userIdMeta = const VerificationMeta('userId');
  @override
  late final GeneratedColumn<String> userId = GeneratedColumn<String>(
    'user_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _worldIdMeta = const VerificationMeta(
    'worldId',
  );
  @override
  late final GeneratedColumn<String> worldId = GeneratedColumn<String>(
    'world_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _characterIdMeta = const VerificationMeta(
    'characterId',
  );
  @override
  late final GeneratedColumn<String> characterId = GeneratedColumn<String>(
    'character_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _displayNameMeta = const VerificationMeta(
    'displayName',
  );
  @override
  late final GeneratedColumn<String> displayName = GeneratedColumn<String>(
    'display_name',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 60,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _handleMeta = const VerificationMeta('handle');
  @override
  late final GeneratedColumn<String> handle = GeneratedColumn<String>(
    'handle',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 30,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _professionMeta = const VerificationMeta(
    'profession',
  );
  @override
  late final GeneratedColumn<String> profession = GeneratedColumn<String>(
    'profession',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 80,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _visibleMoodMeta = const VerificationMeta(
    'visibleMood',
  );
  @override
  late final GeneratedColumn<String> visibleMood = GeneratedColumn<String>(
    'visible_mood',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _isFollowedMeta = const VerificationMeta(
    'isFollowed',
  );
  @override
  late final GeneratedColumn<bool> isFollowed = GeneratedColumn<bool>(
    'is_followed',
    aliasedName,
    false,
    type: DriftSqlType.bool,
    requiredDuringInsert: true,
    defaultConstraints: GeneratedColumn.constraintIsAlways(
      'CHECK ("is_followed" IN (0, 1))',
    ),
  );
  @override
  List<GeneratedColumn> get $columns => [
    userId,
    worldId,
    characterId,
    displayName,
    handle,
    profession,
    visibleMood,
    isFollowed,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'cached_character_summaries';
  @override
  VerificationContext validateIntegrity(
    Insertable<CachedCharacterSummary> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('user_id')) {
      context.handle(
        _userIdMeta,
        userId.isAcceptableOrUnknown(data['user_id']!, _userIdMeta),
      );
    } else if (isInserting) {
      context.missing(_userIdMeta);
    }
    if (data.containsKey('world_id')) {
      context.handle(
        _worldIdMeta,
        worldId.isAcceptableOrUnknown(data['world_id']!, _worldIdMeta),
      );
    } else if (isInserting) {
      context.missing(_worldIdMeta);
    }
    if (data.containsKey('character_id')) {
      context.handle(
        _characterIdMeta,
        characterId.isAcceptableOrUnknown(
          data['character_id']!,
          _characterIdMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_characterIdMeta);
    }
    if (data.containsKey('display_name')) {
      context.handle(
        _displayNameMeta,
        displayName.isAcceptableOrUnknown(
          data['display_name']!,
          _displayNameMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_displayNameMeta);
    }
    if (data.containsKey('handle')) {
      context.handle(
        _handleMeta,
        handle.isAcceptableOrUnknown(data['handle']!, _handleMeta),
      );
    } else if (isInserting) {
      context.missing(_handleMeta);
    }
    if (data.containsKey('profession')) {
      context.handle(
        _professionMeta,
        profession.isAcceptableOrUnknown(data['profession']!, _professionMeta),
      );
    } else if (isInserting) {
      context.missing(_professionMeta);
    }
    if (data.containsKey('visible_mood')) {
      context.handle(
        _visibleMoodMeta,
        visibleMood.isAcceptableOrUnknown(
          data['visible_mood']!,
          _visibleMoodMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_visibleMoodMeta);
    }
    if (data.containsKey('is_followed')) {
      context.handle(
        _isFollowedMeta,
        isFollowed.isAcceptableOrUnknown(data['is_followed']!, _isFollowedMeta),
      );
    } else if (isInserting) {
      context.missing(_isFollowedMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {userId, worldId, characterId};
  @override
  CachedCharacterSummary map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return CachedCharacterSummary(
      userId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}user_id'],
      )!,
      worldId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}world_id'],
      )!,
      characterId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}character_id'],
      )!,
      displayName: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}display_name'],
      )!,
      handle: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}handle'],
      )!,
      profession: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}profession'],
      )!,
      visibleMood: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}visible_mood'],
      )!,
      isFollowed: attachedDatabase.typeMapping.read(
        DriftSqlType.bool,
        data['${effectivePrefix}is_followed'],
      )!,
    );
  }

  @override
  $CachedCharacterSummariesTable createAlias(String alias) {
    return $CachedCharacterSummariesTable(attachedDatabase, alias);
  }
}

class CachedCharacterSummary extends DataClass
    implements Insertable<CachedCharacterSummary> {
  final String userId;
  final String worldId;
  final String characterId;
  final String displayName;
  final String handle;
  final String profession;
  final String visibleMood;
  final bool isFollowed;
  const CachedCharacterSummary({
    required this.userId,
    required this.worldId,
    required this.characterId,
    required this.displayName,
    required this.handle,
    required this.profession,
    required this.visibleMood,
    required this.isFollowed,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['user_id'] = Variable<String>(userId);
    map['world_id'] = Variable<String>(worldId);
    map['character_id'] = Variable<String>(characterId);
    map['display_name'] = Variable<String>(displayName);
    map['handle'] = Variable<String>(handle);
    map['profession'] = Variable<String>(profession);
    map['visible_mood'] = Variable<String>(visibleMood);
    map['is_followed'] = Variable<bool>(isFollowed);
    return map;
  }

  CachedCharacterSummariesCompanion toCompanion(bool nullToAbsent) {
    return CachedCharacterSummariesCompanion(
      userId: Value(userId),
      worldId: Value(worldId),
      characterId: Value(characterId),
      displayName: Value(displayName),
      handle: Value(handle),
      profession: Value(profession),
      visibleMood: Value(visibleMood),
      isFollowed: Value(isFollowed),
    );
  }

  factory CachedCharacterSummary.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return CachedCharacterSummary(
      userId: serializer.fromJson<String>(json['userId']),
      worldId: serializer.fromJson<String>(json['worldId']),
      characterId: serializer.fromJson<String>(json['characterId']),
      displayName: serializer.fromJson<String>(json['displayName']),
      handle: serializer.fromJson<String>(json['handle']),
      profession: serializer.fromJson<String>(json['profession']),
      visibleMood: serializer.fromJson<String>(json['visibleMood']),
      isFollowed: serializer.fromJson<bool>(json['isFollowed']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'userId': serializer.toJson<String>(userId),
      'worldId': serializer.toJson<String>(worldId),
      'characterId': serializer.toJson<String>(characterId),
      'displayName': serializer.toJson<String>(displayName),
      'handle': serializer.toJson<String>(handle),
      'profession': serializer.toJson<String>(profession),
      'visibleMood': serializer.toJson<String>(visibleMood),
      'isFollowed': serializer.toJson<bool>(isFollowed),
    };
  }

  CachedCharacterSummary copyWith({
    String? userId,
    String? worldId,
    String? characterId,
    String? displayName,
    String? handle,
    String? profession,
    String? visibleMood,
    bool? isFollowed,
  }) => CachedCharacterSummary(
    userId: userId ?? this.userId,
    worldId: worldId ?? this.worldId,
    characterId: characterId ?? this.characterId,
    displayName: displayName ?? this.displayName,
    handle: handle ?? this.handle,
    profession: profession ?? this.profession,
    visibleMood: visibleMood ?? this.visibleMood,
    isFollowed: isFollowed ?? this.isFollowed,
  );
  CachedCharacterSummary copyWithCompanion(
    CachedCharacterSummariesCompanion data,
  ) {
    return CachedCharacterSummary(
      userId: data.userId.present ? data.userId.value : this.userId,
      worldId: data.worldId.present ? data.worldId.value : this.worldId,
      characterId: data.characterId.present
          ? data.characterId.value
          : this.characterId,
      displayName: data.displayName.present
          ? data.displayName.value
          : this.displayName,
      handle: data.handle.present ? data.handle.value : this.handle,
      profession: data.profession.present
          ? data.profession.value
          : this.profession,
      visibleMood: data.visibleMood.present
          ? data.visibleMood.value
          : this.visibleMood,
      isFollowed: data.isFollowed.present
          ? data.isFollowed.value
          : this.isFollowed,
    );
  }

  @override
  String toString() {
    return (StringBuffer('CachedCharacterSummary(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('characterId: $characterId, ')
          ..write('displayName: $displayName, ')
          ..write('handle: $handle, ')
          ..write('profession: $profession, ')
          ..write('visibleMood: $visibleMood, ')
          ..write('isFollowed: $isFollowed')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(
    userId,
    worldId,
    characterId,
    displayName,
    handle,
    profession,
    visibleMood,
    isFollowed,
  );
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is CachedCharacterSummary &&
          other.userId == this.userId &&
          other.worldId == this.worldId &&
          other.characterId == this.characterId &&
          other.displayName == this.displayName &&
          other.handle == this.handle &&
          other.profession == this.profession &&
          other.visibleMood == this.visibleMood &&
          other.isFollowed == this.isFollowed);
}

class CachedCharacterSummariesCompanion
    extends UpdateCompanion<CachedCharacterSummary> {
  final Value<String> userId;
  final Value<String> worldId;
  final Value<String> characterId;
  final Value<String> displayName;
  final Value<String> handle;
  final Value<String> profession;
  final Value<String> visibleMood;
  final Value<bool> isFollowed;
  final Value<int> rowid;
  const CachedCharacterSummariesCompanion({
    this.userId = const Value.absent(),
    this.worldId = const Value.absent(),
    this.characterId = const Value.absent(),
    this.displayName = const Value.absent(),
    this.handle = const Value.absent(),
    this.profession = const Value.absent(),
    this.visibleMood = const Value.absent(),
    this.isFollowed = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  CachedCharacterSummariesCompanion.insert({
    required String userId,
    required String worldId,
    required String characterId,
    required String displayName,
    required String handle,
    required String profession,
    required String visibleMood,
    required bool isFollowed,
    this.rowid = const Value.absent(),
  }) : userId = Value(userId),
       worldId = Value(worldId),
       characterId = Value(characterId),
       displayName = Value(displayName),
       handle = Value(handle),
       profession = Value(profession),
       visibleMood = Value(visibleMood),
       isFollowed = Value(isFollowed);
  static Insertable<CachedCharacterSummary> custom({
    Expression<String>? userId,
    Expression<String>? worldId,
    Expression<String>? characterId,
    Expression<String>? displayName,
    Expression<String>? handle,
    Expression<String>? profession,
    Expression<String>? visibleMood,
    Expression<bool>? isFollowed,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (userId != null) 'user_id': userId,
      if (worldId != null) 'world_id': worldId,
      if (characterId != null) 'character_id': characterId,
      if (displayName != null) 'display_name': displayName,
      if (handle != null) 'handle': handle,
      if (profession != null) 'profession': profession,
      if (visibleMood != null) 'visible_mood': visibleMood,
      if (isFollowed != null) 'is_followed': isFollowed,
      if (rowid != null) 'rowid': rowid,
    });
  }

  CachedCharacterSummariesCompanion copyWith({
    Value<String>? userId,
    Value<String>? worldId,
    Value<String>? characterId,
    Value<String>? displayName,
    Value<String>? handle,
    Value<String>? profession,
    Value<String>? visibleMood,
    Value<bool>? isFollowed,
    Value<int>? rowid,
  }) {
    return CachedCharacterSummariesCompanion(
      userId: userId ?? this.userId,
      worldId: worldId ?? this.worldId,
      characterId: characterId ?? this.characterId,
      displayName: displayName ?? this.displayName,
      handle: handle ?? this.handle,
      profession: profession ?? this.profession,
      visibleMood: visibleMood ?? this.visibleMood,
      isFollowed: isFollowed ?? this.isFollowed,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (userId.present) {
      map['user_id'] = Variable<String>(userId.value);
    }
    if (worldId.present) {
      map['world_id'] = Variable<String>(worldId.value);
    }
    if (characterId.present) {
      map['character_id'] = Variable<String>(characterId.value);
    }
    if (displayName.present) {
      map['display_name'] = Variable<String>(displayName.value);
    }
    if (handle.present) {
      map['handle'] = Variable<String>(handle.value);
    }
    if (profession.present) {
      map['profession'] = Variable<String>(profession.value);
    }
    if (visibleMood.present) {
      map['visible_mood'] = Variable<String>(visibleMood.value);
    }
    if (isFollowed.present) {
      map['is_followed'] = Variable<bool>(isFollowed.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('CachedCharacterSummariesCompanion(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('characterId: $characterId, ')
          ..write('displayName: $displayName, ')
          ..write('handle: $handle, ')
          ..write('profession: $profession, ')
          ..write('visibleMood: $visibleMood, ')
          ..write('isFollowed: $isFollowed, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $CachedCharacterDetailsTable extends CachedCharacterDetails
    with TableInfo<$CachedCharacterDetailsTable, CachedCharacterDetail> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $CachedCharacterDetailsTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _userIdMeta = const VerificationMeta('userId');
  @override
  late final GeneratedColumn<String> userId = GeneratedColumn<String>(
    'user_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _worldIdMeta = const VerificationMeta(
    'worldId',
  );
  @override
  late final GeneratedColumn<String> worldId = GeneratedColumn<String>(
    'world_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _characterIdMeta = const VerificationMeta(
    'characterId',
  );
  @override
  late final GeneratedColumn<String> characterId = GeneratedColumn<String>(
    'character_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _displayNameMeta = const VerificationMeta(
    'displayName',
  );
  @override
  late final GeneratedColumn<String> displayName = GeneratedColumn<String>(
    'display_name',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 60,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _handleMeta = const VerificationMeta('handle');
  @override
  late final GeneratedColumn<String> handle = GeneratedColumn<String>(
    'handle',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 30,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _bioMeta = const VerificationMeta('bio');
  @override
  late final GeneratedColumn<String> bio = GeneratedColumn<String>(
    'bio',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(maxTextLength: 300),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _ageMeta = const VerificationMeta('age');
  @override
  late final GeneratedColumn<int> age = GeneratedColumn<int>(
    'age',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _professionMeta = const VerificationMeta(
    'profession',
  );
  @override
  late final GeneratedColumn<String> profession = GeneratedColumn<String>(
    'profession',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 80,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _archetypeMeta = const VerificationMeta(
    'archetype',
  );
  @override
  late final GeneratedColumn<String> archetype = GeneratedColumn<String>(
    'archetype',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 80,
    ),
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _visibleMoodMeta = const VerificationMeta(
    'visibleMood',
  );
  @override
  late final GeneratedColumn<String> visibleMood = GeneratedColumn<String>(
    'visible_mood',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _interestsJsonMeta = const VerificationMeta(
    'interestsJson',
  );
  @override
  late final GeneratedColumn<String> interestsJson = GeneratedColumn<String>(
    'interests_json',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _scheduleJsonMeta = const VerificationMeta(
    'scheduleJson',
  );
  @override
  late final GeneratedColumn<String> scheduleJson = GeneratedColumn<String>(
    'schedule_json',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _cachedAtUtcMeta = const VerificationMeta(
    'cachedAtUtc',
  );
  @override
  late final GeneratedColumn<DateTime> cachedAtUtc = GeneratedColumn<DateTime>(
    'cached_at_utc',
    aliasedName,
    false,
    type: DriftSqlType.dateTime,
    requiredDuringInsert: true,
  );
  @override
  List<GeneratedColumn> get $columns => [
    userId,
    worldId,
    characterId,
    displayName,
    handle,
    bio,
    age,
    profession,
    archetype,
    visibleMood,
    interestsJson,
    scheduleJson,
    cachedAtUtc,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'cached_character_details';
  @override
  VerificationContext validateIntegrity(
    Insertable<CachedCharacterDetail> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('user_id')) {
      context.handle(
        _userIdMeta,
        userId.isAcceptableOrUnknown(data['user_id']!, _userIdMeta),
      );
    } else if (isInserting) {
      context.missing(_userIdMeta);
    }
    if (data.containsKey('world_id')) {
      context.handle(
        _worldIdMeta,
        worldId.isAcceptableOrUnknown(data['world_id']!, _worldIdMeta),
      );
    } else if (isInserting) {
      context.missing(_worldIdMeta);
    }
    if (data.containsKey('character_id')) {
      context.handle(
        _characterIdMeta,
        characterId.isAcceptableOrUnknown(
          data['character_id']!,
          _characterIdMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_characterIdMeta);
    }
    if (data.containsKey('display_name')) {
      context.handle(
        _displayNameMeta,
        displayName.isAcceptableOrUnknown(
          data['display_name']!,
          _displayNameMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_displayNameMeta);
    }
    if (data.containsKey('handle')) {
      context.handle(
        _handleMeta,
        handle.isAcceptableOrUnknown(data['handle']!, _handleMeta),
      );
    } else if (isInserting) {
      context.missing(_handleMeta);
    }
    if (data.containsKey('bio')) {
      context.handle(
        _bioMeta,
        bio.isAcceptableOrUnknown(data['bio']!, _bioMeta),
      );
    } else if (isInserting) {
      context.missing(_bioMeta);
    }
    if (data.containsKey('age')) {
      context.handle(
        _ageMeta,
        age.isAcceptableOrUnknown(data['age']!, _ageMeta),
      );
    } else if (isInserting) {
      context.missing(_ageMeta);
    }
    if (data.containsKey('profession')) {
      context.handle(
        _professionMeta,
        profession.isAcceptableOrUnknown(data['profession']!, _professionMeta),
      );
    } else if (isInserting) {
      context.missing(_professionMeta);
    }
    if (data.containsKey('archetype')) {
      context.handle(
        _archetypeMeta,
        archetype.isAcceptableOrUnknown(data['archetype']!, _archetypeMeta),
      );
    } else if (isInserting) {
      context.missing(_archetypeMeta);
    }
    if (data.containsKey('visible_mood')) {
      context.handle(
        _visibleMoodMeta,
        visibleMood.isAcceptableOrUnknown(
          data['visible_mood']!,
          _visibleMoodMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_visibleMoodMeta);
    }
    if (data.containsKey('interests_json')) {
      context.handle(
        _interestsJsonMeta,
        interestsJson.isAcceptableOrUnknown(
          data['interests_json']!,
          _interestsJsonMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_interestsJsonMeta);
    }
    if (data.containsKey('schedule_json')) {
      context.handle(
        _scheduleJsonMeta,
        scheduleJson.isAcceptableOrUnknown(
          data['schedule_json']!,
          _scheduleJsonMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_scheduleJsonMeta);
    }
    if (data.containsKey('cached_at_utc')) {
      context.handle(
        _cachedAtUtcMeta,
        cachedAtUtc.isAcceptableOrUnknown(
          data['cached_at_utc']!,
          _cachedAtUtcMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_cachedAtUtcMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {userId, worldId, characterId};
  @override
  CachedCharacterDetail map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return CachedCharacterDetail(
      userId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}user_id'],
      )!,
      worldId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}world_id'],
      )!,
      characterId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}character_id'],
      )!,
      displayName: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}display_name'],
      )!,
      handle: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}handle'],
      )!,
      bio: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}bio'],
      )!,
      age: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}age'],
      )!,
      profession: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}profession'],
      )!,
      archetype: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}archetype'],
      )!,
      visibleMood: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}visible_mood'],
      )!,
      interestsJson: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}interests_json'],
      )!,
      scheduleJson: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}schedule_json'],
      )!,
      cachedAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}cached_at_utc'],
      )!,
    );
  }

  @override
  $CachedCharacterDetailsTable createAlias(String alias) {
    return $CachedCharacterDetailsTable(attachedDatabase, alias);
  }
}

class CachedCharacterDetail extends DataClass
    implements Insertable<CachedCharacterDetail> {
  final String userId;
  final String worldId;
  final String characterId;
  final String displayName;
  final String handle;
  final String bio;
  final int age;
  final String profession;
  final String archetype;
  final String visibleMood;
  final String interestsJson;
  final String scheduleJson;
  final DateTime cachedAtUtc;
  const CachedCharacterDetail({
    required this.userId,
    required this.worldId,
    required this.characterId,
    required this.displayName,
    required this.handle,
    required this.bio,
    required this.age,
    required this.profession,
    required this.archetype,
    required this.visibleMood,
    required this.interestsJson,
    required this.scheduleJson,
    required this.cachedAtUtc,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['user_id'] = Variable<String>(userId);
    map['world_id'] = Variable<String>(worldId);
    map['character_id'] = Variable<String>(characterId);
    map['display_name'] = Variable<String>(displayName);
    map['handle'] = Variable<String>(handle);
    map['bio'] = Variable<String>(bio);
    map['age'] = Variable<int>(age);
    map['profession'] = Variable<String>(profession);
    map['archetype'] = Variable<String>(archetype);
    map['visible_mood'] = Variable<String>(visibleMood);
    map['interests_json'] = Variable<String>(interestsJson);
    map['schedule_json'] = Variable<String>(scheduleJson);
    map['cached_at_utc'] = Variable<DateTime>(cachedAtUtc);
    return map;
  }

  CachedCharacterDetailsCompanion toCompanion(bool nullToAbsent) {
    return CachedCharacterDetailsCompanion(
      userId: Value(userId),
      worldId: Value(worldId),
      characterId: Value(characterId),
      displayName: Value(displayName),
      handle: Value(handle),
      bio: Value(bio),
      age: Value(age),
      profession: Value(profession),
      archetype: Value(archetype),
      visibleMood: Value(visibleMood),
      interestsJson: Value(interestsJson),
      scheduleJson: Value(scheduleJson),
      cachedAtUtc: Value(cachedAtUtc),
    );
  }

  factory CachedCharacterDetail.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return CachedCharacterDetail(
      userId: serializer.fromJson<String>(json['userId']),
      worldId: serializer.fromJson<String>(json['worldId']),
      characterId: serializer.fromJson<String>(json['characterId']),
      displayName: serializer.fromJson<String>(json['displayName']),
      handle: serializer.fromJson<String>(json['handle']),
      bio: serializer.fromJson<String>(json['bio']),
      age: serializer.fromJson<int>(json['age']),
      profession: serializer.fromJson<String>(json['profession']),
      archetype: serializer.fromJson<String>(json['archetype']),
      visibleMood: serializer.fromJson<String>(json['visibleMood']),
      interestsJson: serializer.fromJson<String>(json['interestsJson']),
      scheduleJson: serializer.fromJson<String>(json['scheduleJson']),
      cachedAtUtc: serializer.fromJson<DateTime>(json['cachedAtUtc']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'userId': serializer.toJson<String>(userId),
      'worldId': serializer.toJson<String>(worldId),
      'characterId': serializer.toJson<String>(characterId),
      'displayName': serializer.toJson<String>(displayName),
      'handle': serializer.toJson<String>(handle),
      'bio': serializer.toJson<String>(bio),
      'age': serializer.toJson<int>(age),
      'profession': serializer.toJson<String>(profession),
      'archetype': serializer.toJson<String>(archetype),
      'visibleMood': serializer.toJson<String>(visibleMood),
      'interestsJson': serializer.toJson<String>(interestsJson),
      'scheduleJson': serializer.toJson<String>(scheduleJson),
      'cachedAtUtc': serializer.toJson<DateTime>(cachedAtUtc),
    };
  }

  CachedCharacterDetail copyWith({
    String? userId,
    String? worldId,
    String? characterId,
    String? displayName,
    String? handle,
    String? bio,
    int? age,
    String? profession,
    String? archetype,
    String? visibleMood,
    String? interestsJson,
    String? scheduleJson,
    DateTime? cachedAtUtc,
  }) => CachedCharacterDetail(
    userId: userId ?? this.userId,
    worldId: worldId ?? this.worldId,
    characterId: characterId ?? this.characterId,
    displayName: displayName ?? this.displayName,
    handle: handle ?? this.handle,
    bio: bio ?? this.bio,
    age: age ?? this.age,
    profession: profession ?? this.profession,
    archetype: archetype ?? this.archetype,
    visibleMood: visibleMood ?? this.visibleMood,
    interestsJson: interestsJson ?? this.interestsJson,
    scheduleJson: scheduleJson ?? this.scheduleJson,
    cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
  );
  CachedCharacterDetail copyWithCompanion(
    CachedCharacterDetailsCompanion data,
  ) {
    return CachedCharacterDetail(
      userId: data.userId.present ? data.userId.value : this.userId,
      worldId: data.worldId.present ? data.worldId.value : this.worldId,
      characterId: data.characterId.present
          ? data.characterId.value
          : this.characterId,
      displayName: data.displayName.present
          ? data.displayName.value
          : this.displayName,
      handle: data.handle.present ? data.handle.value : this.handle,
      bio: data.bio.present ? data.bio.value : this.bio,
      age: data.age.present ? data.age.value : this.age,
      profession: data.profession.present
          ? data.profession.value
          : this.profession,
      archetype: data.archetype.present ? data.archetype.value : this.archetype,
      visibleMood: data.visibleMood.present
          ? data.visibleMood.value
          : this.visibleMood,
      interestsJson: data.interestsJson.present
          ? data.interestsJson.value
          : this.interestsJson,
      scheduleJson: data.scheduleJson.present
          ? data.scheduleJson.value
          : this.scheduleJson,
      cachedAtUtc: data.cachedAtUtc.present
          ? data.cachedAtUtc.value
          : this.cachedAtUtc,
    );
  }

  @override
  String toString() {
    return (StringBuffer('CachedCharacterDetail(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('characterId: $characterId, ')
          ..write('displayName: $displayName, ')
          ..write('handle: $handle, ')
          ..write('bio: $bio, ')
          ..write('age: $age, ')
          ..write('profession: $profession, ')
          ..write('archetype: $archetype, ')
          ..write('visibleMood: $visibleMood, ')
          ..write('interestsJson: $interestsJson, ')
          ..write('scheduleJson: $scheduleJson, ')
          ..write('cachedAtUtc: $cachedAtUtc')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(
    userId,
    worldId,
    characterId,
    displayName,
    handle,
    bio,
    age,
    profession,
    archetype,
    visibleMood,
    interestsJson,
    scheduleJson,
    cachedAtUtc,
  );
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is CachedCharacterDetail &&
          other.userId == this.userId &&
          other.worldId == this.worldId &&
          other.characterId == this.characterId &&
          other.displayName == this.displayName &&
          other.handle == this.handle &&
          other.bio == this.bio &&
          other.age == this.age &&
          other.profession == this.profession &&
          other.archetype == this.archetype &&
          other.visibleMood == this.visibleMood &&
          other.interestsJson == this.interestsJson &&
          other.scheduleJson == this.scheduleJson &&
          other.cachedAtUtc == this.cachedAtUtc);
}

class CachedCharacterDetailsCompanion
    extends UpdateCompanion<CachedCharacterDetail> {
  final Value<String> userId;
  final Value<String> worldId;
  final Value<String> characterId;
  final Value<String> displayName;
  final Value<String> handle;
  final Value<String> bio;
  final Value<int> age;
  final Value<String> profession;
  final Value<String> archetype;
  final Value<String> visibleMood;
  final Value<String> interestsJson;
  final Value<String> scheduleJson;
  final Value<DateTime> cachedAtUtc;
  final Value<int> rowid;
  const CachedCharacterDetailsCompanion({
    this.userId = const Value.absent(),
    this.worldId = const Value.absent(),
    this.characterId = const Value.absent(),
    this.displayName = const Value.absent(),
    this.handle = const Value.absent(),
    this.bio = const Value.absent(),
    this.age = const Value.absent(),
    this.profession = const Value.absent(),
    this.archetype = const Value.absent(),
    this.visibleMood = const Value.absent(),
    this.interestsJson = const Value.absent(),
    this.scheduleJson = const Value.absent(),
    this.cachedAtUtc = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  CachedCharacterDetailsCompanion.insert({
    required String userId,
    required String worldId,
    required String characterId,
    required String displayName,
    required String handle,
    required String bio,
    required int age,
    required String profession,
    required String archetype,
    required String visibleMood,
    required String interestsJson,
    required String scheduleJson,
    required DateTime cachedAtUtc,
    this.rowid = const Value.absent(),
  }) : userId = Value(userId),
       worldId = Value(worldId),
       characterId = Value(characterId),
       displayName = Value(displayName),
       handle = Value(handle),
       bio = Value(bio),
       age = Value(age),
       profession = Value(profession),
       archetype = Value(archetype),
       visibleMood = Value(visibleMood),
       interestsJson = Value(interestsJson),
       scheduleJson = Value(scheduleJson),
       cachedAtUtc = Value(cachedAtUtc);
  static Insertable<CachedCharacterDetail> custom({
    Expression<String>? userId,
    Expression<String>? worldId,
    Expression<String>? characterId,
    Expression<String>? displayName,
    Expression<String>? handle,
    Expression<String>? bio,
    Expression<int>? age,
    Expression<String>? profession,
    Expression<String>? archetype,
    Expression<String>? visibleMood,
    Expression<String>? interestsJson,
    Expression<String>? scheduleJson,
    Expression<DateTime>? cachedAtUtc,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (userId != null) 'user_id': userId,
      if (worldId != null) 'world_id': worldId,
      if (characterId != null) 'character_id': characterId,
      if (displayName != null) 'display_name': displayName,
      if (handle != null) 'handle': handle,
      if (bio != null) 'bio': bio,
      if (age != null) 'age': age,
      if (profession != null) 'profession': profession,
      if (archetype != null) 'archetype': archetype,
      if (visibleMood != null) 'visible_mood': visibleMood,
      if (interestsJson != null) 'interests_json': interestsJson,
      if (scheduleJson != null) 'schedule_json': scheduleJson,
      if (cachedAtUtc != null) 'cached_at_utc': cachedAtUtc,
      if (rowid != null) 'rowid': rowid,
    });
  }

  CachedCharacterDetailsCompanion copyWith({
    Value<String>? userId,
    Value<String>? worldId,
    Value<String>? characterId,
    Value<String>? displayName,
    Value<String>? handle,
    Value<String>? bio,
    Value<int>? age,
    Value<String>? profession,
    Value<String>? archetype,
    Value<String>? visibleMood,
    Value<String>? interestsJson,
    Value<String>? scheduleJson,
    Value<DateTime>? cachedAtUtc,
    Value<int>? rowid,
  }) {
    return CachedCharacterDetailsCompanion(
      userId: userId ?? this.userId,
      worldId: worldId ?? this.worldId,
      characterId: characterId ?? this.characterId,
      displayName: displayName ?? this.displayName,
      handle: handle ?? this.handle,
      bio: bio ?? this.bio,
      age: age ?? this.age,
      profession: profession ?? this.profession,
      archetype: archetype ?? this.archetype,
      visibleMood: visibleMood ?? this.visibleMood,
      interestsJson: interestsJson ?? this.interestsJson,
      scheduleJson: scheduleJson ?? this.scheduleJson,
      cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (userId.present) {
      map['user_id'] = Variable<String>(userId.value);
    }
    if (worldId.present) {
      map['world_id'] = Variable<String>(worldId.value);
    }
    if (characterId.present) {
      map['character_id'] = Variable<String>(characterId.value);
    }
    if (displayName.present) {
      map['display_name'] = Variable<String>(displayName.value);
    }
    if (handle.present) {
      map['handle'] = Variable<String>(handle.value);
    }
    if (bio.present) {
      map['bio'] = Variable<String>(bio.value);
    }
    if (age.present) {
      map['age'] = Variable<int>(age.value);
    }
    if (profession.present) {
      map['profession'] = Variable<String>(profession.value);
    }
    if (archetype.present) {
      map['archetype'] = Variable<String>(archetype.value);
    }
    if (visibleMood.present) {
      map['visible_mood'] = Variable<String>(visibleMood.value);
    }
    if (interestsJson.present) {
      map['interests_json'] = Variable<String>(interestsJson.value);
    }
    if (scheduleJson.present) {
      map['schedule_json'] = Variable<String>(scheduleJson.value);
    }
    if (cachedAtUtc.present) {
      map['cached_at_utc'] = Variable<DateTime>(cachedAtUtc.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('CachedCharacterDetailsCompanion(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('characterId: $characterId, ')
          ..write('displayName: $displayName, ')
          ..write('handle: $handle, ')
          ..write('bio: $bio, ')
          ..write('age: $age, ')
          ..write('profession: $profession, ')
          ..write('archetype: $archetype, ')
          ..write('visibleMood: $visibleMood, ')
          ..write('interestsJson: $interestsJson, ')
          ..write('scheduleJson: $scheduleJson, ')
          ..write('cachedAtUtc: $cachedAtUtc, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

abstract class _$AppDatabase extends GeneratedDatabase {
  _$AppDatabase(QueryExecutor e) : super(e);
  $AppDatabaseManager get managers => $AppDatabaseManager(this);
  late final $CachedWorldsTable cachedWorlds = $CachedWorldsTable(this);
  late final $CachedCharacterCataloguesTable cachedCharacterCatalogues =
      $CachedCharacterCataloguesTable(this);
  late final $CachedCharacterSummariesTable cachedCharacterSummaries =
      $CachedCharacterSummariesTable(this);
  late final $CachedCharacterDetailsTable cachedCharacterDetails =
      $CachedCharacterDetailsTable(this);
  @override
  Iterable<TableInfo<Table, Object?>> get allTables =>
      allSchemaEntities.whereType<TableInfo<Table, Object?>>();
  @override
  List<DatabaseSchemaEntity> get allSchemaEntities => [
    cachedWorlds,
    cachedCharacterCatalogues,
    cachedCharacterSummaries,
    cachedCharacterDetails,
  ];
}

typedef $$CachedWorldsTableCreateCompanionBuilder =
    CachedWorldsCompanion Function({
      required String userId,
      required String worldId,
      required String name,
      required String status,
      required DateTime currentGameTimeUtc,
      required String playerActorId,
      required String playerDisplayName,
      required DateTime createdAtUtc,
      required DateTime cachedAtUtc,
      Value<int> rowid,
    });
typedef $$CachedWorldsTableUpdateCompanionBuilder =
    CachedWorldsCompanion Function({
      Value<String> userId,
      Value<String> worldId,
      Value<String> name,
      Value<String> status,
      Value<DateTime> currentGameTimeUtc,
      Value<String> playerActorId,
      Value<String> playerDisplayName,
      Value<DateTime> createdAtUtc,
      Value<DateTime> cachedAtUtc,
      Value<int> rowid,
    });

class $$CachedWorldsTableFilterComposer
    extends Composer<_$AppDatabase, $CachedWorldsTable> {
  $$CachedWorldsTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get worldId => $composableBuilder(
    column: $table.worldId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get status => $composableBuilder(
    column: $table.status,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get currentGameTimeUtc => $composableBuilder(
    column: $table.currentGameTimeUtc,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get playerActorId => $composableBuilder(
    column: $table.playerActorId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get playerDisplayName => $composableBuilder(
    column: $table.playerDisplayName,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get createdAtUtc => $composableBuilder(
    column: $table.createdAtUtc,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnFilters(column),
  );
}

class $$CachedWorldsTableOrderingComposer
    extends Composer<_$AppDatabase, $CachedWorldsTable> {
  $$CachedWorldsTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get worldId => $composableBuilder(
    column: $table.worldId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get status => $composableBuilder(
    column: $table.status,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get currentGameTimeUtc => $composableBuilder(
    column: $table.currentGameTimeUtc,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get playerActorId => $composableBuilder(
    column: $table.playerActorId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get playerDisplayName => $composableBuilder(
    column: $table.playerDisplayName,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get createdAtUtc => $composableBuilder(
    column: $table.createdAtUtc,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$CachedWorldsTableAnnotationComposer
    extends Composer<_$AppDatabase, $CachedWorldsTable> {
  $$CachedWorldsTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get userId =>
      $composableBuilder(column: $table.userId, builder: (column) => column);

  GeneratedColumn<String> get worldId =>
      $composableBuilder(column: $table.worldId, builder: (column) => column);

  GeneratedColumn<String> get name =>
      $composableBuilder(column: $table.name, builder: (column) => column);

  GeneratedColumn<String> get status =>
      $composableBuilder(column: $table.status, builder: (column) => column);

  GeneratedColumn<DateTime> get currentGameTimeUtc => $composableBuilder(
    column: $table.currentGameTimeUtc,
    builder: (column) => column,
  );

  GeneratedColumn<String> get playerActorId => $composableBuilder(
    column: $table.playerActorId,
    builder: (column) => column,
  );

  GeneratedColumn<String> get playerDisplayName => $composableBuilder(
    column: $table.playerDisplayName,
    builder: (column) => column,
  );

  GeneratedColumn<DateTime> get createdAtUtc => $composableBuilder(
    column: $table.createdAtUtc,
    builder: (column) => column,
  );

  GeneratedColumn<DateTime> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => column,
  );
}

class $$CachedWorldsTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $CachedWorldsTable,
          CachedWorld,
          $$CachedWorldsTableFilterComposer,
          $$CachedWorldsTableOrderingComposer,
          $$CachedWorldsTableAnnotationComposer,
          $$CachedWorldsTableCreateCompanionBuilder,
          $$CachedWorldsTableUpdateCompanionBuilder,
          (
            CachedWorld,
            BaseReferences<_$AppDatabase, $CachedWorldsTable, CachedWorld>,
          ),
          CachedWorld,
          PrefetchHooks Function()
        > {
  $$CachedWorldsTableTableManager(_$AppDatabase db, $CachedWorldsTable table)
    : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$CachedWorldsTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$CachedWorldsTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$CachedWorldsTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> userId = const Value.absent(),
                Value<String> worldId = const Value.absent(),
                Value<String> name = const Value.absent(),
                Value<String> status = const Value.absent(),
                Value<DateTime> currentGameTimeUtc = const Value.absent(),
                Value<String> playerActorId = const Value.absent(),
                Value<String> playerDisplayName = const Value.absent(),
                Value<DateTime> createdAtUtc = const Value.absent(),
                Value<DateTime> cachedAtUtc = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => CachedWorldsCompanion(
                userId: userId,
                worldId: worldId,
                name: name,
                status: status,
                currentGameTimeUtc: currentGameTimeUtc,
                playerActorId: playerActorId,
                playerDisplayName: playerDisplayName,
                createdAtUtc: createdAtUtc,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String userId,
                required String worldId,
                required String name,
                required String status,
                required DateTime currentGameTimeUtc,
                required String playerActorId,
                required String playerDisplayName,
                required DateTime createdAtUtc,
                required DateTime cachedAtUtc,
                Value<int> rowid = const Value.absent(),
              }) => CachedWorldsCompanion.insert(
                userId: userId,
                worldId: worldId,
                name: name,
                status: status,
                currentGameTimeUtc: currentGameTimeUtc,
                playerActorId: playerActorId,
                playerDisplayName: playerDisplayName,
                createdAtUtc: createdAtUtc,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$CachedWorldsTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $CachedWorldsTable,
      CachedWorld,
      $$CachedWorldsTableFilterComposer,
      $$CachedWorldsTableOrderingComposer,
      $$CachedWorldsTableAnnotationComposer,
      $$CachedWorldsTableCreateCompanionBuilder,
      $$CachedWorldsTableUpdateCompanionBuilder,
      (
        CachedWorld,
        BaseReferences<_$AppDatabase, $CachedWorldsTable, CachedWorld>,
      ),
      CachedWorld,
      PrefetchHooks Function()
    >;
typedef $$CachedCharacterCataloguesTableCreateCompanionBuilder =
    CachedCharacterCataloguesCompanion Function({
      required String userId,
      required String worldId,
      required DateTime cachedAtUtc,
      Value<int> rowid,
    });
typedef $$CachedCharacterCataloguesTableUpdateCompanionBuilder =
    CachedCharacterCataloguesCompanion Function({
      Value<String> userId,
      Value<String> worldId,
      Value<DateTime> cachedAtUtc,
      Value<int> rowid,
    });

class $$CachedCharacterCataloguesTableFilterComposer
    extends Composer<_$AppDatabase, $CachedCharacterCataloguesTable> {
  $$CachedCharacterCataloguesTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get worldId => $composableBuilder(
    column: $table.worldId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnFilters(column),
  );
}

class $$CachedCharacterCataloguesTableOrderingComposer
    extends Composer<_$AppDatabase, $CachedCharacterCataloguesTable> {
  $$CachedCharacterCataloguesTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get worldId => $composableBuilder(
    column: $table.worldId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$CachedCharacterCataloguesTableAnnotationComposer
    extends Composer<_$AppDatabase, $CachedCharacterCataloguesTable> {
  $$CachedCharacterCataloguesTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get userId =>
      $composableBuilder(column: $table.userId, builder: (column) => column);

  GeneratedColumn<String> get worldId =>
      $composableBuilder(column: $table.worldId, builder: (column) => column);

  GeneratedColumn<DateTime> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => column,
  );
}

class $$CachedCharacterCataloguesTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $CachedCharacterCataloguesTable,
          CachedCharacterCatalogue,
          $$CachedCharacterCataloguesTableFilterComposer,
          $$CachedCharacterCataloguesTableOrderingComposer,
          $$CachedCharacterCataloguesTableAnnotationComposer,
          $$CachedCharacterCataloguesTableCreateCompanionBuilder,
          $$CachedCharacterCataloguesTableUpdateCompanionBuilder,
          (
            CachedCharacterCatalogue,
            BaseReferences<
              _$AppDatabase,
              $CachedCharacterCataloguesTable,
              CachedCharacterCatalogue
            >,
          ),
          CachedCharacterCatalogue,
          PrefetchHooks Function()
        > {
  $$CachedCharacterCataloguesTableTableManager(
    _$AppDatabase db,
    $CachedCharacterCataloguesTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$CachedCharacterCataloguesTableFilterComposer(
                $db: db,
                $table: table,
              ),
          createOrderingComposer: () =>
              $$CachedCharacterCataloguesTableOrderingComposer(
                $db: db,
                $table: table,
              ),
          createComputedFieldComposer: () =>
              $$CachedCharacterCataloguesTableAnnotationComposer(
                $db: db,
                $table: table,
              ),
          updateCompanionCallback:
              ({
                Value<String> userId = const Value.absent(),
                Value<String> worldId = const Value.absent(),
                Value<DateTime> cachedAtUtc = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => CachedCharacterCataloguesCompanion(
                userId: userId,
                worldId: worldId,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String userId,
                required String worldId,
                required DateTime cachedAtUtc,
                Value<int> rowid = const Value.absent(),
              }) => CachedCharacterCataloguesCompanion.insert(
                userId: userId,
                worldId: worldId,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$CachedCharacterCataloguesTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $CachedCharacterCataloguesTable,
      CachedCharacterCatalogue,
      $$CachedCharacterCataloguesTableFilterComposer,
      $$CachedCharacterCataloguesTableOrderingComposer,
      $$CachedCharacterCataloguesTableAnnotationComposer,
      $$CachedCharacterCataloguesTableCreateCompanionBuilder,
      $$CachedCharacterCataloguesTableUpdateCompanionBuilder,
      (
        CachedCharacterCatalogue,
        BaseReferences<
          _$AppDatabase,
          $CachedCharacterCataloguesTable,
          CachedCharacterCatalogue
        >,
      ),
      CachedCharacterCatalogue,
      PrefetchHooks Function()
    >;
typedef $$CachedCharacterSummariesTableCreateCompanionBuilder =
    CachedCharacterSummariesCompanion Function({
      required String userId,
      required String worldId,
      required String characterId,
      required String displayName,
      required String handle,
      required String profession,
      required String visibleMood,
      required bool isFollowed,
      Value<int> rowid,
    });
typedef $$CachedCharacterSummariesTableUpdateCompanionBuilder =
    CachedCharacterSummariesCompanion Function({
      Value<String> userId,
      Value<String> worldId,
      Value<String> characterId,
      Value<String> displayName,
      Value<String> handle,
      Value<String> profession,
      Value<String> visibleMood,
      Value<bool> isFollowed,
      Value<int> rowid,
    });

class $$CachedCharacterSummariesTableFilterComposer
    extends Composer<_$AppDatabase, $CachedCharacterSummariesTable> {
  $$CachedCharacterSummariesTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get worldId => $composableBuilder(
    column: $table.worldId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get characterId => $composableBuilder(
    column: $table.characterId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get displayName => $composableBuilder(
    column: $table.displayName,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get handle => $composableBuilder(
    column: $table.handle,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get profession => $composableBuilder(
    column: $table.profession,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get visibleMood => $composableBuilder(
    column: $table.visibleMood,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<bool> get isFollowed => $composableBuilder(
    column: $table.isFollowed,
    builder: (column) => ColumnFilters(column),
  );
}

class $$CachedCharacterSummariesTableOrderingComposer
    extends Composer<_$AppDatabase, $CachedCharacterSummariesTable> {
  $$CachedCharacterSummariesTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get worldId => $composableBuilder(
    column: $table.worldId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get characterId => $composableBuilder(
    column: $table.characterId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get displayName => $composableBuilder(
    column: $table.displayName,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get handle => $composableBuilder(
    column: $table.handle,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get profession => $composableBuilder(
    column: $table.profession,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get visibleMood => $composableBuilder(
    column: $table.visibleMood,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<bool> get isFollowed => $composableBuilder(
    column: $table.isFollowed,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$CachedCharacterSummariesTableAnnotationComposer
    extends Composer<_$AppDatabase, $CachedCharacterSummariesTable> {
  $$CachedCharacterSummariesTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get userId =>
      $composableBuilder(column: $table.userId, builder: (column) => column);

  GeneratedColumn<String> get worldId =>
      $composableBuilder(column: $table.worldId, builder: (column) => column);

  GeneratedColumn<String> get characterId => $composableBuilder(
    column: $table.characterId,
    builder: (column) => column,
  );

  GeneratedColumn<String> get displayName => $composableBuilder(
    column: $table.displayName,
    builder: (column) => column,
  );

  GeneratedColumn<String> get handle =>
      $composableBuilder(column: $table.handle, builder: (column) => column);

  GeneratedColumn<String> get profession => $composableBuilder(
    column: $table.profession,
    builder: (column) => column,
  );

  GeneratedColumn<String> get visibleMood => $composableBuilder(
    column: $table.visibleMood,
    builder: (column) => column,
  );

  GeneratedColumn<bool> get isFollowed => $composableBuilder(
    column: $table.isFollowed,
    builder: (column) => column,
  );
}

class $$CachedCharacterSummariesTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $CachedCharacterSummariesTable,
          CachedCharacterSummary,
          $$CachedCharacterSummariesTableFilterComposer,
          $$CachedCharacterSummariesTableOrderingComposer,
          $$CachedCharacterSummariesTableAnnotationComposer,
          $$CachedCharacterSummariesTableCreateCompanionBuilder,
          $$CachedCharacterSummariesTableUpdateCompanionBuilder,
          (
            CachedCharacterSummary,
            BaseReferences<
              _$AppDatabase,
              $CachedCharacterSummariesTable,
              CachedCharacterSummary
            >,
          ),
          CachedCharacterSummary,
          PrefetchHooks Function()
        > {
  $$CachedCharacterSummariesTableTableManager(
    _$AppDatabase db,
    $CachedCharacterSummariesTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$CachedCharacterSummariesTableFilterComposer(
                $db: db,
                $table: table,
              ),
          createOrderingComposer: () =>
              $$CachedCharacterSummariesTableOrderingComposer(
                $db: db,
                $table: table,
              ),
          createComputedFieldComposer: () =>
              $$CachedCharacterSummariesTableAnnotationComposer(
                $db: db,
                $table: table,
              ),
          updateCompanionCallback:
              ({
                Value<String> userId = const Value.absent(),
                Value<String> worldId = const Value.absent(),
                Value<String> characterId = const Value.absent(),
                Value<String> displayName = const Value.absent(),
                Value<String> handle = const Value.absent(),
                Value<String> profession = const Value.absent(),
                Value<String> visibleMood = const Value.absent(),
                Value<bool> isFollowed = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => CachedCharacterSummariesCompanion(
                userId: userId,
                worldId: worldId,
                characterId: characterId,
                displayName: displayName,
                handle: handle,
                profession: profession,
                visibleMood: visibleMood,
                isFollowed: isFollowed,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String userId,
                required String worldId,
                required String characterId,
                required String displayName,
                required String handle,
                required String profession,
                required String visibleMood,
                required bool isFollowed,
                Value<int> rowid = const Value.absent(),
              }) => CachedCharacterSummariesCompanion.insert(
                userId: userId,
                worldId: worldId,
                characterId: characterId,
                displayName: displayName,
                handle: handle,
                profession: profession,
                visibleMood: visibleMood,
                isFollowed: isFollowed,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$CachedCharacterSummariesTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $CachedCharacterSummariesTable,
      CachedCharacterSummary,
      $$CachedCharacterSummariesTableFilterComposer,
      $$CachedCharacterSummariesTableOrderingComposer,
      $$CachedCharacterSummariesTableAnnotationComposer,
      $$CachedCharacterSummariesTableCreateCompanionBuilder,
      $$CachedCharacterSummariesTableUpdateCompanionBuilder,
      (
        CachedCharacterSummary,
        BaseReferences<
          _$AppDatabase,
          $CachedCharacterSummariesTable,
          CachedCharacterSummary
        >,
      ),
      CachedCharacterSummary,
      PrefetchHooks Function()
    >;
typedef $$CachedCharacterDetailsTableCreateCompanionBuilder =
    CachedCharacterDetailsCompanion Function({
      required String userId,
      required String worldId,
      required String characterId,
      required String displayName,
      required String handle,
      required String bio,
      required int age,
      required String profession,
      required String archetype,
      required String visibleMood,
      required String interestsJson,
      required String scheduleJson,
      required DateTime cachedAtUtc,
      Value<int> rowid,
    });
typedef $$CachedCharacterDetailsTableUpdateCompanionBuilder =
    CachedCharacterDetailsCompanion Function({
      Value<String> userId,
      Value<String> worldId,
      Value<String> characterId,
      Value<String> displayName,
      Value<String> handle,
      Value<String> bio,
      Value<int> age,
      Value<String> profession,
      Value<String> archetype,
      Value<String> visibleMood,
      Value<String> interestsJson,
      Value<String> scheduleJson,
      Value<DateTime> cachedAtUtc,
      Value<int> rowid,
    });

class $$CachedCharacterDetailsTableFilterComposer
    extends Composer<_$AppDatabase, $CachedCharacterDetailsTable> {
  $$CachedCharacterDetailsTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get worldId => $composableBuilder(
    column: $table.worldId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get characterId => $composableBuilder(
    column: $table.characterId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get displayName => $composableBuilder(
    column: $table.displayName,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get handle => $composableBuilder(
    column: $table.handle,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get bio => $composableBuilder(
    column: $table.bio,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get age => $composableBuilder(
    column: $table.age,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get profession => $composableBuilder(
    column: $table.profession,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get archetype => $composableBuilder(
    column: $table.archetype,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get visibleMood => $composableBuilder(
    column: $table.visibleMood,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get interestsJson => $composableBuilder(
    column: $table.interestsJson,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get scheduleJson => $composableBuilder(
    column: $table.scheduleJson,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnFilters(column),
  );
}

class $$CachedCharacterDetailsTableOrderingComposer
    extends Composer<_$AppDatabase, $CachedCharacterDetailsTable> {
  $$CachedCharacterDetailsTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get worldId => $composableBuilder(
    column: $table.worldId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get characterId => $composableBuilder(
    column: $table.characterId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get displayName => $composableBuilder(
    column: $table.displayName,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get handle => $composableBuilder(
    column: $table.handle,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get bio => $composableBuilder(
    column: $table.bio,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get age => $composableBuilder(
    column: $table.age,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get profession => $composableBuilder(
    column: $table.profession,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get archetype => $composableBuilder(
    column: $table.archetype,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get visibleMood => $composableBuilder(
    column: $table.visibleMood,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get interestsJson => $composableBuilder(
    column: $table.interestsJson,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get scheduleJson => $composableBuilder(
    column: $table.scheduleJson,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$CachedCharacterDetailsTableAnnotationComposer
    extends Composer<_$AppDatabase, $CachedCharacterDetailsTable> {
  $$CachedCharacterDetailsTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get userId =>
      $composableBuilder(column: $table.userId, builder: (column) => column);

  GeneratedColumn<String> get worldId =>
      $composableBuilder(column: $table.worldId, builder: (column) => column);

  GeneratedColumn<String> get characterId => $composableBuilder(
    column: $table.characterId,
    builder: (column) => column,
  );

  GeneratedColumn<String> get displayName => $composableBuilder(
    column: $table.displayName,
    builder: (column) => column,
  );

  GeneratedColumn<String> get handle =>
      $composableBuilder(column: $table.handle, builder: (column) => column);

  GeneratedColumn<String> get bio =>
      $composableBuilder(column: $table.bio, builder: (column) => column);

  GeneratedColumn<int> get age =>
      $composableBuilder(column: $table.age, builder: (column) => column);

  GeneratedColumn<String> get profession => $composableBuilder(
    column: $table.profession,
    builder: (column) => column,
  );

  GeneratedColumn<String> get archetype =>
      $composableBuilder(column: $table.archetype, builder: (column) => column);

  GeneratedColumn<String> get visibleMood => $composableBuilder(
    column: $table.visibleMood,
    builder: (column) => column,
  );

  GeneratedColumn<String> get interestsJson => $composableBuilder(
    column: $table.interestsJson,
    builder: (column) => column,
  );

  GeneratedColumn<String> get scheduleJson => $composableBuilder(
    column: $table.scheduleJson,
    builder: (column) => column,
  );

  GeneratedColumn<DateTime> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => column,
  );
}

class $$CachedCharacterDetailsTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $CachedCharacterDetailsTable,
          CachedCharacterDetail,
          $$CachedCharacterDetailsTableFilterComposer,
          $$CachedCharacterDetailsTableOrderingComposer,
          $$CachedCharacterDetailsTableAnnotationComposer,
          $$CachedCharacterDetailsTableCreateCompanionBuilder,
          $$CachedCharacterDetailsTableUpdateCompanionBuilder,
          (
            CachedCharacterDetail,
            BaseReferences<
              _$AppDatabase,
              $CachedCharacterDetailsTable,
              CachedCharacterDetail
            >,
          ),
          CachedCharacterDetail,
          PrefetchHooks Function()
        > {
  $$CachedCharacterDetailsTableTableManager(
    _$AppDatabase db,
    $CachedCharacterDetailsTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$CachedCharacterDetailsTableFilterComposer(
                $db: db,
                $table: table,
              ),
          createOrderingComposer: () =>
              $$CachedCharacterDetailsTableOrderingComposer(
                $db: db,
                $table: table,
              ),
          createComputedFieldComposer: () =>
              $$CachedCharacterDetailsTableAnnotationComposer(
                $db: db,
                $table: table,
              ),
          updateCompanionCallback:
              ({
                Value<String> userId = const Value.absent(),
                Value<String> worldId = const Value.absent(),
                Value<String> characterId = const Value.absent(),
                Value<String> displayName = const Value.absent(),
                Value<String> handle = const Value.absent(),
                Value<String> bio = const Value.absent(),
                Value<int> age = const Value.absent(),
                Value<String> profession = const Value.absent(),
                Value<String> archetype = const Value.absent(),
                Value<String> visibleMood = const Value.absent(),
                Value<String> interestsJson = const Value.absent(),
                Value<String> scheduleJson = const Value.absent(),
                Value<DateTime> cachedAtUtc = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => CachedCharacterDetailsCompanion(
                userId: userId,
                worldId: worldId,
                characterId: characterId,
                displayName: displayName,
                handle: handle,
                bio: bio,
                age: age,
                profession: profession,
                archetype: archetype,
                visibleMood: visibleMood,
                interestsJson: interestsJson,
                scheduleJson: scheduleJson,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String userId,
                required String worldId,
                required String characterId,
                required String displayName,
                required String handle,
                required String bio,
                required int age,
                required String profession,
                required String archetype,
                required String visibleMood,
                required String interestsJson,
                required String scheduleJson,
                required DateTime cachedAtUtc,
                Value<int> rowid = const Value.absent(),
              }) => CachedCharacterDetailsCompanion.insert(
                userId: userId,
                worldId: worldId,
                characterId: characterId,
                displayName: displayName,
                handle: handle,
                bio: bio,
                age: age,
                profession: profession,
                archetype: archetype,
                visibleMood: visibleMood,
                interestsJson: interestsJson,
                scheduleJson: scheduleJson,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$CachedCharacterDetailsTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $CachedCharacterDetailsTable,
      CachedCharacterDetail,
      $$CachedCharacterDetailsTableFilterComposer,
      $$CachedCharacterDetailsTableOrderingComposer,
      $$CachedCharacterDetailsTableAnnotationComposer,
      $$CachedCharacterDetailsTableCreateCompanionBuilder,
      $$CachedCharacterDetailsTableUpdateCompanionBuilder,
      (
        CachedCharacterDetail,
        BaseReferences<
          _$AppDatabase,
          $CachedCharacterDetailsTable,
          CachedCharacterDetail
        >,
      ),
      CachedCharacterDetail,
      PrefetchHooks Function()
    >;

class $AppDatabaseManager {
  final _$AppDatabase _db;
  $AppDatabaseManager(this._db);
  $$CachedWorldsTableTableManager get cachedWorlds =>
      $$CachedWorldsTableTableManager(_db, _db.cachedWorlds);
  $$CachedCharacterCataloguesTableTableManager get cachedCharacterCatalogues =>
      $$CachedCharacterCataloguesTableTableManager(
        _db,
        _db.cachedCharacterCatalogues,
      );
  $$CachedCharacterSummariesTableTableManager get cachedCharacterSummaries =>
      $$CachedCharacterSummariesTableTableManager(
        _db,
        _db.cachedCharacterSummaries,
      );
  $$CachedCharacterDetailsTableTableManager get cachedCharacterDetails =>
      $$CachedCharacterDetailsTableTableManager(
        _db,
        _db.cachedCharacterDetails,
      );
}
