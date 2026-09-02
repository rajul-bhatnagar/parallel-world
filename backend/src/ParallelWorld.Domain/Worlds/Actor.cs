namespace ParallelWorld.Domain.Worlds;

public sealed class Actor
{
    private Actor()
    {
    }

    public Actor(Guid id, Guid worldId, Guid playerProfileId, DateTimeOffset createdAt)
    {
        Id = id;
        WorldId = worldId;
        ActorType = ActorType.Player;
        PlayerProfileId = playerProfileId;
        Status = ActorStatus.Active;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid WorldId { get; private set; }

    public ActorType ActorType { get; private set; }

    public Guid? PlayerProfileId { get; private set; }

    public Guid? CharacterId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public ActorStatus Status { get; private set; }

    public long Version { get; private set; }
}
