namespace ParallelWorld.Domain.Memory;

public sealed class CharacterMemory
{
    private CharacterMemory()
    {
        StructuredContent = string.Empty;
        SubjectTopicId = null;
        TopicId = null;
    }

    public CharacterMemory(
        Guid id,
        Guid worldId,
        Guid ownerCharacterId,
        MemoryType memoryType,
        MemoryAuthorityType authorityType,
        MemorySubjectType subjectType,
        Guid? subjectActorId,
        string? subjectTopicId,
        string? topicId,
        string structuredContent,
        MemorySourceType sourceType,
        Guid sourceId,
        Guid? gameplayEventId,
        Guid? messageId,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty || worldId == Guid.Empty || ownerCharacterId == Guid.Empty
            || sourceId == Guid.Empty)
        {
            throw new ArgumentException("Memory identities are required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(structuredContent);
        if (structuredContent.Length > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(structuredContent));
        }

        ValidateSubject(subjectType, subjectActorId, subjectTopicId);
        ValidateSource(sourceType, sourceId, gameplayEventId, messageId);
        if (!MemoryMechanics.IsApprovedMapping(memoryType, authorityType))
        {
            throw new ArgumentException("The authority category does not map to the memory type.");
        }

        Id = id;
        WorldId = worldId;
        OwnerCharacterId = ownerCharacterId;
        MemoryType = memoryType;
        AuthorityType = authorityType;
        SubjectType = subjectType;
        SubjectActorId = subjectActorId;
        SubjectTopicId = Normalize(subjectTopicId);
        TopicId = Normalize(topicId);
        StructuredContent = structuredContent.Trim();
        Confidence = MemoryMechanics.Confidence(authorityType);
        Importance = MemoryMechanics.Importance(memoryType);
        Visibility = MemoryVisibility.CharacterPrivate;
        LifecycleStatus = MemoryLifecycleStatus.Active;
        SourceType = sourceType;
        SourceId = sourceId;
        GameplayEventId = gameplayEventId;
        MessageId = messageId;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid OwnerCharacterId { get; private set; }
    public MemoryType MemoryType { get; private set; }
    public MemoryAuthorityType AuthorityType { get; private set; }
    public MemorySubjectType SubjectType { get; private set; }
    public Guid? SubjectActorId { get; private set; }
    public string? SubjectTopicId { get; private set; }
    public string? TopicId { get; private set; }
    public string StructuredContent { get; private set; }
    public int Confidence { get; private set; }
    public int Importance { get; private set; }
    public MemoryVisibility Visibility { get; private set; }
    public MemoryLifecycleStatus LifecycleStatus { get; private set; }
    public MemorySourceType SourceType { get; private set; }
    public Guid SourceId { get; private set; }
    public Guid? GameplayEventId { get; private set; }
    public Guid? MessageId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? EvictedAtUtc { get; private set; }
    public long Version { get; private set; }

    public void Evict(DateTimeOffset evictedAtUtc)
    {
        if (LifecycleStatus != MemoryLifecycleStatus.Active)
        {
            return;
        }

        if (MemoryMechanics.IsProtected(MemoryType))
        {
            throw new InvalidOperationException("Protected memories cannot be automatically evicted.");
        }

        LifecycleStatus = MemoryLifecycleStatus.Evicted;
        EvictedAtUtc = evictedAtUtc;
        Version++;
    }

    internal static void ValidateSource(
        MemorySourceType sourceType,
        Guid sourceId,
        Guid? gameplayEventId,
        Guid? messageId)
    {
        var valid = sourceType switch
        {
            MemorySourceType.GameplayEvent => gameplayEventId == sourceId && messageId is null,
            MemorySourceType.Message => messageId == sourceId && gameplayEventId is null,
            _ => false,
        };
        if (!valid)
        {
            throw new ArgumentException("Exactly one matching typed source is required.");
        }
    }

    private static void ValidateSubject(
        MemorySubjectType subjectType,
        Guid? subjectActorId,
        string? subjectTopicId)
    {
        var valid = subjectType switch
        {
            MemorySubjectType.Actor => subjectActorId is not null && subjectActorId != Guid.Empty
                && string.IsNullOrWhiteSpace(subjectTopicId),
            MemorySubjectType.Topic => subjectActorId is null && !string.IsNullOrWhiteSpace(subjectTopicId),
            _ => false,
        };
        if (!valid)
        {
            throw new ArgumentException("Exactly one authoritative subject is required.");
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
