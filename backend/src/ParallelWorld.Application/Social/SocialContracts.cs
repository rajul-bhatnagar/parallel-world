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

public sealed record FeedAuthor(
    Guid ActorId,
    string DisplayName,
    string Handle,
    string ActorType,
    bool IsFollowed);

public sealed record FeedCounts(int Likes, int Replies);

public sealed record FeedPost(
    Guid Id,
    Guid WorldId,
    FeedAuthor Author,
    string Content,
    DateTimeOffset CreatedAtUtc,
    Guid? ParentPostId,
    FeedCounts Counts,
    string? CurrentPlayerReaction,
    string Visibility);

public sealed record FeedPage(IReadOnlyList<FeedPost> Items, string? NextCursor, bool HasMore);

public sealed record FeedCursorValue(DateTimeOffset CreatedAtUtc, Guid Id);

public readonly record struct FeedCursorDecodeResult(bool IsValid, FeedCursorValue? Value);

public sealed record ReplyCursorValue(DateTimeOffset CreatedAtUtc, Guid Id);

public readonly record struct ReplyCursorDecodeResult(bool IsValid, ReplyCursorValue? Value);

public sealed record CreatePostCommand(
    Guid UserId,
    Guid WorldId,
    string Content,
    Guid ClientPostId,
    string IdempotencyKey);

public sealed record CreatedPost(FeedPost Post, bool IsIdempotencyReplay);

public sealed record CreateReplyCommand(
    Guid UserId,
    Guid WorldId,
    Guid ParentPostId,
    string Content,
    Guid ClientPostId,
    string IdempotencyKey);

public sealed record ReactionState(Guid PostId, string Type, bool Active, int LikeCount);

public sealed record FollowState(Guid ActorId, bool IsFollowing, DateTimeOffset FollowedAtUtc);

public sealed record PostActionTarget(Post Post, int Depth);

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

public interface IReplyCursorCodec
{
    string Encode(Guid worldId, Guid parentPostId, ReplyCursorValue value);

    ReplyCursorDecodeResult Decode(string? cursor, Guid worldId, Guid parentPostId);
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
        Guid playerActorId,
        FeedCursorValue? after,
        int take,
        CancellationToken cancellationToken);

    Task<FeedPost?> FindPostAsync(
        Guid worldId,
        Guid postId,
        Guid playerActorId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FeedPost>> ListRepliesAsync(
        Guid worldId,
        Guid parentPostId,
        Guid playerActorId,
        ReplyCursorValue? after,
        int take,
        CancellationToken cancellationToken);

    Task<PostActionTarget?> FindPostForUpdateAsync(
        Guid worldId,
        Guid postId,
        CancellationToken cancellationToken);

    Task<PostReaction?> FindReactionAsync(
        Guid worldId,
        Guid postId,
        Guid actorId,
        CancellationToken cancellationToken);

    Task<bool> LockCharacterActorAsync(
        Guid worldId,
        Guid actorId,
        CancellationToken cancellationToken);

    Task<Follow?> FindActiveFollowAsync(
        Guid worldId,
        Guid followerActorId,
        Guid followedActorId,
        CancellationToken cancellationToken);

    void AddSeedPosts(SeedPostSet seedPosts);

    void AddPlayerPost(GameplayEvent gameplayEvent, Post post, IdempotencyRecord idempotencyRecord);

    void AddReaction(GameplayEvent gameplayEvent, PostReaction reaction);

    void RemoveReaction(PostReaction reaction);

    void AddFollow(GameplayEvent gameplayEvent, Follow follow);
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

    Task<SocialResult<FeedPost>> GetPostAsync(
        Guid userId,
        Guid worldId,
        Guid postId,
        CancellationToken cancellationToken);

    Task<SocialResult<FeedPage>> GetRepliesAsync(
        Guid userId,
        Guid worldId,
        Guid parentPostId,
        int limit,
        string? cursor,
        CancellationToken cancellationToken);

    Task<SocialResult<CreatedPost>> CreateReplyAsync(
        CreateReplyCommand command,
        CancellationToken cancellationToken);

    Task<SocialResult<ReactionState>> SetReactionAsync(
        Guid userId,
        Guid worldId,
        Guid postId,
        ReactionType reactionType,
        CancellationToken cancellationToken);

    Task<SocialResult<ReactionState>> RemoveReactionAsync(
        Guid userId,
        Guid worldId,
        Guid postId,
        CancellationToken cancellationToken);

    Task<SocialResult<FollowState>> FollowAsync(
        Guid userId,
        Guid worldId,
        Guid actorId,
        CancellationToken cancellationToken);

    Task<SocialResult<bool>> UnfollowAsync(
        Guid userId,
        Guid worldId,
        Guid actorId,
        CancellationToken cancellationToken);
}
