namespace ParallelWorld.Domain.Memory;

public sealed class MemoryRecallRequest
{
    private MemoryRecallRequest()
    {
        SubjectTopicId = null;
        TopicId = null;
        IdempotencyKey = string.Empty;
    }

    public MemoryRecallRequest(
        Guid id,
        Guid worldId,
        Guid characterId,
        MemoryRecallPurpose purpose,
        MemorySubjectType subjectType,
        Guid? subjectActorId,
        string? subjectTopicId,
        string? topicId,
        string idempotencyKey,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        Id = id;
        WorldId = worldId;
        CharacterId = characterId;
        Purpose = purpose;
        SubjectType = subjectType;
        SubjectActorId = subjectActorId;
        SubjectTopicId = string.IsNullOrWhiteSpace(subjectTopicId) ? null : subjectTopicId.Trim();
        TopicId = string.IsNullOrWhiteSpace(topicId) ? null : topicId.Trim();
        IdempotencyKey = idempotencyKey;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid CharacterId { get; private set; }
    public MemoryRecallPurpose Purpose { get; private set; }
    public MemorySubjectType SubjectType { get; private set; }
    public Guid? SubjectActorId { get; private set; }
    public string? SubjectTopicId { get; private set; }
    public string? TopicId { get; private set; }
    public string IdempotencyKey { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

public sealed class MemoryRecallSelection
{
    private MemoryRecallSelection() { }

    public MemoryRecallSelection(
        Guid worldId,
        Guid requestId,
        Guid memoryId,
        int rank,
        int score,
        DateTimeOffset usedAtUtc)
    {
        if (rank is < 1 or > MemoryMechanics.MaximumRecallCount || score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(rank));
        }

        WorldId = worldId;
        RequestId = requestId;
        MemoryId = memoryId;
        Rank = rank;
        Score = score;
        UsedAtUtc = usedAtUtc;
    }

    public Guid WorldId { get; private set; }
    public Guid RequestId { get; private set; }
    public Guid MemoryId { get; private set; }
    public int Rank { get; private set; }
    public int Score { get; private set; }
    public DateTimeOffset UsedAtUtc { get; private set; }
}
