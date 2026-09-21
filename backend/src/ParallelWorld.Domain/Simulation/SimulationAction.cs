namespace ParallelWorld.Domain.Simulation;

public sealed class SimulationAction
{
    private SimulationAction()
    {
        ActionType = string.Empty;
        ReasonCode = string.Empty;
        IdempotencyKey = string.Empty;
    }

    public SimulationAction(
        Guid id,
        Guid worldId,
        Guid simulationRunId,
        int stableOrdinal,
        Guid actorId,
        string actionType,
        Guid? targetActorId,
        Guid? targetPostId,
        Guid? topicId,
        string? stance,
        string? tone,
        string reasonCode,
        DateTimeOffset scheduledAt,
        string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionType);
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (stableOrdinal < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stableOrdinal));
        }

        Id = id;
        WorldId = worldId;
        SimulationRunId = simulationRunId;
        StableOrdinal = stableOrdinal;
        ActorId = actorId;
        ActionType = actionType;
        TargetActorId = targetActorId;
        TargetPostId = targetPostId;
        TopicId = topicId;
        Stance = stance;
        Tone = tone;
        ReasonCode = reasonCode;
        Status = SimulationActionStatus.Pending;
        ScheduledAt = scheduledAt;
        IdempotencyKey = idempotencyKey;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid SimulationRunId { get; private set; }
    public int StableOrdinal { get; private set; }
    public Guid ActorId { get; private set; }
    public string ActionType { get; private set; }
    public Guid? TargetActorId { get; private set; }
    public Guid? TargetPostId { get; private set; }
    public Guid? TopicId { get; private set; }
    public string? Stance { get; private set; }
    public string? Tone { get; private set; }
    public string ReasonCode { get; private set; }
    public SimulationActionStatus Status { get; private set; }
    public DateTimeOffset ScheduledAt { get; private set; }
    public DateTimeOffset? ExecutedAt { get; private set; }
    public string IdempotencyKey { get; private set; }
    public long Version { get; private set; }
}

public enum SimulationActionStatus
{
    Pending,
    Executed,
    Cancelled,
}
