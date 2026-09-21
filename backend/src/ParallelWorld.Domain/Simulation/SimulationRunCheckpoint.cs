namespace ParallelWorld.Domain.Simulation;

public sealed class SimulationRunCheckpoint
{
    private SimulationRunCheckpoint()
    {
        IdempotencyKey = string.Empty;
    }

    public SimulationRunCheckpoint(
        Guid id,
        Guid worldId,
        Guid simulationRunId,
        DateTimeOffset bucketStart,
        DateTimeOffset bucketEnd,
        int stableOrdinal,
        DateTimeOffset committedAt,
        string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (bucketEnd <= bucketStart)
        {
            throw new ArgumentException("The checkpoint end must follow its start.");
        }

        if (stableOrdinal < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stableOrdinal));
        }

        Id = id;
        WorldId = worldId;
        SimulationRunId = simulationRunId;
        BucketStart = bucketStart;
        BucketEnd = bucketEnd;
        StableOrdinal = stableOrdinal;
        Status = SimulationCheckpointStatus.Completed;
        CommittedAt = committedAt;
        IdempotencyKey = idempotencyKey;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid SimulationRunId { get; private set; }
    public DateTimeOffset BucketStart { get; private set; }
    public DateTimeOffset BucketEnd { get; private set; }
    public int StableOrdinal { get; private set; }
    public SimulationCheckpointStatus Status { get; private set; }
    public DateTimeOffset CommittedAt { get; private set; }
    public string IdempotencyKey { get; private set; }
    public long Version { get; private set; }
}

public enum SimulationCheckpointStatus
{
    Pending,
    Completed,
}
