import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final session = ref.watch(sessionControllerProvider);
    final world = session.world;
    final isOffline = session.phase == SessionPhase.offlineAuthenticated;
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
            const Card(
              child: Padding(
                padding: EdgeInsets.all(AppSpacing.large),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('World ready', style: TextStyle(fontSize: 20)),
                    SizedBox(height: AppSpacing.small),
                    Text(
                      'Character and feed features arrive in later milestones.',
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
