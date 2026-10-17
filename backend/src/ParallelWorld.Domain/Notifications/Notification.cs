namespace ParallelWorld.Domain.Notifications;

public enum NotificationCategory
{
    Reply,
    PrivateMessage,
    CatchUpSummary,
}

public enum NotificationSourceType
{
    GameplayEvent,
    CatchUpSummary,
}

public enum NotificationPriority
{
    Normal,
    High,
}

public sealed class Notification
{
    private Notification() { IdempotencyKey = string.Empty; }

    public Notification(
        Guid id,
        Guid worldId,
        Guid recipientUserId,
        Guid? recipientActorId,
        NotificationSourceType sourceType,
        Guid sourceId,
        Guid? gameplayEventId,
        NotificationCategory category,
        NotificationPriority priority,
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt,
        string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if ((category == NotificationCategory.CatchUpSummary)
            != (sourceType == NotificationSourceType.CatchUpSummary))
            throw new ArgumentException("Notification category must use its authoritative source type.");
        if (sourceType == NotificationSourceType.GameplayEvent && gameplayEventId != sourceId)
            throw new ArgumentException("GameplayEvent notification source must match GameplayEventId.");
        if (sourceType == NotificationSourceType.CatchUpSummary && gameplayEventId is not null)
            throw new ArgumentException("CatchUpSummary notification cannot reference a GameplayEvent.");
        if (expiresAt <= createdAt)
            throw new ArgumentOutOfRangeException(nameof(expiresAt));

        Id = id;
        WorldId = worldId;
        RecipientUserId = recipientUserId;
        RecipientActorId = recipientActorId;
        SourceType = sourceType;
        SourceId = sourceId;
        GameplayEventId = gameplayEventId;
        Category = category;
        Priority = priority;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        IdempotencyKey = idempotencyKey;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public Guid? RecipientActorId { get; private set; }
    public NotificationSourceType SourceType { get; private set; }
    public Guid SourceId { get; private set; }
    public Guid? GameplayEventId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public string IdempotencyKey { get; private set; }
    public long Version { get; private set; }

    public void MarkRead(DateTimeOffset readAt)
    {
        if (ReadAt is not null) return;
        if (readAt < CreatedAt) throw new ArgumentOutOfRangeException(nameof(readAt));
        ReadAt = readAt;
        Version++;
    }
}
