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

    public void AdvanceSimulation(
        DateTimeOffset intervalEndUtc,
        long worldTimeDeltaTicks,
        DateTimeOffset observedAtUtc)
    {
        if (intervalEndUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("The simulation interval end must be UTC.", nameof(intervalEndUtc));
        }

        if (observedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("The observation time must be UTC.", nameof(observedAtUtc));
        }

        if (worldTimeDeltaTicks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(worldTimeDeltaTicks));
        }

        if (intervalEndUtc <= LastSimulatedAt)
        {
            throw new InvalidOperationException("Simulation time must advance monotonically.");
        }

        CurrentWorldTime = CurrentWorldTime.AddTicks(worldTimeDeltaTicks);
        LastSimulatedAt = intervalEndUtc;
        UpdatedAt = observedAtUtc;
    }
}
