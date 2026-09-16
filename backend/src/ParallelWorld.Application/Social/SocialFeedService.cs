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

        var rows = await repository.ListFeedAsync(
            worldId,
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
                        new FeedAuthor(player.ActorId, player.DisplayName, player.Handle, "player"),
                        post.Content,
                        post.CreatedAt,
                        post.ParentPostId,
                        new FeedCounts(post.LikeCount, post.ReplyCount),
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
}
