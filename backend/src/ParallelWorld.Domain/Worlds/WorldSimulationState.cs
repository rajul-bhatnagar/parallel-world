namespace ParallelWorld.Domain.Worlds;

public sealed class WorldSimulationState
{
    private WorldSimulationState()
    {
    }

    public WorldSimulationState(Guid id, Guid worldId, DateTimeOffset createdAt)
    {
        Id = id;
        WorldId = worldId;
        NextDueAt = createdAt;
        DeterministicSequence = 0;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid WorldId { get; private set; }

    public DateTimeOffset NextDueAt { get; private set; }

    public DateTimeOffset? LastCompletedIntervalEnd { get; private set; }

    public long DeterministicSequence { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public long Version { get; private set; }
}
