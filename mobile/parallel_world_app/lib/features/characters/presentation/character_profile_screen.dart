import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/characters/application/character_details_provider.dart';
import 'package:parallel_world_app/features/characters/domain/character_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';

class CharacterProfileScreen extends ConsumerWidget {
  const CharacterProfileScreen({required this.characterId, super.key});

  final String characterId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final session = ref.watch(sessionControllerProvider);
    final userId = session.userId;
    final worldId = session.world?.id;
    if (userId == null || worldId == null) {
      return const Scaffold(body: Center(child: Text('Profile unavailable.')));
    }
    final request = CharacterDetailsRequest(
      userId: userId,
      worldId: worldId,
      characterId: characterId,
    );
    final details = ref.watch(characterDetailsProvider(request));
    return Scaffold(
      appBar: AppBar(title: const Text('Character profile')),
      body: SafeArea(
        child: details.when(
          loading: () => const Center(
            child: CircularProgressIndicator(
              semanticsLabel: 'Loading character profile',
            ),
          ),
          error: (error, _) => _ProfileError(
            message: error is AppFailure
                ? error.message
                : const UnknownFailure().message,
            onRetry: () => ref.invalidate(characterDetailsProvider(request)),
          ),
          data: (view) => _ProfileBody(view: view),
        ),
      ),
    );
  }
}

class _ProfileBody extends StatelessWidget {
  const _ProfileBody({required this.view});

  final CharacterDetailsView view;

  @override
  Widget build(BuildContext context) {
    final character = view.details;
    return ListView(
      padding: const EdgeInsets.all(AppSpacing.large),
      children: [
        if (view.isStale && !view.isOffline) const LinearProgressIndicator(),
        if (view.isStale && !view.isOffline)
          const SizedBox(height: AppSpacing.medium),
        if (view.isOffline)
          const Card(
            child: ListTile(
              leading: Icon(Icons.cloud_off_outlined),
              title: Text('Offline saved profile'),
              subtitle: Text('This profile may be out of date.'),
            ),
          ),
        if (view.isOffline) const SizedBox(height: AppSpacing.large),
        Semantics(
          header: true,
          child: Text(
            character.displayName,
            style: Theme.of(context).textTheme.headlineMedium,
          ),
        ),
        Text('@${character.handle}'),
        const SizedBox(height: AppSpacing.large),
        _Fact(label: 'Profession', value: character.profession),
        _Fact(label: 'Age', value: '${character.age}'),
        _Fact(label: 'Archetype', value: character.archetype),
        _Fact(label: 'Mood', value: _label(character.visibleMood)),
        const SizedBox(height: AppSpacing.medium),
        Text(character.bio),
        const SizedBox(height: AppSpacing.large),
        Text('Interests', style: Theme.of(context).textTheme.titleLarge),
        const SizedBox(height: AppSpacing.small),
        Wrap(
          spacing: AppSpacing.small,
          runSpacing: AppSpacing.small,
          children: character.interests
              .map((interest) => Chip(label: Text(_label(interest.topicId))))
              .toList(growable: false),
        ),
        const SizedBox(height: AppSpacing.large),
        Text('Schedule', style: Theme.of(context).textTheme.titleLarge),
        const SizedBox(height: AppSpacing.small),
        if (character.schedule.isEmpty)
          const Text('No public schedule is available.')
        else
          ...character.schedule.map(_ScheduleTile.new),
      ],
    );
  }
}

class _Fact extends StatelessWidget {
  const _Fact({required this.label, required this.value});
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: AppSpacing.small),
    child: Text('$label: $value'),
  );
}

class _ScheduleTile extends StatelessWidget {
  const _ScheduleTile(this.item);
  final CharacterSchedule item;

  @override
  Widget build(BuildContext context) => ListTile(
    contentPadding: EdgeInsets.zero,
    leading: const Icon(Icons.schedule_outlined),
    title: Text(item.activity),
    subtitle: Text(
      '${_day(item.dayOfWeek)}, ${item.startLocalTime}–${item.endLocalTime}',
    ),
  );
}

class _ProfileError extends StatelessWidget {
  const _ProfileError({required this.message, required this.onRetry});
  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) => Center(
    child: Padding(
      padding: const EdgeInsets.all(AppSpacing.large),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(message, textAlign: TextAlign.center),
          const SizedBox(height: AppSpacing.medium),
          FilledButton(onPressed: onRetry, child: const Text('Retry')),
        ],
      ),
    ),
  );
}

String _day(int value) => const [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
][value.clamp(0, 6)];

String _label(String value) => value.isEmpty
    ? value
    : '${value[0].toUpperCase()}${value.substring(1).replaceAll('-', ' ')}';
