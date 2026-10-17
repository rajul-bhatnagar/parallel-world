using System.Globalization;

namespace ParallelWorld.Domain.Social;

public sealed class Post
{
    public const int MaximumContentCharacters = 500;

    private Post()
    {
        Content = string.Empty;
    }

    public Post(
        Guid id,
        Guid worldId,
        Guid authorActorId,
        Guid? parentPostId,
        Guid gameplayEventId,
        string content,
        DateTimeOffset createdAt)
    {
        var normalizedContent = content.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedContent);
        if (new StringInfo(normalizedContent).LengthInTextElements > MaximumContentCharacters)
        {
            throw new ArgumentOutOfRangeException(nameof(content));
        }

        if (id == parentPostId)
        {
            throw new ArgumentException("A post cannot be its own parent.", nameof(parentPostId));
        }

        Id = id;
        WorldId = worldId;
        AuthorActorId = authorActorId;
        ParentPostId = parentPostId;
        GameplayEventId = gameplayEventId;
        Content = normalizedContent;
        Visibility = PostVisibility.World;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid AuthorActorId { get; private set; }
    public Guid? ParentPostId { get; private set; }
    public Guid GameplayEventId { get; private set; }
    public string Content { get; private set; }
    public PostVisibility Visibility { get; private set; }
    public int LikeCount { get; private set; }
    public int ReplyCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public long Version { get; private set; }

    public void AddLike() => LikeCount = checked(LikeCount + 1);

    public void RemoveLike()
    {
        if (LikeCount <= 0)
        {
            throw new InvalidOperationException("The like count cannot become negative.");
        }

        LikeCount--;
    }

    public void AddReply() => ReplyCount = checked(ReplyCount + 1);
}
