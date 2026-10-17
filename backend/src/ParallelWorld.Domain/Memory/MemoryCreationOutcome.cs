namespace ParallelWorld.Domain.Memory;

public sealed class MemoryCreationOutcome
{
    private MemoryCreationOutcome() { ReasonCode = null; }

    public MemoryCreationOutcome(
        Guid id,
        Guid worldId,
        Guid ownerCharacterId,
        MemoryType memoryType,
        MemorySourceType sourceType,
        Guid sourceId,
        Guid? gameplayEventId,
        Guid? messageId,
        MemoryCreationOutcomeType outcome,
        string? reasonCode,
        Guid? memoryId,
        DateTimeOffset createdAtUtc)
    {
        CharacterMemory.ValidateSource(sourceType, sourceId, gameplayEventId, messageId);
        if ((outcome == MemoryCreationOutcomeType.Created) != memoryId.HasValue
            || (outcome == MemoryCreationOutcomeType.Rejected) != !string.IsNullOrWhiteSpace(reasonCode))
        {
            throw new ArgumentException("The memory creation outcome shape is invalid.");
        }

        Id = id;
        WorldId = worldId;
        OwnerCharacterId = ownerCharacterId;
        MemoryType = memoryType;
        SourceType = sourceType;
        SourceId = sourceId;
        GameplayEventId = gameplayEventId;
        MessageId = messageId;
        Outcome = outcome;
        ReasonCode = reasonCode;
        MemoryId = memoryId;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid OwnerCharacterId { get; private set; }
    public MemoryType MemoryType { get; private set; }
    public MemorySourceType SourceType { get; private set; }
    public Guid SourceId { get; private set; }
    public Guid? GameplayEventId { get; private set; }
    public Guid? MessageId { get; private set; }
    public MemoryCreationOutcomeType Outcome { get; private set; }
    public string? ReasonCode { get; private set; }
    public Guid? MemoryId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
