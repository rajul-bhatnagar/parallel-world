namespace ParallelWorld.Domain.Worlds;

public sealed class GameWorld
{
    private GameWorld()
    {
        Name = string.Empty;
    }

    public GameWorld(
        Guid id,
        Guid ownerUserId,
        string name,
        long seed,
        DateTimeOffset createdAt)
    {
        Id = id;
        OwnerUserId = ownerUserId;
        Name = name;
        Seed = seed;
        CurrentWorldTime = createdAt;
        LastSimulatedAt = createdAt;
        Status = WorldStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; }

    public long Seed { get; private set; }

    public DateTimeOffset CurrentWorldTime { get; private set; }

    public DateTimeOffset LastSimulatedAt { get; private set; }

    public WorldStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public long Version { get; private set; }
}
