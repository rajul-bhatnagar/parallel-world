using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.Characters;
using ParallelWorld.Application.Common;
using ParallelWorld.Domain.Characters;
using ParallelWorld.Domain.Social;

namespace ParallelWorld.Application.Social;

public sealed class SocialFeedService(
    ISocialRepository repository,
    IFeedCursorCodec cursorCodec,
    IReplyCursorCodec replyCursorCodec,
    ICharacterCatalogueService characterCatalogueService,
    IUnitOfWork unitOfWork,
    IPersistenceFailureClassifier failureClassifier,
    TimeProvider timeProvider) : ISocialFeedService
{
    public const string CreatePostOperation = "social.create-post";

    public async Task<SocialResult<FeedPage>> GetFeedAsync(
        Guid userId,
        Guid worldId,
        int limit,
        string? cursor,
        CancellationToken cancellationToken)
    {
        if (limit is < 1 or > 50)
        {
            return Invalid<FeedPage>("Limit must be between 1 and 50.");
        }

        var decodedCursor = cursorCodec.Decode(cursor, worldId);
        if (!decodedCursor.IsValid)
        {
            return Invalid<FeedPage>("The feed cursor is invalid.", "invalid_cursor");
        }

        var initializationFailure = await EnsureSeedPostsAsync(userId, worldId, cancellationToken);
        if (initializationFailure is not null)
        {
            return SocialResult<FeedPage>.Fail(initializationFailure);
        }

        var player = await repository.FindPlayerAuthorAsync(userId, worldId, cancellationToken);
        if (player is null)
        {
            return SocialResult<FeedPage>.Fail(NotAvailable());
        }

        var rows = await repository.ListFeedAsync(
            worldId,
            player.ActorId,
            decodedCursor.Value,
            limit + 1,
            cancellationToken);
        var hasMore = rows.Count > limit;
        var items = rows.Take(limit).ToArray();
        var nextCursor = hasMore
            ? cursorCodec.Encode(worldId, new(items[^1].CreatedAtUtc, items[^1].Id))
            : null;
        return SocialResult<FeedPage>.Success(new(items, nextCursor, hasMore));
    }

    public async Task<SocialResult<CreatedPost>> CreatePostAsync(
        CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        var content = command.Content.Trim();
        if (string.IsNullOrWhiteSpace(content)
            || new StringInfo(content).LengthInTextElements > Post.MaximumContentCharacters)
        {
            return Invalid<CreatedPost>(
                $"Content is required and must not exceed {Post.MaximumContentCharacters} characters.");
        }

        var requestHash = HashRequest(command.WorldId, command.ClientPostId, content);
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var transaction = await unitOfWork.BeginTransactionAsync(
                    ApplicationIsolationLevel.Serializable,
                    cancellationToken);
                var replay = await repository.FindIdempotentPostAsync(
                    command.UserId,
                    CreatePostOperation,
                    command.IdempotencyKey,
                    cancellationToken);
                if (replay is not null)
                {
                    if (!CryptographicOperations.FixedTimeEquals(
                        Convert.FromHexString(replay.Value.RequestHash),
                        Convert.FromHexString(requestHash)))
                    {
                        return SocialResult<CreatedPost>.Fail(new(
                            "idempotency_key_reused",
                            409,
                            "The idempotency key was already used for a different request."));
                    }

                    return SocialResult<CreatedPost>.Success(new(replay.Value.Post, true));
                }

                var player = await repository.FindPlayerAuthorAsync(
                    command.UserId,
                    command.WorldId,
                    cancellationToken);
                if (player is null)
                {
                    return SocialResult<CreatedPost>.Fail(NotAvailable());
                }

                var now = timeProvider.GetUtcNow();
                var eventId = Guid.NewGuid();
                var postId = Guid.NewGuid();
                var gameplayEvent = new GameplayEvent(
                    eventId,
                    command.WorldId,
                    "postCreated",
                    player.ActorId,
                    null,
                    now,
                    0,
                    0,
                    "player_post",
                    player.RuleVersion,
                    $"m06:player-post:{command.UserId:N}:{command.IdempotencyKey}",
                    now);
                var post = new Post(
                    postId,
                    command.WorldId,
                    player.ActorId,
                    null,
                    eventId,
                    content,
                    now);
                var idempotency = new IdempotencyRecord(
                    Guid.NewGuid(),
                    command.UserId,
                    command.WorldId,
                    CreatePostOperation,
                    command.IdempotencyKey,
                    requestHash,
                    postId,
                    now);
                repository.AddPlayerPost(gameplayEvent, post, idempotency);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return SocialResult<CreatedPost>.Success(new(
                    new FeedPost(
                        post.Id,
                        post.WorldId,
                        new FeedAuthor(player.ActorId, player.DisplayName, player.Handle, "player", false),
                        post.Content,
                        post.CreatedAt,
                        post.ParentPostId,
                        new FeedCounts(post.LikeCount, post.ReplyCount),
                        null,
                        "world"),
                    false));
            }
            catch (Exception exception) when (failureClassifier.IsRetryableConcurrency(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
        }

        return SocialResult<CreatedPost>.Fail(new(
            "concurrency_conflict",
            409,
            "The post could not be created concurrently."));
    }

    public async Task<SocialResult<FeedPost>> GetPostAsync(
        Guid userId,
        Guid worldId,
        Guid postId,
        CancellationToken cancellationToken)
    {
        var initializationFailure = await EnsureSeedPostsAsync(userId, worldId, cancellationToken);
        if (initializationFailure is not null)
        {
            return SocialResult<FeedPost>.Fail(initializationFailure);
        }

        var player = await repository.FindPlayerAuthorAsync(userId, worldId, cancellationToken);
        if (player is null)
        {
            return SocialResult<FeedPost>.Fail(NotAvailable());
        }

        var post = await repository.FindPostAsync(worldId, postId, player.ActorId, cancellationToken);
        return post is null
            ? SocialResult<FeedPost>.Fail(NotAvailable())
            : SocialResult<FeedPost>.Success(post);
    }

    public async Task<SocialResult<FeedPage>> GetRepliesAsync(
        Guid userId,
        Guid worldId,
        Guid parentPostId,
        int limit,
        string? cursor,
        CancellationToken cancellationToken)
    {
        if (limit is < 1 or > 50)
        {
            return Invalid<FeedPage>("Limit must be between 1 and 50.");
        }

        var decodedCursor = replyCursorCodec.Decode(cursor, worldId, parentPostId);
        if (!decodedCursor.IsValid)
        {
            return Invalid<FeedPage>("The reply cursor is invalid.", "invalid_cursor");
        }

        var initializationFailure = await EnsureSeedPostsAsync(userId, worldId, cancellationToken);
        if (initializationFailure is not null)
        {
            return SocialResult<FeedPage>.Fail(initializationFailure);
        }

        var player = await repository.FindPlayerAuthorAsync(userId, worldId, cancellationToken);
        if (player is null
            || await repository.FindPostAsync(worldId, parentPostId, player.ActorId, cancellationToken) is null)
        {
            return SocialResult<FeedPage>.Fail(NotAvailable());
        }

        var rows = await repository.ListRepliesAsync(
            worldId,
            parentPostId,
            player.ActorId,
            decodedCursor.Value,
            limit + 1,
            cancellationToken);
        var hasMore = rows.Count > limit;
        var items = rows.Take(limit).ToArray();
        var nextCursor = hasMore
            ? replyCursorCodec.Encode(
                worldId,
                parentPostId,
                new(items[^1].CreatedAtUtc, items[^1].Id))
            : null;
        return SocialResult<FeedPage>.Success(new(items, nextCursor, hasMore));
    }

    public async Task<SocialResult<CreatedPost>> CreateReplyAsync(
        CreateReplyCommand command,
        CancellationToken cancellationToken)
    {
        var content = command.Content.Trim();
        if (string.IsNullOrWhiteSpace(content)
            || new StringInfo(content).LengthInTextElements > Post.MaximumContentCharacters)
        {
            return Invalid<CreatedPost>(
                $"Content is required and must not exceed {Post.MaximumContentCharacters} characters.");
        }

        var requestHash = HashReplyRequest(
            command.WorldId,
            command.ParentPostId,
            command.ClientPostId,
            content);
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var transaction = await unitOfWork.BeginTransactionAsync(
                    ApplicationIsolationLevel.Serializable,
                    cancellationToken);
                var replay = await repository.FindIdempotentPostAsync(
                    command.UserId,
                    CreateReplyOperation,
                    command.IdempotencyKey,
                    cancellationToken);
                if (replay is not null)
                {
                    if (!CryptographicOperations.FixedTimeEquals(
                        Convert.FromHexString(replay.Value.RequestHash),
                        Convert.FromHexString(requestHash)))
                    {
                        return SocialResult<CreatedPost>.Fail(new(
                            "idempotency_key_reused",
                            409,
                            "The idempotency key was already used for a different request."));
                    }

                    return SocialResult<CreatedPost>.Success(new(replay.Value.Post, true));
                }

                var player = await repository.FindPlayerAuthorAsync(
                    command.UserId,
                    command.WorldId,
                    cancellationToken);
                var parent = await repository.FindPostForUpdateAsync(
                    command.WorldId,
                    command.ParentPostId,
                    cancellationToken);
                if (player is null || parent is null)
                {
                    return SocialResult<CreatedPost>.Fail(NotAvailable());
                }

                if (parent.Depth >= 2)
                {
                    return SocialResult<CreatedPost>.Fail(new(
                        "invalid_state_transition",
                        400,
                        "Replies cannot be created beyond depth 2."));
                }

                var now = timeProvider.GetUtcNow();
                var eventId = Guid.NewGuid();
                var replyId = Guid.NewGuid();
                var gameplayEvent = new GameplayEvent(
                    eventId,
                    command.WorldId,
                    "replyCreated",
                    player.ActorId,
                    parent.Post.AuthorActorId == player.ActorId ? null : parent.Post.AuthorActorId,
                    now,
                    0,
                    0,
                    "player_reply",
                    player.RuleVersion,
                    $"m07:player-reply:{command.UserId:N}:{command.IdempotencyKey}",
                    now);
                var reply = new Post(
                    replyId,
                    command.WorldId,
                    player.ActorId,
                    parent.Post.Id,
                    eventId,
                    content,
                    now);
                var idempotency = new IdempotencyRecord(
                    Guid.NewGuid(),
                    command.UserId,
                    command.WorldId,
                    CreateReplyOperation,
                    command.IdempotencyKey,
                    requestHash,
                    replyId,
                    now);
                parent.Post.AddReply();
                repository.AddPlayerPost(gameplayEvent, reply, idempotency);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return SocialResult<CreatedPost>.Success(new(
                    new FeedPost(
                        reply.Id,
                        reply.WorldId,
                        new FeedAuthor(player.ActorId, player.DisplayName, player.Handle, "player", false),
                        reply.Content,
                        reply.CreatedAt,
                        reply.ParentPostId,
                        new FeedCounts(0, 0),
                        null,
                        "world"),
                    false));
            }
            catch (Exception exception) when (failureClassifier.IsRetryableConcurrency(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
        }

        return SocialResult<CreatedPost>.Fail(ConcurrencyFailure("reply"));
    }

    public Task<SocialResult<ReactionState>> SetReactionAsync(
        Guid userId,
        Guid worldId,
        Guid postId,
        ReactionType reactionType,
        CancellationToken cancellationToken) =>
        ChangeReactionAsync(userId, worldId, postId, reactionType, true, cancellationToken);

    public Task<SocialResult<ReactionState>> RemoveReactionAsync(
        Guid userId,
        Guid worldId,
        Guid postId,
        CancellationToken cancellationToken) =>
        ChangeReactionAsync(userId, worldId, postId, ReactionType.Like, false, cancellationToken);

    public async Task<SocialResult<FollowState>> FollowAsync(
        Guid userId,
        Guid worldId,
        Guid actorId,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var transaction = await unitOfWork.BeginTransactionAsync(
                    ApplicationIsolationLevel.Serializable,
                    cancellationToken);
                var player = await repository.FindPlayerAuthorAsync(userId, worldId, cancellationToken);
                if (player is null
                    || actorId == player.ActorId
                    || !await repository.LockCharacterActorAsync(worldId, actorId, cancellationToken))
                {
                    return SocialResult<FollowState>.Fail(NotAvailable());
                }

                var existing = await repository.FindActiveFollowAsync(
                    worldId,
                    player.ActorId,
                    actorId,
                    cancellationToken);
                if (existing is not null)
                {
                    return SocialResult<FollowState>.Success(new(
                        actorId,
                        true,
                        existing.StartedAt));
                }

                var now = timeProvider.GetUtcNow();
                var operationId = Guid.NewGuid();
                var eventId = Guid.NewGuid();
                var gameplayEvent = new GameplayEvent(
                    eventId,
                    worldId,
                    "followStarted",
                    player.ActorId,
                    actorId,
                    now,
                    0,
                    0,
                    "player_follow",
                    player.RuleVersion,
                    $"m07:follow:{operationId:N}",
                    now);
                var follow = new Follow(
                    Guid.NewGuid(),
                    worldId,
                    player.ActorId,
                    actorId,
                    now,
                    eventId,
                    $"m07:follow:{operationId:N}");
                repository.AddFollow(gameplayEvent, follow);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return SocialResult<FollowState>.Success(new(actorId, true, now));
            }
            catch (Exception exception) when (failureClassifier.IsRetryableConcurrency(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
        }

        return SocialResult<FollowState>.Fail(ConcurrencyFailure("follow"));
    }

    public async Task<SocialResult<bool>> UnfollowAsync(
        Guid userId,
        Guid worldId,
        Guid actorId,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var transaction = await unitOfWork.BeginTransactionAsync(
                    ApplicationIsolationLevel.Serializable,
                    cancellationToken);
                var player = await repository.FindPlayerAuthorAsync(userId, worldId, cancellationToken);
                if (player is null
                    || actorId == player.ActorId
                    || !await repository.LockCharacterActorAsync(worldId, actorId, cancellationToken))
                {
                    return SocialResult<bool>.Fail(NotAvailable());
                }

                var existing = await repository.FindActiveFollowAsync(
                    worldId,
                    player.ActorId,
                    actorId,
                    cancellationToken);
                if (existing is not null)
                {
                    existing.End(timeProvider.GetUtcNow());
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return SocialResult<bool>.Success(true);
            }
            catch (Exception exception) when (failureClassifier.IsRetryableConcurrency(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
        }

        return SocialResult<bool>.Fail(ConcurrencyFailure("unfollow"));
    }

    private async Task<SocialResult<ReactionState>> ChangeReactionAsync(
        Guid userId,
        Guid worldId,
        Guid postId,
        ReactionType reactionType,
        bool active,
        CancellationToken cancellationToken)
    {
        if (reactionType != ReactionType.Like)
        {
            return Invalid<ReactionState>("Only the like reaction is supported.");
        }

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var transaction = await unitOfWork.BeginTransactionAsync(
                    ApplicationIsolationLevel.Serializable,
                    cancellationToken);
                var player = await repository.FindPlayerAuthorAsync(userId, worldId, cancellationToken);
                var target = await repository.FindPostForUpdateAsync(worldId, postId, cancellationToken);
                if (player is null || target is null)
                {
                    return SocialResult<ReactionState>.Fail(NotAvailable());
                }

                if (target.Post.AuthorActorId == player.ActorId)
                {
                    return SocialResult<ReactionState>.Fail(new(
                        "invalid_state_transition",
                        400,
                        "A player cannot react to their own post."));
                }

                var existing = await repository.FindReactionAsync(
                    worldId,
                    postId,
                    player.ActorId,
                    cancellationToken);
                if (active && existing is null)
                {
                    var now = timeProvider.GetUtcNow();
                    var operationId = Guid.NewGuid();
                    var eventId = Guid.NewGuid();
                    repository.AddReaction(
                        new GameplayEvent(
                            eventId,
                            worldId,
                            "reactionAdded",
                            player.ActorId,
                            target.Post.AuthorActorId,
                            now,
                            0,
                            0,
                            "player_like",
                            player.RuleVersion,
                            $"m07:like:{operationId:N}",
                            now),
                        new PostReaction(
                            Guid.NewGuid(),
                            worldId,
                            postId,
                            player.ActorId,
                            ReactionType.Like,
                            eventId,
                            now));
                    target.Post.AddLike();
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else if (!active && existing is not null)
                {
                    repository.RemoveReaction(existing);
                    target.Post.RemoveLike();
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return SocialResult<ReactionState>.Success(new(
                    postId,
                    "like",
                    active,
                    target.Post.LikeCount));
            }
            catch (Exception exception) when (failureClassifier.IsRetryableConcurrency(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
        }

        return SocialResult<ReactionState>.Fail(ConcurrencyFailure("reaction"));
    }

    private async Task<ServiceFailure?> EnsureSeedPostsAsync(
        Guid userId,
        Guid worldId,
        CancellationToken cancellationToken)
    {
        var characters = await characterCatalogueService.ListAsync(
            userId,
            worldId,
            CharacterStatus.Active,
            100,
            null,
            cancellationToken);
        if (!characters.IsSuccess)
        {
            return characters.Failure;
        }

        var context = await repository.FindSeedContextAsync(userId, worldId, false, cancellationToken);
        if (context is null)
        {
            return NotAvailable();
        }

        var count = await repository.CountSeedPostsAsync(worldId, cancellationToken);
        if (count == SeedPostFactory.SeedPostCount)
        {
            return null;
        }

        if (count != 0)
        {
            return IncompleteSeed();
        }

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var transaction = await unitOfWork.BeginTransactionAsync(
                    ApplicationIsolationLevel.Serializable,
                    cancellationToken);
                context = await repository.FindSeedContextAsync(userId, worldId, true, cancellationToken);
                if (context is null)
                {
                    return NotAvailable();
                }

                count = await repository.CountSeedPostsAsync(worldId, cancellationToken);
                if (count == 0)
                {
                    var authors = await repository.ListCharacterAuthorsAsync(
                        worldId,
                        SeedPostFactory.SeedPostCount,
                        cancellationToken);
                    repository.AddSeedPosts(SeedPostFactory.Create(context, authors));
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else if (count != SeedPostFactory.SeedPostCount)
                {
                    return IncompleteSeed();
                }

                await transaction.CommitAsync(cancellationToken);
                return null;
            }
            catch (Exception exception) when (failureClassifier.IsRetryableConcurrency(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
        }

        return new ServiceFailure(
            "concurrency_conflict",
            409,
            "The initial feed could not be initialized concurrently.");
    }

    private static string HashRequest(Guid worldId, Guid clientPostId, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(FormattableString.Invariant(
            $"POST\n/api/v1/worlds/{worldId:N}/posts\n{clientPostId:N}\n{content}"));
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private const string CreateReplyOperation = "social.create-reply";

    private static string HashReplyRequest(
        Guid worldId,
        Guid parentPostId,
        Guid clientPostId,
        string content)
    {
        var bytes = Encoding.UTF8.GetBytes(FormattableString.Invariant(
            $"POST\n/api/v1/worlds/{worldId:N}/posts/{parentPostId:N}/replies\n{clientPostId:N}\n{content}"));
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private static SocialResult<T> Invalid<T>(string title, string code = "validation_failed") =>
        SocialResult<T>.Fail(new ServiceFailure(code, 400, title));

    private static ServiceFailure NotAvailable() => new(
        "resource_not_available",
        404,
        "The requested resource is not available.");

    private static ServiceFailure IncompleteSeed() => new(
        "feed_seed_incomplete",
        409,
        "The initial feed is incomplete.");

    private static ServiceFailure ConcurrencyFailure(string operation) => new(
        "concurrency_conflict",
        409,
        $"The {operation} could not be completed concurrently.");
}
