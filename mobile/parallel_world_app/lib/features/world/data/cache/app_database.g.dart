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

class $CachedFeedMetadataTable extends CachedFeedMetadata
    with TableInfo<$CachedFeedMetadataTable, CachedFeedMetadataData> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $CachedFeedMetadataTable(this.attachedDatabase, [this._alias]);
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
  static const VerificationMeta _nextCursorMeta = const VerificationMeta(
    'nextCursor',
  );
  @override
  late final GeneratedColumn<String> nextCursor = GeneratedColumn<String>(
    'next_cursor',
    aliasedName,
    true,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
  );
  static const VerificationMeta _hasMoreMeta = const VerificationMeta(
    'hasMore',
  );
  @override
  late final GeneratedColumn<bool> hasMore = GeneratedColumn<bool>(
    'has_more',
    aliasedName,
    false,
    type: DriftSqlType.bool,
    requiredDuringInsert: true,
    defaultConstraints: GeneratedColumn.constraintIsAlways(
      'CHECK ("has_more" IN (0, 1))',
    ),
  );
  @override
  List<GeneratedColumn> get $columns => [
    userId,
    worldId,
    cachedAtUtc,
    nextCursor,
    hasMore,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'cached_feed_metadata';
  @override
  VerificationContext validateIntegrity(
    Insertable<CachedFeedMetadataData> instance, {
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
    if (data.containsKey('next_cursor')) {
      context.handle(
        _nextCursorMeta,
        nextCursor.isAcceptableOrUnknown(data['next_cursor']!, _nextCursorMeta),
      );
    }
    if (data.containsKey('has_more')) {
      context.handle(
        _hasMoreMeta,
        hasMore.isAcceptableOrUnknown(data['has_more']!, _hasMoreMeta),
      );
    } else if (isInserting) {
      context.missing(_hasMoreMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {userId, worldId};
  @override
  CachedFeedMetadataData map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return CachedFeedMetadataData(
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
      nextCursor: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}next_cursor'],
      ),
      hasMore: attachedDatabase.typeMapping.read(
        DriftSqlType.bool,
        data['${effectivePrefix}has_more'],
      )!,
    );
  }

  @override
  $CachedFeedMetadataTable createAlias(String alias) {
    return $CachedFeedMetadataTable(attachedDatabase, alias);
  }
}

class CachedFeedMetadataData extends DataClass
    implements Insertable<CachedFeedMetadataData> {
  final String userId;
  final String worldId;
  final DateTime cachedAtUtc;
  final String? nextCursor;
  final bool hasMore;
  const CachedFeedMetadataData({
    required this.userId,
    required this.worldId,
    required this.cachedAtUtc,
    this.nextCursor,
    required this.hasMore,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['user_id'] = Variable<String>(userId);
    map['world_id'] = Variable<String>(worldId);
    map['cached_at_utc'] = Variable<DateTime>(cachedAtUtc);
    if (!nullToAbsent || nextCursor != null) {
      map['next_cursor'] = Variable<String>(nextCursor);
    }
    map['has_more'] = Variable<bool>(hasMore);
    return map;
  }

  CachedFeedMetadataCompanion toCompanion(bool nullToAbsent) {
    return CachedFeedMetadataCompanion(
      userId: Value(userId),
      worldId: Value(worldId),
      cachedAtUtc: Value(cachedAtUtc),
      nextCursor: nextCursor == null && nullToAbsent
          ? const Value.absent()
          : Value(nextCursor),
      hasMore: Value(hasMore),
    );
  }

  factory CachedFeedMetadataData.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return CachedFeedMetadataData(
      userId: serializer.fromJson<String>(json['userId']),
      worldId: serializer.fromJson<String>(json['worldId']),
      cachedAtUtc: serializer.fromJson<DateTime>(json['cachedAtUtc']),
      nextCursor: serializer.fromJson<String?>(json['nextCursor']),
      hasMore: serializer.fromJson<bool>(json['hasMore']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'userId': serializer.toJson<String>(userId),
      'worldId': serializer.toJson<String>(worldId),
      'cachedAtUtc': serializer.toJson<DateTime>(cachedAtUtc),
      'nextCursor': serializer.toJson<String?>(nextCursor),
      'hasMore': serializer.toJson<bool>(hasMore),
    };
  }

  CachedFeedMetadataData copyWith({
    String? userId,
    String? worldId,
    DateTime? cachedAtUtc,
    Value<String?> nextCursor = const Value.absent(),
    bool? hasMore,
  }) => CachedFeedMetadataData(
    userId: userId ?? this.userId,
    worldId: worldId ?? this.worldId,
    cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
    nextCursor: nextCursor.present ? nextCursor.value : this.nextCursor,
    hasMore: hasMore ?? this.hasMore,
  );
  CachedFeedMetadataData copyWithCompanion(CachedFeedMetadataCompanion data) {
    return CachedFeedMetadataData(
      userId: data.userId.present ? data.userId.value : this.userId,
      worldId: data.worldId.present ? data.worldId.value : this.worldId,
      cachedAtUtc: data.cachedAtUtc.present
          ? data.cachedAtUtc.value
          : this.cachedAtUtc,
      nextCursor: data.nextCursor.present
          ? data.nextCursor.value
          : this.nextCursor,
      hasMore: data.hasMore.present ? data.hasMore.value : this.hasMore,
    );
  }

  @override
  String toString() {
    return (StringBuffer('CachedFeedMetadataData(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('cachedAtUtc: $cachedAtUtc, ')
          ..write('nextCursor: $nextCursor, ')
          ..write('hasMore: $hasMore')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode =>
      Object.hash(userId, worldId, cachedAtUtc, nextCursor, hasMore);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is CachedFeedMetadataData &&
          other.userId == this.userId &&
          other.worldId == this.worldId &&
          other.cachedAtUtc == this.cachedAtUtc &&
          other.nextCursor == this.nextCursor &&
          other.hasMore == this.hasMore);
}

class CachedFeedMetadataCompanion
    extends UpdateCompanion<CachedFeedMetadataData> {
  final Value<String> userId;
  final Value<String> worldId;
  final Value<DateTime> cachedAtUtc;
  final Value<String?> nextCursor;
  final Value<bool> hasMore;
  final Value<int> rowid;
  const CachedFeedMetadataCompanion({
    this.userId = const Value.absent(),
    this.worldId = const Value.absent(),
    this.cachedAtUtc = const Value.absent(),
    this.nextCursor = const Value.absent(),
    this.hasMore = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  CachedFeedMetadataCompanion.insert({
    required String userId,
    required String worldId,
    required DateTime cachedAtUtc,
    this.nextCursor = const Value.absent(),
    required bool hasMore,
    this.rowid = const Value.absent(),
  }) : userId = Value(userId),
       worldId = Value(worldId),
       cachedAtUtc = Value(cachedAtUtc),
       hasMore = Value(hasMore);
  static Insertable<CachedFeedMetadataData> custom({
    Expression<String>? userId,
    Expression<String>? worldId,
    Expression<DateTime>? cachedAtUtc,
    Expression<String>? nextCursor,
    Expression<bool>? hasMore,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (userId != null) 'user_id': userId,
      if (worldId != null) 'world_id': worldId,
      if (cachedAtUtc != null) 'cached_at_utc': cachedAtUtc,
      if (nextCursor != null) 'next_cursor': nextCursor,
      if (hasMore != null) 'has_more': hasMore,
      if (rowid != null) 'rowid': rowid,
    });
  }

  CachedFeedMetadataCompanion copyWith({
    Value<String>? userId,
    Value<String>? worldId,
    Value<DateTime>? cachedAtUtc,
    Value<String?>? nextCursor,
    Value<bool>? hasMore,
    Value<int>? rowid,
  }) {
    return CachedFeedMetadataCompanion(
      userId: userId ?? this.userId,
      worldId: worldId ?? this.worldId,
      cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
      nextCursor: nextCursor ?? this.nextCursor,
      hasMore: hasMore ?? this.hasMore,
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
    if (nextCursor.present) {
      map['next_cursor'] = Variable<String>(nextCursor.value);
    }
    if (hasMore.present) {
      map['has_more'] = Variable<bool>(hasMore.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('CachedFeedMetadataCompanion(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('cachedAtUtc: $cachedAtUtc, ')
          ..write('nextCursor: $nextCursor, ')
          ..write('hasMore: $hasMore, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $CachedFeedPostsTable extends CachedFeedPosts
    with TableInfo<$CachedFeedPostsTable, CachedFeedPost> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $CachedFeedPostsTable(this.attachedDatabase, [this._alias]);
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
  static const VerificationMeta _postIdMeta = const VerificationMeta('postId');
  @override
  late final GeneratedColumn<String> postId = GeneratedColumn<String>(
    'post_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _authorActorIdMeta = const VerificationMeta(
    'authorActorId',
  );
  @override
  late final GeneratedColumn<String> authorActorId = GeneratedColumn<String>(
    'author_actor_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _authorDisplayNameMeta = const VerificationMeta(
    'authorDisplayName',
  );
  @override
  late final GeneratedColumn<String> authorDisplayName =
      GeneratedColumn<String>(
        'author_display_name',
        aliasedName,
        false,
        type: DriftSqlType.string,
        requiredDuringInsert: true,
      );
  static const VerificationMeta _authorHandleMeta = const VerificationMeta(
    'authorHandle',
  );
  @override
  late final GeneratedColumn<String> authorHandle = GeneratedColumn<String>(
    'author_handle',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _authorActorTypeMeta = const VerificationMeta(
    'authorActorType',
  );
  @override
  late final GeneratedColumn<String> authorActorType = GeneratedColumn<String>(
    'author_actor_type',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _contentMeta = const VerificationMeta(
    'content',
  );
  @override
  late final GeneratedColumn<String> content = GeneratedColumn<String>(
    'content',
    aliasedName,
    false,
    additionalChecks: GeneratedColumn.checkTextLength(
      minTextLength: 1,
      maxTextLength: 500,
    ),
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
  static const VerificationMeta _parentPostIdMeta = const VerificationMeta(
    'parentPostId',
  );
  @override
  late final GeneratedColumn<String> parentPostId = GeneratedColumn<String>(
    'parent_post_id',
    aliasedName,
    true,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
  );
  static const VerificationMeta _likeCountMeta = const VerificationMeta(
    'likeCount',
  );
  @override
  late final GeneratedColumn<int> likeCount = GeneratedColumn<int>(
    'like_count',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _replyCountMeta = const VerificationMeta(
    'replyCount',
  );
  @override
  late final GeneratedColumn<int> replyCount = GeneratedColumn<int>(
    'reply_count',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _visibilityMeta = const VerificationMeta(
    'visibility',
  );
  @override
  late final GeneratedColumn<String> visibility = GeneratedColumn<String>(
    'visibility',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _localStateMeta = const VerificationMeta(
    'localState',
  );
  @override
  late final GeneratedColumn<String> localState = GeneratedColumn<String>(
    'local_state',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _clientPostIdMeta = const VerificationMeta(
    'clientPostId',
  );
  @override
  late final GeneratedColumn<String> clientPostId = GeneratedColumn<String>(
    'client_post_id',
    aliasedName,
    true,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
  );
  static const VerificationMeta _idempotencyKeyMeta = const VerificationMeta(
    'idempotencyKey',
  );
  @override
  late final GeneratedColumn<String> idempotencyKey = GeneratedColumn<String>(
    'idempotency_key',
    aliasedName,
    true,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
  );
  static const VerificationMeta _failureMessageMeta = const VerificationMeta(
    'failureMessage',
  );
  @override
  late final GeneratedColumn<String> failureMessage = GeneratedColumn<String>(
    'failure_message',
    aliasedName,
    true,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
  );
  @override
  List<GeneratedColumn> get $columns => [
    userId,
    worldId,
    postId,
    authorActorId,
    authorDisplayName,
    authorHandle,
    authorActorType,
    content,
    createdAtUtc,
    parentPostId,
    likeCount,
    replyCount,
    visibility,
    localState,
    clientPostId,
    idempotencyKey,
    failureMessage,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'cached_feed_posts';
  @override
  VerificationContext validateIntegrity(
    Insertable<CachedFeedPost> instance, {
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
    if (data.containsKey('post_id')) {
      context.handle(
        _postIdMeta,
        postId.isAcceptableOrUnknown(data['post_id']!, _postIdMeta),
      );
    } else if (isInserting) {
      context.missing(_postIdMeta);
    }
    if (data.containsKey('author_actor_id')) {
      context.handle(
        _authorActorIdMeta,
        authorActorId.isAcceptableOrUnknown(
          data['author_actor_id']!,
          _authorActorIdMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_authorActorIdMeta);
    }
    if (data.containsKey('author_display_name')) {
      context.handle(
        _authorDisplayNameMeta,
        authorDisplayName.isAcceptableOrUnknown(
          data['author_display_name']!,
          _authorDisplayNameMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_authorDisplayNameMeta);
    }
    if (data.containsKey('author_handle')) {
      context.handle(
        _authorHandleMeta,
        authorHandle.isAcceptableOrUnknown(
          data['author_handle']!,
          _authorHandleMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_authorHandleMeta);
    }
    if (data.containsKey('author_actor_type')) {
      context.handle(
        _authorActorTypeMeta,
        authorActorType.isAcceptableOrUnknown(
          data['author_actor_type']!,
          _authorActorTypeMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_authorActorTypeMeta);
    }
    if (data.containsKey('content')) {
      context.handle(
        _contentMeta,
        content.isAcceptableOrUnknown(data['content']!, _contentMeta),
      );
    } else if (isInserting) {
      context.missing(_contentMeta);
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
    if (data.containsKey('parent_post_id')) {
      context.handle(
        _parentPostIdMeta,
        parentPostId.isAcceptableOrUnknown(
          data['parent_post_id']!,
          _parentPostIdMeta,
        ),
      );
    }
    if (data.containsKey('like_count')) {
      context.handle(
        _likeCountMeta,
        likeCount.isAcceptableOrUnknown(data['like_count']!, _likeCountMeta),
      );
    } else if (isInserting) {
      context.missing(_likeCountMeta);
    }
    if (data.containsKey('reply_count')) {
      context.handle(
        _replyCountMeta,
        replyCount.isAcceptableOrUnknown(data['reply_count']!, _replyCountMeta),
      );
    } else if (isInserting) {
      context.missing(_replyCountMeta);
    }
    if (data.containsKey('visibility')) {
      context.handle(
        _visibilityMeta,
        visibility.isAcceptableOrUnknown(data['visibility']!, _visibilityMeta),
      );
    } else if (isInserting) {
      context.missing(_visibilityMeta);
    }
    if (data.containsKey('local_state')) {
      context.handle(
        _localStateMeta,
        localState.isAcceptableOrUnknown(data['local_state']!, _localStateMeta),
      );
    } else if (isInserting) {
      context.missing(_localStateMeta);
    }
    if (data.containsKey('client_post_id')) {
      context.handle(
        _clientPostIdMeta,
        clientPostId.isAcceptableOrUnknown(
          data['client_post_id']!,
          _clientPostIdMeta,
        ),
      );
    }
    if (data.containsKey('idempotency_key')) {
      context.handle(
        _idempotencyKeyMeta,
        idempotencyKey.isAcceptableOrUnknown(
          data['idempotency_key']!,
          _idempotencyKeyMeta,
        ),
      );
    }
    if (data.containsKey('failure_message')) {
      context.handle(
        _failureMessageMeta,
        failureMessage.isAcceptableOrUnknown(
          data['failure_message']!,
          _failureMessageMeta,
        ),
      );
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {userId, worldId, postId};
  @override
  CachedFeedPost map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return CachedFeedPost(
      userId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}user_id'],
      )!,
      worldId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}world_id'],
      )!,
      postId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}post_id'],
      )!,
      authorActorId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}author_actor_id'],
      )!,
      authorDisplayName: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}author_display_name'],
      )!,
      authorHandle: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}author_handle'],
      )!,
      authorActorType: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}author_actor_type'],
      )!,
      content: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}content'],
      )!,
      createdAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}created_at_utc'],
      )!,
      parentPostId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}parent_post_id'],
      ),
      likeCount: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}like_count'],
      )!,
      replyCount: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}reply_count'],
      )!,
      visibility: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}visibility'],
      )!,
      localState: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}local_state'],
      )!,
      clientPostId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}client_post_id'],
      ),
      idempotencyKey: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}idempotency_key'],
      ),
      failureMessage: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}failure_message'],
      ),
    );
  }

  @override
  $CachedFeedPostsTable createAlias(String alias) {
    return $CachedFeedPostsTable(attachedDatabase, alias);
  }
}

class CachedFeedPost extends DataClass implements Insertable<CachedFeedPost> {
  final String userId;
  final String worldId;
  final String postId;
  final String authorActorId;
  final String authorDisplayName;
  final String authorHandle;
  final String authorActorType;
  final String content;
  final DateTime createdAtUtc;
  final String? parentPostId;
  final int likeCount;
  final int replyCount;
  final String visibility;
  final String localState;
  final String? clientPostId;
  final String? idempotencyKey;
  final String? failureMessage;
  const CachedFeedPost({
    required this.userId,
    required this.worldId,
    required this.postId,
    required this.authorActorId,
    required this.authorDisplayName,
    required this.authorHandle,
    required this.authorActorType,
    required this.content,
    required this.createdAtUtc,
    this.parentPostId,
    required this.likeCount,
    required this.replyCount,
    required this.visibility,
    required this.localState,
    this.clientPostId,
    this.idempotencyKey,
    this.failureMessage,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['user_id'] = Variable<String>(userId);
    map['world_id'] = Variable<String>(worldId);
    map['post_id'] = Variable<String>(postId);
    map['author_actor_id'] = Variable<String>(authorActorId);
    map['author_display_name'] = Variable<String>(authorDisplayName);
    map['author_handle'] = Variable<String>(authorHandle);
    map['author_actor_type'] = Variable<String>(authorActorType);
    map['content'] = Variable<String>(content);
    map['created_at_utc'] = Variable<DateTime>(createdAtUtc);
    if (!nullToAbsent || parentPostId != null) {
      map['parent_post_id'] = Variable<String>(parentPostId);
    }
    map['like_count'] = Variable<int>(likeCount);
    map['reply_count'] = Variable<int>(replyCount);
    map['visibility'] = Variable<String>(visibility);
    map['local_state'] = Variable<String>(localState);
    if (!nullToAbsent || clientPostId != null) {
      map['client_post_id'] = Variable<String>(clientPostId);
    }
    if (!nullToAbsent || idempotencyKey != null) {
      map['idempotency_key'] = Variable<String>(idempotencyKey);
    }
    if (!nullToAbsent || failureMessage != null) {
      map['failure_message'] = Variable<String>(failureMessage);
    }
    return map;
  }

  CachedFeedPostsCompanion toCompanion(bool nullToAbsent) {
    return CachedFeedPostsCompanion(
      userId: Value(userId),
      worldId: Value(worldId),
      postId: Value(postId),
      authorActorId: Value(authorActorId),
      authorDisplayName: Value(authorDisplayName),
      authorHandle: Value(authorHandle),
      authorActorType: Value(authorActorType),
      content: Value(content),
      createdAtUtc: Value(createdAtUtc),
      parentPostId: parentPostId == null && nullToAbsent
          ? const Value.absent()
          : Value(parentPostId),
      likeCount: Value(likeCount),
      replyCount: Value(replyCount),
      visibility: Value(visibility),
      localState: Value(localState),
      clientPostId: clientPostId == null && nullToAbsent
          ? const Value.absent()
          : Value(clientPostId),
      idempotencyKey: idempotencyKey == null && nullToAbsent
          ? const Value.absent()
          : Value(idempotencyKey),
      failureMessage: failureMessage == null && nullToAbsent
          ? const Value.absent()
          : Value(failureMessage),
    );
  }

  factory CachedFeedPost.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return CachedFeedPost(
      userId: serializer.fromJson<String>(json['userId']),
      worldId: serializer.fromJson<String>(json['worldId']),
      postId: serializer.fromJson<String>(json['postId']),
      authorActorId: serializer.fromJson<String>(json['authorActorId']),
      authorDisplayName: serializer.fromJson<String>(json['authorDisplayName']),
      authorHandle: serializer.fromJson<String>(json['authorHandle']),
      authorActorType: serializer.fromJson<String>(json['authorActorType']),
      content: serializer.fromJson<String>(json['content']),
      createdAtUtc: serializer.fromJson<DateTime>(json['createdAtUtc']),
      parentPostId: serializer.fromJson<String?>(json['parentPostId']),
      likeCount: serializer.fromJson<int>(json['likeCount']),
      replyCount: serializer.fromJson<int>(json['replyCount']),
      visibility: serializer.fromJson<String>(json['visibility']),
      localState: serializer.fromJson<String>(json['localState']),
      clientPostId: serializer.fromJson<String?>(json['clientPostId']),
      idempotencyKey: serializer.fromJson<String?>(json['idempotencyKey']),
      failureMessage: serializer.fromJson<String?>(json['failureMessage']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'userId': serializer.toJson<String>(userId),
      'worldId': serializer.toJson<String>(worldId),
      'postId': serializer.toJson<String>(postId),
      'authorActorId': serializer.toJson<String>(authorActorId),
      'authorDisplayName': serializer.toJson<String>(authorDisplayName),
      'authorHandle': serializer.toJson<String>(authorHandle),
      'authorActorType': serializer.toJson<String>(authorActorType),
      'content': serializer.toJson<String>(content),
      'createdAtUtc': serializer.toJson<DateTime>(createdAtUtc),
      'parentPostId': serializer.toJson<String?>(parentPostId),
      'likeCount': serializer.toJson<int>(likeCount),
      'replyCount': serializer.toJson<int>(replyCount),
      'visibility': serializer.toJson<String>(visibility),
      'localState': serializer.toJson<String>(localState),
      'clientPostId': serializer.toJson<String?>(clientPostId),
      'idempotencyKey': serializer.toJson<String?>(idempotencyKey),
      'failureMessage': serializer.toJson<String?>(failureMessage),
    };
  }

  CachedFeedPost copyWith({
    String? userId,
    String? worldId,
    String? postId,
    String? authorActorId,
    String? authorDisplayName,
    String? authorHandle,
    String? authorActorType,
    String? content,
    DateTime? createdAtUtc,
    Value<String?> parentPostId = const Value.absent(),
    int? likeCount,
    int? replyCount,
    String? visibility,
    String? localState,
    Value<String?> clientPostId = const Value.absent(),
    Value<String?> idempotencyKey = const Value.absent(),
    Value<String?> failureMessage = const Value.absent(),
  }) => CachedFeedPost(
    userId: userId ?? this.userId,
    worldId: worldId ?? this.worldId,
    postId: postId ?? this.postId,
    authorActorId: authorActorId ?? this.authorActorId,
    authorDisplayName: authorDisplayName ?? this.authorDisplayName,
    authorHandle: authorHandle ?? this.authorHandle,
    authorActorType: authorActorType ?? this.authorActorType,
    content: content ?? this.content,
    createdAtUtc: createdAtUtc ?? this.createdAtUtc,
    parentPostId: parentPostId.present ? parentPostId.value : this.parentPostId,
    likeCount: likeCount ?? this.likeCount,
    replyCount: replyCount ?? this.replyCount,
    visibility: visibility ?? this.visibility,
    localState: localState ?? this.localState,
    clientPostId: clientPostId.present ? clientPostId.value : this.clientPostId,
    idempotencyKey: idempotencyKey.present
        ? idempotencyKey.value
        : this.idempotencyKey,
    failureMessage: failureMessage.present
        ? failureMessage.value
        : this.failureMessage,
  );
  CachedFeedPost copyWithCompanion(CachedFeedPostsCompanion data) {
    return CachedFeedPost(
      userId: data.userId.present ? data.userId.value : this.userId,
      worldId: data.worldId.present ? data.worldId.value : this.worldId,
      postId: data.postId.present ? data.postId.value : this.postId,
      authorActorId: data.authorActorId.present
          ? data.authorActorId.value
          : this.authorActorId,
      authorDisplayName: data.authorDisplayName.present
          ? data.authorDisplayName.value
          : this.authorDisplayName,
      authorHandle: data.authorHandle.present
          ? data.authorHandle.value
          : this.authorHandle,
      authorActorType: data.authorActorType.present
          ? data.authorActorType.value
          : this.authorActorType,
      content: data.content.present ? data.content.value : this.content,
      createdAtUtc: data.createdAtUtc.present
          ? data.createdAtUtc.value
          : this.createdAtUtc,
      parentPostId: data.parentPostId.present
          ? data.parentPostId.value
          : this.parentPostId,
      likeCount: data.likeCount.present ? data.likeCount.value : this.likeCount,
      replyCount: data.replyCount.present
          ? data.replyCount.value
          : this.replyCount,
      visibility: data.visibility.present
          ? data.visibility.value
          : this.visibility,
      localState: data.localState.present
          ? data.localState.value
          : this.localState,
      clientPostId: data.clientPostId.present
          ? data.clientPostId.value
          : this.clientPostId,
      idempotencyKey: data.idempotencyKey.present
          ? data.idempotencyKey.value
          : this.idempotencyKey,
      failureMessage: data.failureMessage.present
          ? data.failureMessage.value
          : this.failureMessage,
    );
  }

  @override
  String toString() {
    return (StringBuffer('CachedFeedPost(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('postId: $postId, ')
          ..write('authorActorId: $authorActorId, ')
          ..write('authorDisplayName: $authorDisplayName, ')
          ..write('authorHandle: $authorHandle, ')
          ..write('authorActorType: $authorActorType, ')
          ..write('content: $content, ')
          ..write('createdAtUtc: $createdAtUtc, ')
          ..write('parentPostId: $parentPostId, ')
          ..write('likeCount: $likeCount, ')
          ..write('replyCount: $replyCount, ')
          ..write('visibility: $visibility, ')
          ..write('localState: $localState, ')
          ..write('clientPostId: $clientPostId, ')
          ..write('idempotencyKey: $idempotencyKey, ')
          ..write('failureMessage: $failureMessage')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(
    userId,
    worldId,
    postId,
    authorActorId,
    authorDisplayName,
    authorHandle,
    authorActorType,
    content,
    createdAtUtc,
    parentPostId,
    likeCount,
    replyCount,
    visibility,
    localState,
    clientPostId,
    idempotencyKey,
    failureMessage,
  );
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is CachedFeedPost &&
          other.userId == this.userId &&
          other.worldId == this.worldId &&
          other.postId == this.postId &&
          other.authorActorId == this.authorActorId &&
          other.authorDisplayName == this.authorDisplayName &&
          other.authorHandle == this.authorHandle &&
          other.authorActorType == this.authorActorType &&
          other.content == this.content &&
          other.createdAtUtc == this.createdAtUtc &&
          other.parentPostId == this.parentPostId &&
          other.likeCount == this.likeCount &&
          other.replyCount == this.replyCount &&
          other.visibility == this.visibility &&
          other.localState == this.localState &&
          other.clientPostId == this.clientPostId &&
          other.idempotencyKey == this.idempotencyKey &&
          other.failureMessage == this.failureMessage);
}

class CachedFeedPostsCompanion extends UpdateCompanion<CachedFeedPost> {
  final Value<String> userId;
  final Value<String> worldId;
  final Value<String> postId;
  final Value<String> authorActorId;
  final Value<String> authorDisplayName;
  final Value<String> authorHandle;
  final Value<String> authorActorType;
  final Value<String> content;
  final Value<DateTime> createdAtUtc;
  final Value<String?> parentPostId;
  final Value<int> likeCount;
  final Value<int> replyCount;
  final Value<String> visibility;
  final Value<String> localState;
  final Value<String?> clientPostId;
  final Value<String?> idempotencyKey;
  final Value<String?> failureMessage;
  final Value<int> rowid;
  const CachedFeedPostsCompanion({
    this.userId = const Value.absent(),
    this.worldId = const Value.absent(),
    this.postId = const Value.absent(),
    this.authorActorId = const Value.absent(),
    this.authorDisplayName = const Value.absent(),
    this.authorHandle = const Value.absent(),
    this.authorActorType = const Value.absent(),
    this.content = const Value.absent(),
    this.createdAtUtc = const Value.absent(),
    this.parentPostId = const Value.absent(),
    this.likeCount = const Value.absent(),
    this.replyCount = const Value.absent(),
    this.visibility = const Value.absent(),
    this.localState = const Value.absent(),
    this.clientPostId = const Value.absent(),
    this.idempotencyKey = const Value.absent(),
    this.failureMessage = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  CachedFeedPostsCompanion.insert({
    required String userId,
    required String worldId,
    required String postId,
    required String authorActorId,
    required String authorDisplayName,
    required String authorHandle,
    required String authorActorType,
    required String content,
    required DateTime createdAtUtc,
    this.parentPostId = const Value.absent(),
    required int likeCount,
    required int replyCount,
    required String visibility,
    required String localState,
    this.clientPostId = const Value.absent(),
    this.idempotencyKey = const Value.absent(),
    this.failureMessage = const Value.absent(),
    this.rowid = const Value.absent(),
  }) : userId = Value(userId),
       worldId = Value(worldId),
       postId = Value(postId),
       authorActorId = Value(authorActorId),
       authorDisplayName = Value(authorDisplayName),
       authorHandle = Value(authorHandle),
       authorActorType = Value(authorActorType),
       content = Value(content),
       createdAtUtc = Value(createdAtUtc),
       likeCount = Value(likeCount),
       replyCount = Value(replyCount),
       visibility = Value(visibility),
       localState = Value(localState);
  static Insertable<CachedFeedPost> custom({
    Expression<String>? userId,
    Expression<String>? worldId,
    Expression<String>? postId,
    Expression<String>? authorActorId,
    Expression<String>? authorDisplayName,
    Expression<String>? authorHandle,
    Expression<String>? authorActorType,
    Expression<String>? content,
    Expression<DateTime>? createdAtUtc,
    Expression<String>? parentPostId,
    Expression<int>? likeCount,
    Expression<int>? replyCount,
    Expression<String>? visibility,
    Expression<String>? localState,
    Expression<String>? clientPostId,
    Expression<String>? idempotencyKey,
    Expression<String>? failureMessage,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (userId != null) 'user_id': userId,
      if (worldId != null) 'world_id': worldId,
      if (postId != null) 'post_id': postId,
      if (authorActorId != null) 'author_actor_id': authorActorId,
      if (authorDisplayName != null) 'author_display_name': authorDisplayName,
      if (authorHandle != null) 'author_handle': authorHandle,
      if (authorActorType != null) 'author_actor_type': authorActorType,
      if (content != null) 'content': content,
      if (createdAtUtc != null) 'created_at_utc': createdAtUtc,
      if (parentPostId != null) 'parent_post_id': parentPostId,
      if (likeCount != null) 'like_count': likeCount,
      if (replyCount != null) 'reply_count': replyCount,
      if (visibility != null) 'visibility': visibility,
      if (localState != null) 'local_state': localState,
      if (clientPostId != null) 'client_post_id': clientPostId,
      if (idempotencyKey != null) 'idempotency_key': idempotencyKey,
      if (failureMessage != null) 'failure_message': failureMessage,
      if (rowid != null) 'rowid': rowid,
    });
  }

  CachedFeedPostsCompanion copyWith({
    Value<String>? userId,
    Value<String>? worldId,
    Value<String>? postId,
    Value<String>? authorActorId,
    Value<String>? authorDisplayName,
    Value<String>? authorHandle,
    Value<String>? authorActorType,
    Value<String>? content,
    Value<DateTime>? createdAtUtc,
    Value<String?>? parentPostId,
    Value<int>? likeCount,
    Value<int>? replyCount,
    Value<String>? visibility,
    Value<String>? localState,
    Value<String?>? clientPostId,
    Value<String?>? idempotencyKey,
    Value<String?>? failureMessage,
    Value<int>? rowid,
  }) {
    return CachedFeedPostsCompanion(
      userId: userId ?? this.userId,
      worldId: worldId ?? this.worldId,
      postId: postId ?? this.postId,
      authorActorId: authorActorId ?? this.authorActorId,
      authorDisplayName: authorDisplayName ?? this.authorDisplayName,
      authorHandle: authorHandle ?? this.authorHandle,
      authorActorType: authorActorType ?? this.authorActorType,
      content: content ?? this.content,
      createdAtUtc: createdAtUtc ?? this.createdAtUtc,
      parentPostId: parentPostId ?? this.parentPostId,
      likeCount: likeCount ?? this.likeCount,
      replyCount: replyCount ?? this.replyCount,
      visibility: visibility ?? this.visibility,
      localState: localState ?? this.localState,
      clientPostId: clientPostId ?? this.clientPostId,
      idempotencyKey: idempotencyKey ?? this.idempotencyKey,
      failureMessage: failureMessage ?? this.failureMessage,
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
    if (postId.present) {
      map['post_id'] = Variable<String>(postId.value);
    }
    if (authorActorId.present) {
      map['author_actor_id'] = Variable<String>(authorActorId.value);
    }
    if (authorDisplayName.present) {
      map['author_display_name'] = Variable<String>(authorDisplayName.value);
    }
    if (authorHandle.present) {
      map['author_handle'] = Variable<String>(authorHandle.value);
    }
    if (authorActorType.present) {
      map['author_actor_type'] = Variable<String>(authorActorType.value);
    }
    if (content.present) {
      map['content'] = Variable<String>(content.value);
    }
    if (createdAtUtc.present) {
      map['created_at_utc'] = Variable<DateTime>(createdAtUtc.value);
    }
    if (parentPostId.present) {
      map['parent_post_id'] = Variable<String>(parentPostId.value);
    }
    if (likeCount.present) {
      map['like_count'] = Variable<int>(likeCount.value);
    }
    if (replyCount.present) {
      map['reply_count'] = Variable<int>(replyCount.value);
    }
    if (visibility.present) {
      map['visibility'] = Variable<String>(visibility.value);
    }
    if (localState.present) {
      map['local_state'] = Variable<String>(localState.value);
    }
    if (clientPostId.present) {
      map['client_post_id'] = Variable<String>(clientPostId.value);
    }
    if (idempotencyKey.present) {
      map['idempotency_key'] = Variable<String>(idempotencyKey.value);
    }
    if (failureMessage.present) {
      map['failure_message'] = Variable<String>(failureMessage.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('CachedFeedPostsCompanion(')
          ..write('userId: $userId, ')
          ..write('worldId: $worldId, ')
          ..write('postId: $postId, ')
          ..write('authorActorId: $authorActorId, ')
          ..write('authorDisplayName: $authorDisplayName, ')
          ..write('authorHandle: $authorHandle, ')
          ..write('authorActorType: $authorActorType, ')
          ..write('content: $content, ')
          ..write('createdAtUtc: $createdAtUtc, ')
          ..write('parentPostId: $parentPostId, ')
          ..write('likeCount: $likeCount, ')
          ..write('replyCount: $replyCount, ')
          ..write('visibility: $visibility, ')
          ..write('localState: $localState, ')
          ..write('clientPostId: $clientPostId, ')
          ..write('idempotencyKey: $idempotencyKey, ')
          ..write('failureMessage: $failureMessage, ')
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
  late final $CachedFeedMetadataTable cachedFeedMetadata =
      $CachedFeedMetadataTable(this);
  late final $CachedFeedPostsTable cachedFeedPosts = $CachedFeedPostsTable(
    this,
  );
  @override
  Iterable<TableInfo<Table, Object?>> get allTables =>
      allSchemaEntities.whereType<TableInfo<Table, Object?>>();
  @override
  List<DatabaseSchemaEntity> get allSchemaEntities => [
    cachedWorlds,
    cachedCharacterCatalogues,
    cachedCharacterSummaries,
    cachedCharacterDetails,
    cachedFeedMetadata,
    cachedFeedPosts,
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
typedef $$CachedFeedMetadataTableCreateCompanionBuilder =
    CachedFeedMetadataCompanion Function({
      required String userId,
      required String worldId,
      required DateTime cachedAtUtc,
      Value<String?> nextCursor,
      required bool hasMore,
      Value<int> rowid,
    });
typedef $$CachedFeedMetadataTableUpdateCompanionBuilder =
    CachedFeedMetadataCompanion Function({
      Value<String> userId,
      Value<String> worldId,
      Value<DateTime> cachedAtUtc,
      Value<String?> nextCursor,
      Value<bool> hasMore,
      Value<int> rowid,
    });

class $$CachedFeedMetadataTableFilterComposer
    extends Composer<_$AppDatabase, $CachedFeedMetadataTable> {
  $$CachedFeedMetadataTableFilterComposer({
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

  ColumnFilters<String> get nextCursor => $composableBuilder(
    column: $table.nextCursor,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<bool> get hasMore => $composableBuilder(
    column: $table.hasMore,
    builder: (column) => ColumnFilters(column),
  );
}

class $$CachedFeedMetadataTableOrderingComposer
    extends Composer<_$AppDatabase, $CachedFeedMetadataTable> {
  $$CachedFeedMetadataTableOrderingComposer({
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

  ColumnOrderings<String> get nextCursor => $composableBuilder(
    column: $table.nextCursor,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<bool> get hasMore => $composableBuilder(
    column: $table.hasMore,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$CachedFeedMetadataTableAnnotationComposer
    extends Composer<_$AppDatabase, $CachedFeedMetadataTable> {
  $$CachedFeedMetadataTableAnnotationComposer({
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

  GeneratedColumn<String> get nextCursor => $composableBuilder(
    column: $table.nextCursor,
    builder: (column) => column,
  );

  GeneratedColumn<bool> get hasMore =>
      $composableBuilder(column: $table.hasMore, builder: (column) => column);
}

class $$CachedFeedMetadataTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $CachedFeedMetadataTable,
          CachedFeedMetadataData,
          $$CachedFeedMetadataTableFilterComposer,
          $$CachedFeedMetadataTableOrderingComposer,
          $$CachedFeedMetadataTableAnnotationComposer,
          $$CachedFeedMetadataTableCreateCompanionBuilder,
          $$CachedFeedMetadataTableUpdateCompanionBuilder,
          (
            CachedFeedMetadataData,
            BaseReferences<
              _$AppDatabase,
              $CachedFeedMetadataTable,
              CachedFeedMetadataData
            >,
          ),
          CachedFeedMetadataData,
          PrefetchHooks Function()
        > {
  $$CachedFeedMetadataTableTableManager(
    _$AppDatabase db,
    $CachedFeedMetadataTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$CachedFeedMetadataTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$CachedFeedMetadataTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$CachedFeedMetadataTableAnnotationComposer(
                $db: db,
                $table: table,
              ),
          updateCompanionCallback:
              ({
                Value<String> userId = const Value.absent(),
                Value<String> worldId = const Value.absent(),
                Value<DateTime> cachedAtUtc = const Value.absent(),
                Value<String?> nextCursor = const Value.absent(),
                Value<bool> hasMore = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => CachedFeedMetadataCompanion(
                userId: userId,
                worldId: worldId,
                cachedAtUtc: cachedAtUtc,
                nextCursor: nextCursor,
                hasMore: hasMore,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String userId,
                required String worldId,
                required DateTime cachedAtUtc,
                Value<String?> nextCursor = const Value.absent(),
                required bool hasMore,
                Value<int> rowid = const Value.absent(),
              }) => CachedFeedMetadataCompanion.insert(
                userId: userId,
                worldId: worldId,
                cachedAtUtc: cachedAtUtc,
                nextCursor: nextCursor,
                hasMore: hasMore,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$CachedFeedMetadataTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $CachedFeedMetadataTable,
      CachedFeedMetadataData,
      $$CachedFeedMetadataTableFilterComposer,
      $$CachedFeedMetadataTableOrderingComposer,
      $$CachedFeedMetadataTableAnnotationComposer,
      $$CachedFeedMetadataTableCreateCompanionBuilder,
      $$CachedFeedMetadataTableUpdateCompanionBuilder,
      (
        CachedFeedMetadataData,
        BaseReferences<
          _$AppDatabase,
          $CachedFeedMetadataTable,
          CachedFeedMetadataData
        >,
      ),
      CachedFeedMetadataData,
      PrefetchHooks Function()
    >;
typedef $$CachedFeedPostsTableCreateCompanionBuilder =
    CachedFeedPostsCompanion Function({
      required String userId,
      required String worldId,
      required String postId,
      required String authorActorId,
      required String authorDisplayName,
      required String authorHandle,
      required String authorActorType,
      required String content,
      required DateTime createdAtUtc,
      Value<String?> parentPostId,
      required int likeCount,
      required int replyCount,
      required String visibility,
      required String localState,
      Value<String?> clientPostId,
      Value<String?> idempotencyKey,
      Value<String?> failureMessage,
      Value<int> rowid,
    });
typedef $$CachedFeedPostsTableUpdateCompanionBuilder =
    CachedFeedPostsCompanion Function({
      Value<String> userId,
      Value<String> worldId,
      Value<String> postId,
      Value<String> authorActorId,
      Value<String> authorDisplayName,
      Value<String> authorHandle,
      Value<String> authorActorType,
      Value<String> content,
      Value<DateTime> createdAtUtc,
      Value<String?> parentPostId,
      Value<int> likeCount,
      Value<int> replyCount,
      Value<String> visibility,
      Value<String> localState,
      Value<String?> clientPostId,
      Value<String?> idempotencyKey,
      Value<String?> failureMessage,
      Value<int> rowid,
    });

class $$CachedFeedPostsTableFilterComposer
    extends Composer<_$AppDatabase, $CachedFeedPostsTable> {
  $$CachedFeedPostsTableFilterComposer({
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

  ColumnFilters<String> get postId => $composableBuilder(
    column: $table.postId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get authorActorId => $composableBuilder(
    column: $table.authorActorId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get authorDisplayName => $composableBuilder(
    column: $table.authorDisplayName,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get authorHandle => $composableBuilder(
    column: $table.authorHandle,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get authorActorType => $composableBuilder(
    column: $table.authorActorType,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get content => $composableBuilder(
    column: $table.content,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get createdAtUtc => $composableBuilder(
    column: $table.createdAtUtc,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get parentPostId => $composableBuilder(
    column: $table.parentPostId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get likeCount => $composableBuilder(
    column: $table.likeCount,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get replyCount => $composableBuilder(
    column: $table.replyCount,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get visibility => $composableBuilder(
    column: $table.visibility,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get localState => $composableBuilder(
    column: $table.localState,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get clientPostId => $composableBuilder(
    column: $table.clientPostId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get idempotencyKey => $composableBuilder(
    column: $table.idempotencyKey,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get failureMessage => $composableBuilder(
    column: $table.failureMessage,
    builder: (column) => ColumnFilters(column),
  );
}

class $$CachedFeedPostsTableOrderingComposer
    extends Composer<_$AppDatabase, $CachedFeedPostsTable> {
  $$CachedFeedPostsTableOrderingComposer({
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

  ColumnOrderings<String> get postId => $composableBuilder(
    column: $table.postId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get authorActorId => $composableBuilder(
    column: $table.authorActorId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get authorDisplayName => $composableBuilder(
    column: $table.authorDisplayName,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get authorHandle => $composableBuilder(
    column: $table.authorHandle,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get authorActorType => $composableBuilder(
    column: $table.authorActorType,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get content => $composableBuilder(
    column: $table.content,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get createdAtUtc => $composableBuilder(
    column: $table.createdAtUtc,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get parentPostId => $composableBuilder(
    column: $table.parentPostId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get likeCount => $composableBuilder(
    column: $table.likeCount,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get replyCount => $composableBuilder(
    column: $table.replyCount,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get visibility => $composableBuilder(
    column: $table.visibility,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get localState => $composableBuilder(
    column: $table.localState,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get clientPostId => $composableBuilder(
    column: $table.clientPostId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get idempotencyKey => $composableBuilder(
    column: $table.idempotencyKey,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get failureMessage => $composableBuilder(
    column: $table.failureMessage,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$CachedFeedPostsTableAnnotationComposer
    extends Composer<_$AppDatabase, $CachedFeedPostsTable> {
  $$CachedFeedPostsTableAnnotationComposer({
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

  GeneratedColumn<String> get postId =>
      $composableBuilder(column: $table.postId, builder: (column) => column);

  GeneratedColumn<String> get authorActorId => $composableBuilder(
    column: $table.authorActorId,
    builder: (column) => column,
  );

  GeneratedColumn<String> get authorDisplayName => $composableBuilder(
    column: $table.authorDisplayName,
    builder: (column) => column,
  );

  GeneratedColumn<String> get authorHandle => $composableBuilder(
    column: $table.authorHandle,
    builder: (column) => column,
  );

  GeneratedColumn<String> get authorActorType => $composableBuilder(
    column: $table.authorActorType,
    builder: (column) => column,
  );

  GeneratedColumn<String> get content =>
      $composableBuilder(column: $table.content, builder: (column) => column);

  GeneratedColumn<DateTime> get createdAtUtc => $composableBuilder(
    column: $table.createdAtUtc,
    builder: (column) => column,
  );

  GeneratedColumn<String> get parentPostId => $composableBuilder(
    column: $table.parentPostId,
    builder: (column) => column,
  );

  GeneratedColumn<int> get likeCount =>
      $composableBuilder(column: $table.likeCount, builder: (column) => column);

  GeneratedColumn<int> get replyCount => $composableBuilder(
    column: $table.replyCount,
    builder: (column) => column,
  );

  GeneratedColumn<String> get visibility => $composableBuilder(
    column: $table.visibility,
    builder: (column) => column,
  );

  GeneratedColumn<String> get localState => $composableBuilder(
    column: $table.localState,
    builder: (column) => column,
  );

  GeneratedColumn<String> get clientPostId => $composableBuilder(
    column: $table.clientPostId,
    builder: (column) => column,
  );

  GeneratedColumn<String> get idempotencyKey => $composableBuilder(
    column: $table.idempotencyKey,
    builder: (column) => column,
  );

  GeneratedColumn<String> get failureMessage => $composableBuilder(
    column: $table.failureMessage,
    builder: (column) => column,
  );
}

class $$CachedFeedPostsTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $CachedFeedPostsTable,
          CachedFeedPost,
          $$CachedFeedPostsTableFilterComposer,
          $$CachedFeedPostsTableOrderingComposer,
          $$CachedFeedPostsTableAnnotationComposer,
          $$CachedFeedPostsTableCreateCompanionBuilder,
          $$CachedFeedPostsTableUpdateCompanionBuilder,
          (
            CachedFeedPost,
            BaseReferences<
              _$AppDatabase,
              $CachedFeedPostsTable,
              CachedFeedPost
            >,
          ),
          CachedFeedPost,
          PrefetchHooks Function()
        > {
  $$CachedFeedPostsTableTableManager(
    _$AppDatabase db,
    $CachedFeedPostsTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$CachedFeedPostsTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$CachedFeedPostsTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$CachedFeedPostsTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> userId = const Value.absent(),
                Value<String> worldId = const Value.absent(),
                Value<String> postId = const Value.absent(),
                Value<String> authorActorId = const Value.absent(),
                Value<String> authorDisplayName = const Value.absent(),
                Value<String> authorHandle = const Value.absent(),
                Value<String> authorActorType = const Value.absent(),
                Value<String> content = const Value.absent(),
                Value<DateTime> createdAtUtc = const Value.absent(),
                Value<String?> parentPostId = const Value.absent(),
                Value<int> likeCount = const Value.absent(),
                Value<int> replyCount = const Value.absent(),
                Value<String> visibility = const Value.absent(),
                Value<String> localState = const Value.absent(),
                Value<String?> clientPostId = const Value.absent(),
                Value<String?> idempotencyKey = const Value.absent(),
                Value<String?> failureMessage = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => CachedFeedPostsCompanion(
                userId: userId,
                worldId: worldId,
                postId: postId,
                authorActorId: authorActorId,
                authorDisplayName: authorDisplayName,
                authorHandle: authorHandle,
                authorActorType: authorActorType,
                content: content,
                createdAtUtc: createdAtUtc,
                parentPostId: parentPostId,
                likeCount: likeCount,
                replyCount: replyCount,
                visibility: visibility,
                localState: localState,
                clientPostId: clientPostId,
                idempotencyKey: idempotencyKey,
                failureMessage: failureMessage,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String userId,
                required String worldId,
                required String postId,
                required String authorActorId,
                required String authorDisplayName,
                required String authorHandle,
                required String authorActorType,
                required String content,
                required DateTime createdAtUtc,
                Value<String?> parentPostId = const Value.absent(),
                required int likeCount,
                required int replyCount,
                required String visibility,
                required String localState,
                Value<String?> clientPostId = const Value.absent(),
                Value<String?> idempotencyKey = const Value.absent(),
                Value<String?> failureMessage = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => CachedFeedPostsCompanion.insert(
                userId: userId,
                worldId: worldId,
                postId: postId,
                authorActorId: authorActorId,
                authorDisplayName: authorDisplayName,
                authorHandle: authorHandle,
                authorActorType: authorActorType,
                content: content,
                createdAtUtc: createdAtUtc,
                parentPostId: parentPostId,
                likeCount: likeCount,
                replyCount: replyCount,
                visibility: visibility,
                localState: localState,
                clientPostId: clientPostId,
                idempotencyKey: idempotencyKey,
                failureMessage: failureMessage,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map((e) => (e.readTable(table), BaseReferences(db, table, e)))
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$CachedFeedPostsTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $CachedFeedPostsTable,
      CachedFeedPost,
      $$CachedFeedPostsTableFilterComposer,
      $$CachedFeedPostsTableOrderingComposer,
      $$CachedFeedPostsTableAnnotationComposer,
      $$CachedFeedPostsTableCreateCompanionBuilder,
      $$CachedFeedPostsTableUpdateCompanionBuilder,
      (
        CachedFeedPost,
        BaseReferences<_$AppDatabase, $CachedFeedPostsTable, CachedFeedPost>,
      ),
      CachedFeedPost,
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
  $$CachedFeedMetadataTableTableManager get cachedFeedMetadata =>
      $$CachedFeedMetadataTableTableManager(_db, _db.cachedFeedMetadata);
  $$CachedFeedPostsTableTableManager get cachedFeedPosts =>
      $$CachedFeedPostsTableTableManager(_db, _db.cachedFeedPosts);
}
