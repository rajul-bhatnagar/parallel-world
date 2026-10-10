namespace ParallelWorld.Domain.Simulation;

public enum CatchUpSummaryStatus
{
    Processing,
    Partial,
    Completed,
    FailedRetryable,
}

public sealed class CatchUpSummary
{
    private CatchUpSummary() { IdempotencyKey = Text = string.Empty; }

    public CatchUpSummary(
        Guid id,
        Guid worldId,
        Guid simulationRunId,
        DateTimeOffset fromGameTime,
        DateTimeOffset generatedAt,
        string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        Id = id;
        WorldId = worldId;
        SimulationRunId = simulationRunId;
        FromGameTime = fromGameTime;
        ToGameTime = fromGameTime;
        Status = CatchUpSummaryStatus.Processing;
        GeneratedAt = generatedAt;
        Text = "Your world is catching up.";
        IdempotencyKey = idempotencyKey;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid SimulationRunId { get; private set; }
    public DateTimeOffset FromGameTime { get; private set; }
    public DateTimeOffset ToGameTime { get; private set; }
    public CatchUpSummaryStatus Status { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }
    public string Text { get; private set; }
    public string IdempotencyKey { get; private set; }
    public long Version { get; private set; }

    public void Update(
        DateTimeOffset toGameTime,
        CatchUpSummaryStatus status,
        DateTimeOffset generatedAt,
        int committedFactCount)
    {
        if (toGameTime < ToGameTime || toGameTime < FromGameTime)
        {
            throw new InvalidOperationException("CatchUp summary time cannot move backwards.");
        }

        ToGameTime = toGameTime;
        Status = status;
        GeneratedAt = generatedAt;
        Text = committedFactCount == 0
            ? "Your world advanced. There are no major changes to report."
            : committedFactCount == 1
                ? "Your world advanced with 1 meaningful change."
                : $"Your world advanced with {committedFactCount} meaningful changes.";
        Version++;
    }
}

public sealed class CatchUpSummaryItem
{
    private CatchUpSummaryItem() { ItemType = FactCode = Wording = string.Empty; }

    public CatchUpSummaryItem(
        Guid id,
        Guid worldId,
        Guid catchUpSummaryId,
        Guid gameplayEventId,
        string itemType,
        int stableOrdinal,
        string factCode,
        Guid? actorId,
        Guid? targetActorId,
        DateOnly gameDate,
        string wording,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemType);
        ArgumentException.ThrowIfNullOrWhiteSpace(factCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(wording);
        if (stableOrdinal < 0) throw new ArgumentOutOfRangeException(nameof(stableOrdinal));
        Id = id;
        WorldId = worldId;
        CatchUpSummaryId = catchUpSummaryId;
        GameplayEventId = gameplayEventId;
        ItemType = itemType;
        StableOrdinal = stableOrdinal;
        FactCode = factCode;
        ActorId = actorId;
        TargetActorId = targetActorId;
        GameDate = gameDate;
        Wording = wording;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid CatchUpSummaryId { get; private set; }
    public Guid GameplayEventId { get; private set; }
    public string ItemType { get; private set; }
    public int StableOrdinal { get; private set; }
    public string FactCode { get; private set; }
    public Guid? ActorId { get; private set; }
    public Guid? TargetActorId { get; private set; }
    public DateOnly GameDate { get; private set; }
    public string Wording { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
