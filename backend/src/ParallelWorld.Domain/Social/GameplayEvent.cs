namespace ParallelWorld.Domain.Social;

public sealed class GameplayEvent
{
    private GameplayEvent()
    {
        EventType = string.Empty;
        ReasonCode = string.Empty;
        IdempotencyKey = string.Empty;
    }

    public GameplayEvent(
        Guid id,
        Guid worldId,
        string eventType,
        Guid? actorId,
        Guid? targetActorId,
        DateTimeOffset occurredAt,
        int importance,
        int emotionalImpact,
        string reasonCode,
        int ruleVersion,
        string idempotencyKey,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (importance is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(importance));
        }

        if (emotionalImpact is < -100 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(emotionalImpact));
        }

        if (actorId is not null && actorId == targetActorId)
        {
            throw new ArgumentException("An event actor and target actor must be distinct.");
        }

        Id = id;
        WorldId = worldId;
        EventType = eventType;
        ActorId = actorId;
        TargetActorId = targetActorId;
        OccurredAt = occurredAt;
        Importance = importance;
        EmotionalImpact = emotionalImpact;
        ReasonCode = reasonCode;
        RuleVersion = ruleVersion;
        IdempotencyKey = idempotencyKey;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public string EventType { get; private set; }
    public Guid? ActorId { get; private set; }
    public Guid? TargetActorId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public int Importance { get; private set; }
    public int EmotionalImpact { get; private set; }
    public string ReasonCode { get; private set; }
    public int RuleVersion { get; private set; }
    public string IdempotencyKey { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
