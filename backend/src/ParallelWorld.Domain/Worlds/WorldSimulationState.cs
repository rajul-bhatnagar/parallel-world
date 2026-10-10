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
        NextDueAt = createdAt.Add(ActiveIntervalDuration);
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

    public void CompleteInterval(
        DateTimeOffset intervalStartUtc,
        DateTimeOffset intervalEndUtc,
        DateTimeOffset observedAtUtc)
    {
        if (intervalStartUtc.Offset != TimeSpan.Zero
            || intervalEndUtc.Offset != TimeSpan.Zero
            || observedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Simulation cursor timestamps must be UTC.");
        }

        if (intervalEndUtc - intervalStartUtc != ActiveIntervalDuration)
        {
            throw new ArgumentException("An active simulation interval must be exactly 15 minutes.");
        }

        if (LastCompletedIntervalEnd is DateTimeOffset completedEnd
            && completedEnd != intervalStartUtc)
        {
            throw new InvalidOperationException("The interval does not start at the authoritative cursor.");
        }

        if (NextDueAt != intervalEndUtc)
        {
            throw new InvalidOperationException("The due projection does not match the cursor-derived interval.");
        }

        LastCompletedIntervalEnd = intervalEndUtc;
        NextDueAt = intervalEndUtc.Add(ActiveIntervalDuration);
        DeterministicSequence = checked(DeterministicSequence + 1);
        UpdatedAt = observedAtUtc;
    }

    public void CompleteCatchUpRange(
        DateTimeOffset rangeStartUtc,
        DateTimeOffset rangeEndUtc,
        DateTimeOffset observedAtUtc)
    {
        if (rangeStartUtc.Offset != TimeSpan.Zero
            || rangeEndUtc.Offset != TimeSpan.Zero
            || observedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Simulation cursor timestamps must be UTC.");
        }
        if (rangeEndUtc <= rangeStartUtc)
        {
            throw new ArgumentException("A CatchUp range must advance the cursor.");
        }

        var intervalCount = checked((long)((rangeEndUtc - rangeStartUtc).Ticks
            / ActiveIntervalDuration.Ticks));
        if (intervalCount <= 0
            || rangeStartUtc.AddTicks(intervalCount * ActiveIntervalDuration.Ticks) != rangeEndUtc)
        {
            throw new ArgumentException("A CatchUp range must contain complete 15-minute intervals.");
        }

        var expectedStart = LastCompletedIntervalEnd ?? CreatedAt;
        if (expectedStart != rangeStartUtc || NextDueAt != rangeStartUtc.Add(ActiveIntervalDuration))
        {
            throw new InvalidOperationException("The CatchUp range does not start at the authoritative cursor.");
        }

        LastCompletedIntervalEnd = rangeEndUtc;
        NextDueAt = rangeEndUtc.Add(ActiveIntervalDuration);
        DeterministicSequence = checked(DeterministicSequence + intervalCount);
        UpdatedAt = observedAtUtc;
        Version++;
    }

    public static readonly TimeSpan ActiveIntervalDuration = TimeSpan.FromMinutes(15);
}
