namespace ParallelWorld.Domain.Social;

public sealed class PostReaction
{
    private PostReaction()
    {
    }

    public PostReaction(
        Guid id,
        Guid worldId,
        Guid postId,
        Guid actorId,
        ReactionType reactionType,
        Guid gameplayEventId,
        DateTimeOffset createdAt)
    {
        Id = id;
        WorldId = worldId;
        PostId = postId;
        ActorId = actorId;
        ReactionType = reactionType;
        GameplayEventId = gameplayEventId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid PostId { get; private set; }
    public Guid ActorId { get; private set; }
    public ReactionType ReactionType { get; private set; }
    public Guid GameplayEventId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
