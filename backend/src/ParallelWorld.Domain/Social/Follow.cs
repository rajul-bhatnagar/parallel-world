namespace ParallelWorld.Domain.Social;

public sealed class Follow
{
    private Follow()
    {
        IdempotencyKey = string.Empty;
    }

    public Follow(
        Guid id,
        Guid worldId,
        Guid followerActorId,
        Guid followedActorId,
        DateTimeOffset startedAt,
        Guid gameplayEventId,
        string idempotencyKey)
    {
        if (followerActorId == followedActorId)
        {
            throw new ArgumentException("An actor cannot follow itself.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        Id = id;
        WorldId = worldId;
        FollowerActorId = followerActorId;
        FollowedActorId = followedActorId;
        StartedAt = startedAt;
        GameplayEventId = gameplayEventId;
        IdempotencyKey = idempotencyKey;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid FollowerActorId { get; private set; }
    public Guid FollowedActorId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public Guid GameplayEventId { get; private set; }
    public string IdempotencyKey { get; private set; }

    public void End(DateTimeOffset endedAt)
    {
        if (EndedAt is not null)
        {
            return;
        }

        if (endedAt < StartedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(endedAt));
        }

        EndedAt = endedAt;
    }
}
