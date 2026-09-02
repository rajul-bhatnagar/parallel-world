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

abstract class _$AppDatabase extends GeneratedDatabase {
  _$AppDatabase(QueryExecutor e) : super(e);
  $AppDatabaseManager get managers => $AppDatabaseManager(this);
  late final $CachedWorldsTable cachedWorlds = $CachedWorldsTable(this);
  @override
  Iterable<TableInfo<Table, Object?>> get allTables =>
      allSchemaEntities.whereType<TableInfo<Table, Object?>>();
  @override
  List<DatabaseSchemaEntity> get allSchemaEntities => [cachedWorlds];
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

class $AppDatabaseManager {
  final _$AppDatabase _db;
  $AppDatabaseManager(this._db);
  $$CachedWorldsTableTableManager get cachedWorlds =>
      $$CachedWorldsTableTableManager(_db, _db.cachedWorlds);
}
