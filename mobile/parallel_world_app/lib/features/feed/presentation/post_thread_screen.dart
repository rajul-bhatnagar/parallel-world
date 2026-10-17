import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:parallel_world_app/app/theme.dart';
import 'package:parallel_world_app/features/feed/application/post_thread_controller.dart';
import 'package:parallel_world_app/features/feed/domain/feed_models.dart';
import 'package:parallel_world_app/features/session/application/session_controller.dart';

class PostThreadScreen extends ConsumerStatefulWidget {
  const PostThreadScreen({required this.postId, super.key});

  final String postId;

  @override
  ConsumerState<PostThreadScreen> createState() => _PostThreadScreenState();
}

class _PostThreadScreenState extends ConsumerState<PostThreadScreen> {
  @override
  void initState() {
    super.initState();
    Future<void>.microtask(
      () =>
          ref.read(postThreadControllerProvider(widget.postId).notifier).load(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final provider = postThreadControllerProvider(widget.postId);
    final state = ref.watch(provider);
    return Scaffold(
      appBar: AppBar(title: const Text('Replies')),
      floatingActionButton: state.root == null
          ? null
          : FloatingActionButton.extended(
              onPressed: state.isSubmitting
                  ? null
                  : () => _showReplyComposer(context),
              icon: const Icon(Icons.reply),
              label: const Text('Reply'),
            ),
      body: SafeArea(
        child: _ThreadBody(postId: widget.postId, state: state),
      ),
    );
  }

  Future<void> _showReplyComposer(BuildContext context) async {
    final content = await showModalBottomSheet<String>(
      context: context,
      isScrollControlled: true,
      builder: (_) => const _ReplyComposer(),
    );
    if (content != null && mounted) {
      await ref
          .read(postThreadControllerProvider(widget.postId).notifier)
          .submitReply(content);
    }
  }
}

class _ThreadBody extends ConsumerWidget {
  const _ThreadBody({required this.postId, required this.state});

  final String postId;
  final PostThreadState state;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final controller = ref.read(postThreadControllerProvider(postId).notifier);
    if (state.isLoading) {
      return const Center(
        child: CircularProgressIndicator(semanticsLabel: 'Loading replies'),
      );
    }
    if (state.root == null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.large),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(state.message ?? 'This thread is unavailable.'),
              const SizedBox(height: AppSpacing.medium),
              FilledButton(
                onPressed: controller.load,
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      );
    }
    return ListView(
      padding: const EdgeInsets.fromLTRB(
        AppSpacing.medium,
        AppSpacing.medium,
        AppSpacing.medium,
        96,
      ),
      children: [
        _ThreadPostCard(postId: postId, post: state.root!, isRoot: true),
        const Divider(height: AppSpacing.large),
        if (state.replies.isEmpty)
          const Padding(
            padding: EdgeInsets.symmetric(vertical: AppSpacing.large),
            child: Text('No replies yet.', textAlign: TextAlign.center),
          )
        else
          ...state.replies.map(
            (reply) => _ThreadPostCard(postId: postId, post: reply),
          ),
        if (state.hasMore)
          OutlinedButton(
            onPressed: state.isLoadingNextPage ? null : controller.loadNextPage,
            child: state.isLoadingNextPage
                ? const SizedBox.square(
                    dimension: 20,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Text('Load more replies'),
          ),
        if (state.message != null)
          Text(state.message!, textAlign: TextAlign.center),
      ],
    );
  }
}

class _ThreadPostCard extends ConsumerWidget {
  const _ThreadPostCard({
    required this.postId,
    required this.post,
    this.isRoot = false,
  });

  final String postId;
  final FeedPost post;
  final bool isRoot;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(postThreadControllerProvider(postId));
    final controller = ref.read(postThreadControllerProvider(postId).notifier);
    final playerActorId = ref.watch(
      sessionControllerProvider.select(
        (session) => session.world?.playerActorId,
      ),
    );
    return Card(
      color: isRoot
          ? Theme.of(context).colorScheme.surfaceContainerHighest
          : null,
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.medium),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    '${post.author.displayName}  @${post.author.handle}',
                    style: const TextStyle(fontWeight: FontWeight.w600),
                  ),
                ),
                if (post.author.actorType == 'character')
                  TextButton(
                    onPressed:
                        state.pendingFollowActorIds.contains(
                          post.author.actorId,
                        )
                        ? null
                        : () => controller.toggleFollow(post.author.actorId),
                    child: Text(
                      post.author.isFollowed ? 'Following' : 'Follow',
                    ),
                  ),
              ],
            ),
            const SizedBox(height: AppSpacing.small),
            Text(post.content),
            if (post.localState == FeedPostLocalState.pending)
              const Text('Sending…')
            else if (post.localState == FeedPostLocalState.failed) ...[
              Text(
                post.failureMessage ?? 'This reply could not be sent.',
                style: TextStyle(color: Theme.of(context).colorScheme.error),
              ),
              TextButton.icon(
                onPressed: post.clientPostId == null
                    ? null
                    : () => controller.retryReply(post.clientPostId!),
                icon: const Icon(Icons.refresh),
                label: const Text('Retry reply'),
              ),
            ] else
              Row(
                children: [
                  IconButton(
                    tooltip: post.currentPlayerReaction == 'like'
                        ? 'Unlike'
                        : 'Like',
                    onPressed:
                        state.pendingReactionPostIds.contains(post.id) ||
                            post.author.actorId == playerActorId
                        ? null
                        : () => controller.toggleLike(post.id),
                    icon: Icon(
                      post.currentPlayerReaction == 'like'
                          ? Icons.favorite
                          : Icons.favorite_border,
                    ),
                  ),
                  Text('${post.counts.likes}'),
                  const SizedBox(width: AppSpacing.medium),
                  if (isRoot) ...[
                    const Icon(Icons.chat_bubble_outline, size: 18),
                    const SizedBox(width: AppSpacing.small),
                    Text('${post.counts.replies}'),
                  ] else
                    TextButton.icon(
                      onPressed: () => context.push('/posts/${post.id}'),
                      icon: const Icon(Icons.reply, size: 18),
                      label: Text('${post.counts.replies} Replies'),
                    ),
                ],
              ),
          ],
        ),
      ),
    );
  }
}

class _ReplyComposer extends StatefulWidget {
  const _ReplyComposer();

  @override
  State<_ReplyComposer> createState() => _ReplyComposerState();
}

class _ReplyComposerState extends State<_ReplyComposer> {
  final _controller = TextEditingController();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final valid =
        _controller.text.trim().isNotEmpty &&
        _controller.text.characters.length <= 500;
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
          Text('Write a reply', style: Theme.of(context).textTheme.titleLarge),
          const SizedBox(height: AppSpacing.medium),
          TextField(
            controller: _controller,
            autofocus: true,
            minLines: 3,
            maxLines: 6,
            maxLength: 500,
            decoration: const InputDecoration(
              labelText: 'Reply',
              border: OutlineInputBorder(),
            ),
            onChanged: (_) => setState(() {}),
          ),
          FilledButton(
            onPressed: valid
                ? () => Navigator.of(context).pop(_controller.text)
                : null,
            child: const Text('Reply'),
          ),
        ],
      ),
    );
  }
}
