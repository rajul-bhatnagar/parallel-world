import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/app/router.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';

class ParallelWorldApp extends ConsumerStatefulWidget {
  const ParallelWorldApp({super.key});

  @override
  ConsumerState<ParallelWorldApp> createState() => _ParallelWorldAppState();
}

class _ParallelWorldAppState extends ConsumerState<ParallelWorldApp> {
  @override
  void initState() {
    super.initState();
    Future<void>.microtask(
      () => ref.read(sessionControllerProvider.notifier).initialize(),
    );
  }

  @override
  Widget build(BuildContext context) => MaterialApp.router(
    title: 'Parallel World',
    debugShowCheckedModeBanner: false,
    theme: buildAppTheme(),
    routerConfig: ref.watch(appRouterProvider),
  );
}

class ConfigurationFailureApp extends StatelessWidget {
  const ConfigurationFailureApp({required this.message, super.key});

  final String message;

  @override
  Widget build(BuildContext context) => MaterialApp(
    debugShowCheckedModeBanner: false,
    theme: buildAppTheme(),
    home: Scaffold(
      body: SafeArea(
        child: Center(
          child: Padding(
            padding: const EdgeInsets.all(AppSpacing.large),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.settings_outlined, size: 48),
                const SizedBox(height: AppSpacing.medium),
                Text(
                  'App configuration is incomplete',
                  style: Theme.of(context).textTheme.headlineSmall,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: AppSpacing.small),
                Text(message, textAlign: TextAlign.center),
              ],
            ),
          ),
        ),
      ),
    ),
  );
}
