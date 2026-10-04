namespace ParallelWorld.Domain.Memory;

public sealed class Promise
{
    private Promise()
    {
        PromiseType = string.Empty;
        DueEventType = null;
    }

    public Promise(
        Guid id,
        Guid worldId,
        Guid memoryId,
        string promiseType,
        Guid sourceActorId,
        Guid? targetActorId,
        PromiseDueConditionType dueConditionType,
        DateTimeOffset? dueAtWorldTime,
        Guid? dueGameplayEventId,
        string? dueEventType,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(promiseType);
        if (sourceActorId == Guid.Empty || sourceActorId == targetActorId)
        {
            throw new ArgumentException("Promise actors are invalid.");
        }

        ValidateDueCondition(dueConditionType, dueAtWorldTime, dueGameplayEventId, dueEventType);
        Id = id;
        WorldId = worldId;
        MemoryId = memoryId;
        PromiseType = promiseType.Trim();
        SourceActorId = sourceActorId;
        TargetActorId = targetActorId;
        DueConditionType = dueConditionType;
        DueAtWorldTime = dueAtWorldTime;
        DueGameplayEventId = dueGameplayEventId;
        DueEventType = string.IsNullOrWhiteSpace(dueEventType) ? null : dueEventType.Trim();
        Status = PromiseStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid MemoryId { get; private set; }
    public string PromiseType { get; private set; }
    public Guid SourceActorId { get; private set; }
    public Guid? TargetActorId { get; private set; }
    public PromiseDueConditionType DueConditionType { get; private set; }
    public DateTimeOffset? DueAtWorldTime { get; private set; }
    public Guid? DueGameplayEventId { get; private set; }
    public string? DueEventType { get; private set; }
    public PromiseStatus Status { get; private set; }
    public Guid? ResolutionGameplayEventId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    public long Version { get; private set; }

    public void Fulfill(Guid gameplayEventId, string eventType, DateTimeOffset occurredAtUtc)
    {
        EnsureActive();
        if (DueConditionType != PromiseDueConditionType.GameplayEvent
            || (DueGameplayEventId.HasValue && DueGameplayEventId != gameplayEventId)
            || (!string.IsNullOrWhiteSpace(DueEventType)
                && !string.Equals(DueEventType, eventType, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("The gameplay event does not satisfy the promise condition.");
        }

        Resolve(PromiseStatus.Fulfilled, gameplayEventId, occurredAtUtc);
    }

    public void Cancel(Guid gameplayEventId, DateTimeOffset occurredAtUtc)
    {
        EnsureActive();
        Resolve(PromiseStatus.Cancelled, gameplayEventId, occurredAtUtc);
    }

    public void Expire(DateTimeOffset worldTime)
    {
        EnsureActive();
        if (DueConditionType != PromiseDueConditionType.WorldTime
            || DueAtWorldTime is null
            || worldTime < DueAtWorldTime)
        {
            throw new InvalidOperationException("The promise has not reached an explicit due time.");
        }

        Resolve(PromiseStatus.Expired, null, worldTime);
    }

    private static void ValidateDueCondition(
        PromiseDueConditionType type,
        DateTimeOffset? dueAt,
        Guid? eventId,
        string? eventType)
    {
        var valid = type switch
        {
            PromiseDueConditionType.WorldTime => dueAt.HasValue
                && eventId is null && string.IsNullOrWhiteSpace(eventType),
            PromiseDueConditionType.GameplayEvent => dueAt is null
                && (eventId.HasValue || !string.IsNullOrWhiteSpace(eventType)),
            _ => false,
        };
        if (!valid)
        {
            throw new ArgumentException("A supported structured promise due condition is required.");
        }
    }

    private void EnsureActive()
    {
        if (Status != PromiseStatus.Active)
        {
            throw new InvalidOperationException("Promise terminal states cannot transition.");
        }
    }

    private void Resolve(PromiseStatus status, Guid? gameplayEventId, DateTimeOffset at)
    {
        Status = status;
        ResolutionGameplayEventId = gameplayEventId;
        ResolvedAtUtc = at;
        Version++;
    }
}
