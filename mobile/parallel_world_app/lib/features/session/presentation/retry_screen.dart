import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_state.dart';

class RetryScreen extends ConsumerWidget {
  const RetryScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final session = ref.watch(sessionControllerProvider);
    final isExpired = session.phase == SessionPhase.sessionExpired;
    final needsNewGuest =
        isExpired || session.phase == SessionPhase.bootstrapRecoveryExhausted;
    final title = session.isFirstLaunchOffline
        ? 'Connect to create your private world'
        : needsNewGuest
        ? 'Session recovery needed'
        : 'Parallel World is unavailable';
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(AppSpacing.large),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.cloud_off_outlined, size: 48),
                const SizedBox(height: AppSpacing.medium),
                Text(
                  title,
                  style: Theme.of(context).textTheme.headlineSmall,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: AppSpacing.small),
                Text(
                  session.message ?? 'Please retry.',
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: AppSpacing.large),
                FilledButton.icon(
                  onPressed: session.isBusy
                      ? null
                      : () => needsNewGuest
                            ? ref
                                  .read(sessionControllerProvider.notifier)
                                  .startNewGuestSession()
                            : ref
                                  .read(sessionControllerProvider.notifier)
                                  .retry(),
                  icon: const Icon(Icons.refresh),
                  label: Text(needsNewGuest ? 'Start new session' : 'Retry'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
