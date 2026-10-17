import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';

class WorldCreateScreen extends ConsumerStatefulWidget {
  const WorldCreateScreen({super.key});

  @override
  ConsumerState<WorldCreateScreen> createState() => _WorldCreateScreenState();
}

class _WorldCreateScreenState extends ConsumerState<WorldCreateScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController(text: 'My Parallel World');

  @override
  void dispose() {
    _nameController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final session = ref.watch(sessionControllerProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Create world')),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(AppSpacing.large),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 480),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      'Name your private world',
                      style: Theme.of(context).textTheme.headlineSmall,
                    ),
                    const SizedBox(height: AppSpacing.small),
                    const Text(
                      'Only you can access this world. The server remains the source of truth.',
                    ),
                    const SizedBox(height: AppSpacing.large),
                    TextFormField(
                      controller: _nameController,
                      maxLength: 80,
                      textInputAction: TextInputAction.done,
                      decoration: const InputDecoration(
                        labelText: 'World name',
                      ),
                      validator: (value) {
                        final length = value?.trim().length ?? 0;
                        return length == 0 || length > 80
                            ? 'Enter a name up to 80 characters.'
                            : null;
                      },
                      onFieldSubmitted: (_) => _submit(),
                    ),
                    if (session.message case final message?) ...[
                      const SizedBox(height: AppSpacing.small),
                      Text(
                        message,
                        style: TextStyle(
                          color: Theme.of(context).colorScheme.error,
                        ),
                      ),
                    ],
                    const SizedBox(height: AppSpacing.medium),
                    FilledButton(
                      onPressed: session.isBusy ? null : _submit,
                      child: Text(
                        session.isBusy ? 'Creating…' : 'Create world',
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  void _submit() {
    if (_formKey.currentState?.validate() != true) {
      return;
    }
    ref
        .read(sessionControllerProvider.notifier)
        .createWorld(_nameController.text);
  }
}
