import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/features/feed/application/feed_controller.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';

class FeedScreen extends ConsumerStatefulWidget {
  const FeedScreen({super.key});

  @override
  ConsumerState<FeedScreen> createState() => _FeedScreenState();
}

class _FeedScreenState extends ConsumerState<FeedScreen> {
  @override
  void initState() {
    super.initState();
    Future<void>.microtask(
      () => ref.read(feedControllerProvider.notifier).load(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(feedControllerProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Feed')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: state.isSubmitting ? null : () => _showComposer(context),
        icon: const Icon(Icons.edit_outlined),
        label: const Text('Post'),
      ),
      body: SafeArea(
        child: RefreshIndicator(
          onRefresh: () =>
              ref.read(feedControllerProvider.notifier).load(refresh: true),
          child: _FeedBody(state: state),
        ),
      ),
    );
  }

  Future<void> _showComposer(BuildContext context) async {
    final content = await showModalBottomSheet<String>(
      context: context,
      isScrollControlled: true,
      builder: (_) => const _PostComposer(),
    );
    if (content != null && mounted) {
      await ref.read(feedControllerProvider.notifier).submit(content);
    }
  }
}

class _PostComposer extends StatefulWidget {
  const _PostComposer();

  @override
  State<_PostComposer> createState() => _PostComposerState();
}

class _PostComposerState extends State<_PostComposer> {
  final _controller = TextEditingController();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final length = _controller.text.characters.length;
    final valid = _controller.text.trim().isNotEmpty && length <= 500;
    return SingleChildScrollView(
      padding: EdgeInsets.fromLTRB(
        AppSpacing.large,
        AppSpacing.large,
        AppSpacing.large,
        MediaQuery.viewInsetsOf(context).bottom + AppSpacing.large,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text('Create post', style: Theme.of(context).textTheme.titleLarge),
          const SizedBox(height: AppSpacing.medium),
          TextField(
            controller: _controller,
            autofocus: true,
            minLines: 3,
            maxLines: 6,
            maxLength: 500,
            decoration: const InputDecoration(
              labelText: 'What is happening?',
              border: OutlineInputBorder(),
            ),
            onChanged: (_) => setState(() {}),
          ),
          const SizedBox(height: AppSpacing.small),
          FilledButton(
            onPressed: valid
                ? () => Navigator.of(context).pop(_controller.text)
                : null,
            child: const Text('Publish'),
          ),
        ],
      ),
    );
  }
}

class _FeedBody extends ConsumerWidget {
  const _FeedBody({required this.state});

  final FeedState state;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    if (state.isLoading) {
      return const _ScrollableMessage(
        child: CircularProgressIndicator(semanticsLabel: 'Loading feed'),
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
              onPressed: () =>
                  ref.read(feedControllerProvider.notifier).load(refresh: true),
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }

    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.fromLTRB(
        AppSpacing.medium,
        AppSpacing.medium,
        AppSpacing.medium,
        96,
      ),
      children: [
        if (state.isOffline)
          const Card(
            child: ListTile(
              leading: Icon(Icons.cloud_off_outlined),
              title: Text('Offline saved feed'),
              subtitle: Text('Posts may be out of date.'),
            ),
          ),
        if (state.isOffline) const SizedBox(height: AppSpacing.medium),
        if (state.isRefreshing) const LinearProgressIndicator(),
        if (state.isRefreshing) const SizedBox(height: AppSpacing.medium),
        if (state.items.isEmpty)
          const Padding(
            padding: EdgeInsets.symmetric(vertical: AppSpacing.large),
            child: Text(
              'No posts are available in this world yet.',
              textAlign: TextAlign.center,
            ),
          )
        else
          ...state.items.map((post) => _PostCard(post: post)),
        if (state.hasMore)
          OutlinedButton(
            onPressed: state.isLoadingNextPage
                ? null
                : () =>
                      ref.read(feedControllerProvider.notifier).loadNextPage(),
            child: state.isLoadingNextPage
                ? const SizedBox.square(
                    dimension: 20,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Text('Load more'),
          ),
        if (state.message != null && state.items.isNotEmpty)
          Padding(
            padding: const EdgeInsets.only(top: AppSpacing.small),
            child: Text(state.message!, textAlign: TextAlign.center),
          ),
      ],
    );
  }
}

class _PostCard extends ConsumerWidget {
  const _PostCard({required this.post});

  final FeedPost post;

  @override
  Widget build(BuildContext context, WidgetRef ref) => Padding(
    padding: const EdgeInsets.only(bottom: AppSpacing.small),
    child: Card(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.medium),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                CircleAvatar(
                  child: Text(
                    post.author.displayName.characters.first.toUpperCase(),
                  ),
                ),
                const SizedBox(width: AppSpacing.small),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        post.author.displayName,
                        style: const TextStyle(fontWeight: FontWeight.w600),
                      ),
                      Text('@${post.author.handle}'),
                    ],
                  ),
                ),
                if (post.localState == FeedPostLocalState.pending)
                  const Text('Sending…')
                else if (post.localState == FeedPostLocalState.failed)
                  const Text('Not sent'),
              ],
            ),
            const SizedBox(height: AppSpacing.medium),
            Text(post.content),
            if (post.localState == FeedPostLocalState.failed) ...[
              const SizedBox(height: AppSpacing.small),
              Text(
                post.failureMessage ?? 'This post could not be sent.',
                style: TextStyle(color: Theme.of(context).colorScheme.error),
              ),
              TextButton.icon(
                onPressed: post.clientPostId == null
                    ? null
                    : () => ref
                          .read(feedControllerProvider.notifier)
                          .retry(post.clientPostId!),
                icon: const Icon(Icons.refresh),
                label: const Text('Retry post'),
              ),
            ],
          ],
        ),
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
