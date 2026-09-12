import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/features/characters/application/character_catalogue_controller.dart';
import 'package:parallel_world_app/features/characters/domain/character_models.dart';

class CharacterCatalogueScreen extends ConsumerStatefulWidget {
  const CharacterCatalogueScreen({super.key});

  @override
  ConsumerState<CharacterCatalogueScreen> createState() =>
      _CharacterCatalogueScreenState();
}

class _CharacterCatalogueScreenState
    extends ConsumerState<CharacterCatalogueScreen> {
  @override
  void initState() {
    super.initState();
    Future<void>.microtask(
      () => ref.read(characterCatalogueProvider.notifier).load(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(characterCatalogueProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Characters')),
      body: SafeArea(
        child: RefreshIndicator(
          onRefresh: () =>
              ref.read(characterCatalogueProvider.notifier).load(refresh: true),
          child: _CatalogueBody(state: state),
        ),
      ),
    );
  }
}

class _CatalogueBody extends ConsumerWidget {
  const _CatalogueBody({required this.state});

  final CharacterCatalogueState state;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    if (state.isLoading) {
      return const _ScrollableMessage(
        child: CircularProgressIndicator(semanticsLabel: 'Loading characters'),
      );
    }
    if (state.items.isEmpty && state.message != null && !state.isOffline) {
      return _ScrollableMessage(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 40),
            const SizedBox(height: AppSpacing.small),
            Text(state.message!, textAlign: TextAlign.center),
            const SizedBox(height: AppSpacing.medium),
            FilledButton(
              onPressed: () => ref
                  .read(characterCatalogueProvider.notifier)
                  .load(refresh: true),
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }

    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(AppSpacing.medium),
      children: [
        if (state.isOffline)
          const Card(
            child: ListTile(
              leading: Icon(Icons.cloud_off_outlined),
              title: Text('Offline saved profiles'),
              subtitle: Text('Details may be out of date.'),
            ),
          ),
        if (state.isOffline) const SizedBox(height: AppSpacing.medium),
        if (state.isRefreshing) const LinearProgressIndicator(),
        if (state.isRefreshing) const SizedBox(height: AppSpacing.medium),
        if (state.items.isEmpty)
          const Padding(
            padding: EdgeInsets.symmetric(vertical: AppSpacing.large),
            child: Text(
              'No characters are available in this world yet.',
              textAlign: TextAlign.center,
            ),
          )
        else
          ...state.items.map((character) => _CharacterCard(character)),
        if (state.hasMore)
          Padding(
            padding: const EdgeInsets.only(top: AppSpacing.small),
            child: OutlinedButton(
              onPressed: state.isLoadingNextPage
                  ? null
                  : () => ref
                        .read(characterCatalogueProvider.notifier)
                        .loadNextPage(),
              child: state.isLoadingNextPage
                  ? const SizedBox.square(
                      dimension: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Load more'),
            ),
          ),
        if (state.message != null && state.items.isNotEmpty)
          Padding(
            padding: const EdgeInsets.only(top: AppSpacing.medium),
            child: Text(state.message!, textAlign: TextAlign.center),
          ),
      ],
    );
  }
}

class _CharacterCard extends StatelessWidget {
  const _CharacterCard(this.character);

  final CharacterSummary character;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: AppSpacing.small),
    child: Card(
      child: ListTile(
        leading: CircleAvatar(
          child: Text(character.displayName.characters.first.toUpperCase()),
        ),
        title: Text(character.displayName),
        subtitle: Text(
          '@${character.handle} · ${character.profession}\nMood: ${_label(character.visibleMood)}',
        ),
        isThreeLine: true,
        trailing: const Icon(Icons.chevron_right),
        onTap: () => context.push('/characters/${character.id}'),
      ),
    ),
  );
}

class _ScrollableMessage extends StatelessWidget {
  const _ScrollableMessage({required this.child});
  final Widget child;

  @override
  Widget build(BuildContext context) => ListView(
    physics: const AlwaysScrollableScrollPhysics(),
    padding: const EdgeInsets.all(AppSpacing.large),
    children: [
      SizedBox(height: MediaQuery.sizeOf(context).height * 0.25),
      Center(child: child),
    ],
  );
}

String _label(String value) => value.isEmpty
    ? value
    : '${value[0].toUpperCase()}${value.substring(1).replaceAll('-', ' ')}';
