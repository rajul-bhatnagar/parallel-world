import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/features/catch_up/application/catch_up_provider.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final session = ref.watch(sessionControllerProvider);
    final world = session.world;
    final catchUpWorldId = world?.id;
    final isOffline = session.phase == SessionPhase.offlineAuthenticated;
    final catchUp = catchUpWorldId == null || isOffline
        ? null
        : ref.watch(catchUpProvider(catchUpWorldId));
    return Scaffold(
      appBar: AppBar(
        title: const Text('Parallel World'),
        actions: [
          IconButton(
            tooltip: 'Sign out',
            onPressed: session.isBusy
                ? null
                : () => ref.read(sessionControllerProvider.notifier).logout(),
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.all(AppSpacing.large),
          children: [
            if (isOffline)
              Card(
                child: ListTile(
                  leading: const Icon(Icons.cloud_off_outlined),
                  title: const Text('Offline cached view'),
                  subtitle: Text(
                    session.message ??
                        'Server data will refresh when available.',
                  ),
                ),
              ),
            if (isOffline) const SizedBox(height: AppSpacing.medium),
            Text(
              world?.name ?? 'Private world',
              style: Theme.of(context).textTheme.headlineMedium,
            ),
            const SizedBox(height: AppSpacing.small),
            Text(
              world == null
                  ? 'World details are unavailable.'
                  : 'Welcome, ${world.playerDisplayName}.',
            ),
            const SizedBox(height: AppSpacing.large),
            if (catchUp != null) ...[
              catchUp.when(
                loading: () => const Card(
                  child: ListTile(
                    leading: CircularProgressIndicator(),
                    title: Text('Catching up your world…'),
                  ),
                ),
                error: (error, stackTrace) => Card(
                  child: ListTile(
                    leading: const Icon(Icons.sync_problem_outlined),
                    title: const Text('World catch-up needs another try'),
                    trailing: IconButton(
                      tooltip: 'Retry catch-up',
                      onPressed: () =>
                          ref.invalidate(catchUpProvider(catchUpWorldId!)),
                      icon: const Icon(Icons.refresh),
                    ),
                  ),
                ),
                data: (view) {
                  final result = view.result;
                  if (view.isOffline || result == null) {
                    return const SizedBox.shrink();
                  }
                  final summary = view.summary;
                  if (!result.isPartial && summary == null) {
                    return const SizedBox.shrink();
                  }
                  return Card(
                    child: Padding(
                      padding: const EdgeInsets.all(AppSpacing.large),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            result.isPartial
                                ? 'World catch-up in progress'
                                : 'While you were away',
                            style: const TextStyle(fontSize: 20),
                          ),
                          const SizedBox(height: AppSpacing.small),
                          if (summary != null) Text(summary.text),
                          for (final item in summary?.items ?? const [])
                            Padding(
                              padding: const EdgeInsets.only(
                                top: AppSpacing.small,
                              ),
                              child: Text('• ${item.wording}'),
                            ),
                          if (summary?.items.any(
                                (item) => item.itemType == 'follow',
                              ) ??
                              false)
                            TextButton.icon(
                              onPressed: () => context.push('/characters'),
                              icon: const Icon(Icons.people_outline),
                              label: const Text('View characters'),
                            ),
                          if (result.isPartial) ...[
                            const SizedBox(height: AppSpacing.small),
                            Text(
                              '${result.remainingIntervals} intervals remain.',
                            ),
                            TextButton.icon(
                              onPressed: () => ref.invalidate(
                                catchUpProvider(catchUpWorldId!),
                              ),
                              icon: const Icon(Icons.refresh),
                              label: const Text('Continue catch-up'),
                            ),
                          ],
                        ],
                      ),
                    ),
                  );
                },
              ),
              const SizedBox(height: AppSpacing.medium),
            ],
            Card(
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.large),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Meet the characters',
                      style: TextStyle(fontSize: 20),
                    ),
                    const SizedBox(height: AppSpacing.small),
                    const Text('Browse the people who live in this world.'),
                    const SizedBox(height: AppSpacing.medium),
                    FilledButton.icon(
                      onPressed: () => context.push('/characters'),
                      icon: const Icon(Icons.people_outline),
                      label: const Text('View characters'),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: AppSpacing.medium),
            Card(
              child: ListTile(
                leading: const Icon(Icons.chat_bubble_outline),
                title: const Text('Messages'),
                subtitle: const Text(
                  'Continue private character conversations.',
                ),
                trailing: const Icon(Icons.chevron_right),
                onTap: () => context.push('/conversations'),
              ),
            ),
            const SizedBox(height: AppSpacing.medium),
            Card(
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.large),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('World feed', style: TextStyle(fontSize: 20)),
                    const SizedBox(height: AppSpacing.small),
                    const Text('See what is happening and share a post.'),
                    const SizedBox(height: AppSpacing.medium),
                    FilledButton.icon(
                      onPressed: () => context.push('/feed'),
                      icon: const Icon(Icons.dynamic_feed_outlined),
                      label: const Text('Open feed'),
                    ),
                  ],
                ),
              ),
            ),
            if (session.message case final message?) ...[
              const SizedBox(height: AppSpacing.medium),
              Text(
                message,
                style: TextStyle(color: Theme.of(context).colorScheme.error),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
