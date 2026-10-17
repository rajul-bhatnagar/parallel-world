import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/characters/application/character_contracts.dart';
import 'package:parallel_world_app/features/characters/application/character_dependencies.dart';
import 'package:parallel_world_app/features/characters/domain/character_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';

final characterCatalogueProvider =
    NotifierProvider<CharacterCatalogueController, CharacterCatalogueState>(
      CharacterCatalogueController.new,
    );

class CharacterCatalogueState {
  const CharacterCatalogueState({
    this.items = const [],
    this.nextCursor,
    this.hasMore = false,
    this.isLoading = false,
    this.isRefreshing = false,
    this.isLoadingNextPage = false,
    this.isOffline = false,
    this.message,
  });

  final List<CharacterSummary> items;
  final String? nextCursor;
  final bool hasMore;
  final bool isLoading;
  final bool isRefreshing;
  final bool isLoadingNextPage;
  final bool isOffline;
  final String? message;

  CharacterCatalogueState copyWith({
    List<CharacterSummary>? items,
    String? nextCursor,
    bool? hasMore,
    bool? isLoading,
    bool? isRefreshing,
    bool? isLoadingNextPage,
    bool? isOffline,
    String? message,
    bool clearCursor = false,
    bool clearMessage = false,
  }) => CharacterCatalogueState(
    items: items ?? this.items,
    nextCursor: clearCursor ? null : nextCursor ?? this.nextCursor,
    hasMore: hasMore ?? this.hasMore,
    isLoading: isLoading ?? this.isLoading,
    isRefreshing: isRefreshing ?? this.isRefreshing,
    isLoadingNextPage: isLoadingNextPage ?? this.isLoadingNextPage,
    isOffline: isOffline ?? this.isOffline,
    message: clearMessage ? null : message ?? this.message,
  );
}

class CharacterCatalogueController extends Notifier<CharacterCatalogueState> {
  Future<void>? _activeLoad;

  @override
  CharacterCatalogueState build() => const CharacterCatalogueState();

  Future<void> load({bool refresh = false}) {
    if (_activeLoad case final active?) {
      return active;
    }
    final load = _load(refresh: refresh);
    _activeLoad = load;
    return load.whenComplete(() {
      if (identical(_activeLoad, load)) {
        _activeLoad = null;
      }
    });
  }

  Future<void> loadNextPage() async {
    if (!state.hasMore || state.isLoadingNextPage) {
      return;
    }
    final scope = _scope();
    if (scope == null) {
      return;
    }
    state = state.copyWith(isLoadingNextPage: true, clearMessage: true);
    try {
      final page = await ref
          .read(characterRepositoryProvider)
          .fetchCatalogue(
            userId: scope.userId,
            worldId: scope.worldId,
            cursor: state.nextCursor,
          );
      final knownIds = state.items.map((item) => item.id).toSet();
      state = state.copyWith(
        items: [
          ...state.items,
          ...page.items.where((item) => knownIds.add(item.id)),
        ],
        nextCursor: page.nextCursor,
        hasMore: page.hasMore,
        isLoadingNextPage: false,
        isOffline: false,
      );
    } on AppFailure catch (failure) {
      state = state.copyWith(
        isLoadingNextPage: false,
        message: failure.message,
        isOffline: failure is NetworkFailure,
      );
    } on FormatException {
      state = state.copyWith(
        isLoadingNextPage: false,
        message: const UnknownFailure().message,
      );
    }
  }

  Future<void> _load({required bool refresh}) async {
    final scope = _scope();
    if (scope == null) {
      state = const CharacterCatalogueState(
        message: 'A private world is required to view characters.',
      );
      return;
    }
    final repository = ref.read(characterRepositoryProvider);
    if (!refresh) {
      CachedCharacterCatalogue? cached;
      try {
        cached = await repository.readCachedCatalogue(
          scope.userId,
          scope.worldId,
        );
      } on FormatException {
        cached = null;
      } catch (_) {
        cached = null;
      }
      if (cached != null) {
        state = CharacterCatalogueState(
          items: cached.items,
          isRefreshing: true,
        );
      } else {
        state = const CharacterCatalogueState(isLoading: true);
      }
    } else {
      state = state.copyWith(
        isRefreshing: state.items.isNotEmpty,
        isLoading: state.items.isEmpty,
        clearMessage: true,
      );
    }

    try {
      final page = await repository.fetchCatalogue(
        userId: scope.userId,
        worldId: scope.worldId,
      );
      state = CharacterCatalogueState(
        items: page.items,
        nextCursor: page.nextCursor,
        hasMore: page.hasMore,
      );
    } on NetworkFailure catch (failure) {
      if (state.items.isNotEmpty || !state.isLoading) {
        state = state.copyWith(
          isLoading: false,
          isRefreshing: false,
          isOffline: true,
          message: 'Offline — showing saved character profiles.',
        );
      } else {
        state = CharacterCatalogueState(message: failure.message);
      }
    } on AppFailure catch (failure) {
      state = state.copyWith(
        isLoading: false,
        isRefreshing: false,
        message: failure.message,
      );
    } on FormatException {
      state = state.copyWith(
        isLoading: false,
        isRefreshing: false,
        message: const UnknownFailure().message,
      );
    }
  }

  _CharacterScope? _scope() {
    final session = ref.read(sessionControllerProvider);
    final userId = session.userId;
    final worldId = session.world?.id;
    return userId == null || worldId == null
        ? null
        : _CharacterScope(userId, worldId);
  }
}

class _CharacterScope {
  const _CharacterScope(this.userId, this.worldId);
  final String userId;
  final String worldId;
}
