import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';
import 'package:parallel_world_app/features/session/application/session_contracts.dart';

class AuthScreen extends ConsumerStatefulWidget {
  const AuthScreen({super.key});
  @override
  ConsumerState<AuthScreen> createState() => _AuthScreenState();
}

class _AuthScreenState extends ConsumerState<AuthScreen> {
  final email = TextEditingController(),
      password = TextEditingController(),
      currentPassword = TextEditingController(),
      code = TextEditingController();
  String? message;
  List<AccountSession> sessions = const [];
  bool busy = false;
  @override
  void dispose() {
    email.dispose();
    password.dispose();
    currentPassword.dispose();
    code.dispose();
    super.dispose();
  }

  Future<void> run(Future<String?> Function() action) async {
    setState(() => busy = true);
    try {
      final value = await action();
      if (!mounted) return;
      setState(
        () => message = value == null
            ? 'Completed.'
            : 'Save this recovery code now: $value',
      );
    } catch (_) {
      if (mounted) {
        setState(() => message = 'The request could not be completed.');
      }
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Account')),
    body: ListView(
      padding: const EdgeInsets.all(24),
      children: [
        TextField(
          controller: email,
          keyboardType: TextInputType.emailAddress,
          autocorrect: false,
          decoration: const InputDecoration(labelText: 'Email'),
        ),
        TextField(
          controller: password,
          obscureText: true,
          enableSuggestions: false,
          decoration: const InputDecoration(labelText: 'Password'),
        ),
        TextField(
          controller: currentPassword,
          obscureText: true,
          enableSuggestions: false,
          decoration: const InputDecoration(
            labelText: 'Current password (for password change)',
          ),
        ),
        TextField(
          controller: code,
          autocorrect: false,
          decoration: const InputDecoration(
            labelText: 'Recovery code (for recovery only)',
          ),
        ),
        const SizedBox(height: 16),
        FilledButton(
          onPressed: busy
              ? null
              : () => run(() async {
                  await ref
                      .read(sessionControllerProvider.notifier)
                      .login(email.text, password.text);
                  if (context.mounted) context.go('/home');
                  return null;
                }),
          child: const Text('Log in'),
        ),
        OutlinedButton(
          onPressed: busy
              ? null
              : () => run(
                  () => ref
                      .read(sessionControllerProvider.notifier)
                      .register(email.text, password.text),
                ),
          child: const Text('Create new account'),
        ),
        OutlinedButton(
          onPressed: busy
              ? null
              : () => run(
                  () => ref
                      .read(sessionControllerProvider.notifier)
                      .upgrade(email.text, password.text),
                ),
          child: const Text('Upgrade this guest'),
        ),
        TextButton(
          onPressed: busy
              ? null
              : () => run(
                  () => ref
                      .read(sessionControllerProvider.notifier)
                      .recover(email.text, code.text, password.text),
                ),
          child: const Text('Recover and set password'),
        ),
        const Divider(),
        OutlinedButton(
          onPressed: busy
              ? null
              : () => run(() async {
                  await ref
                      .read(sessionControllerProvider.notifier)
                      .changePassword(currentPassword.text, password.text);
                  return null;
                }),
          child: const Text('Change password and sign out everywhere'),
        ),
        OutlinedButton(
          onPressed: busy
              ? null
              : () => run(
                  () => ref
                      .read(sessionControllerProvider.notifier)
                      .regenerateRecoveryCode(),
                ),
          child: const Text('Generate new recovery code'),
        ),
        OutlinedButton(
          onPressed: busy
              ? null
              : () => run(() async {
                  final value = await ref
                      .read(sessionControllerProvider.notifier)
                      .listSessions();
                  if (mounted) setState(() => sessions = value);
                  return null;
                }),
          child: const Text('Refresh active sessions'),
        ),
        for (final session in sessions)
          ListTile(
            title: Text('${session.platform} ${session.appVersion}'),
            subtitle: Text(
              session.isCurrent
                  ? 'Current session'
                  : 'Last seen ${session.lastSeenAt.toLocal()}',
            ),
            trailing: IconButton(
              tooltip: 'Revoke session',
              icon: const Icon(Icons.logout),
              onPressed: busy
                  ? null
                  : () => run(() async {
                      await ref
                          .read(sessionControllerProvider.notifier)
                          .revokeSession(session.id);
                      final value = await ref
                          .read(sessionControllerProvider.notifier)
                          .listSessions();
                      if (mounted) setState(() => sessions = value);
                      return null;
                    }),
            ),
          ),
        TextButton(
          onPressed: busy
              ? null
              : () => run(() async {
                  await ref
                      .read(sessionControllerProvider.notifier)
                      .logoutAll();
                  return null;
                }),
          child: const Text('Sign out all sessions'),
        ),
        if (message != null)
          SelectableText(message!, key: const Key('auth-result')),
      ],
    ),
  );
}
