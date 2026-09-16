using ParallelWorld.Application.Common;
using ParallelWorld.Domain.Social;

namespace ParallelWorld.Application.Social;

public sealed record FeedSeedContext(
    Guid WorldId,
    long WorldSeed,
    int RuleVersion,
    DateTimeOffset WorldCreatedAt);

public sealed record CharacterPostAuthor(Guid ActorId, string DisplayName, string Handle);

public sealed record PlayerPostAuthor(Guid ActorId, string DisplayName, string Handle, int RuleVersion);

public sealed record FeedAuthor(Guid ActorId, string DisplayName, string Handle, string ActorType);

public sealed record FeedCounts(int Likes, int Replies);

public sealed record FeedPost(
    Guid Id,
    Guid WorldId,
    FeedAuthor Author,
    string Content,
    DateTimeOffset CreatedAtUtc,
    Guid? ParentPostId,
    FeedCounts Counts,
    string Visibility);

public sealed record FeedPage(IReadOnlyList<FeedPost> Items, string? NextCursor, bool HasMore);

public sealed record FeedCursorValue(DateTimeOffset CreatedAtUtc, Guid Id);

public readonly record struct FeedCursorDecodeResult(bool IsValid, FeedCursorValue? Value);

public sealed record CreatePostCommand(
    Guid UserId,
    Guid WorldId,
    string Content,
    Guid ClientPostId,
    string IdempotencyKey);

public sealed record CreatedPost(FeedPost Post, bool IsIdempotencyReplay);

public sealed record SocialResult<T>(T? Value, ServiceFailure? Failure)
{
    public bool IsSuccess => Failure is null;

    public static SocialResult<T> Success(T value) => new(value, null);

    public static SocialResult<T> Fail(ServiceFailure failure) => new(default, failure);
}

public sealed record SeedPostSet(
    IReadOnlyList<GameplayEvent> Events,
    IReadOnlyList<Post> Posts);

public interface IFeedCursorCodec
{
    string Encode(Guid worldId, FeedCursorValue value);

    FeedCursorDecodeResult Decode(string? cursor, Guid worldId);
}

public interface ISocialRepository
{
    Task<FeedSeedContext?> FindSeedContextAsync(
        Guid ownerUserId,
        Guid worldId,
        bool forUpdate,
        CancellationToken cancellationToken);

    Task<int> CountSeedPostsAsync(Guid worldId, CancellationToken cancellationToken);

    Task<IReadOnlyList<CharacterPostAuthor>> ListCharacterAuthorsAsync(
        Guid worldId,
        int take,
        CancellationToken cancellationToken);

    Task<PlayerPostAuthor?> FindPlayerAuthorAsync(
        Guid ownerUserId,
        Guid worldId,
        CancellationToken cancellationToken);

    Task<(string RequestHash, FeedPost Post)?> FindIdempotentPostAsync(
        Guid userId,
        string operation,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FeedPost>> ListFeedAsync(
        Guid worldId,
        FeedCursorValue? after,
        int take,
        CancellationToken cancellationToken);

    void AddSeedPosts(SeedPostSet seedPosts);

    void AddPlayerPost(GameplayEvent gameplayEvent, Post post, IdempotencyRecord idempotencyRecord);
}

public interface ISocialFeedService
{
    Task<SocialResult<FeedPage>> GetFeedAsync(
        Guid userId,
        Guid worldId,
        int limit,
        string? cursor,
        CancellationToken cancellationToken);

    Task<SocialResult<CreatedPost>> CreatePostAsync(
        CreatePostCommand command,
        CancellationToken cancellationToken);
}
