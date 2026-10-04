namespace ParallelWorld.Domain.Memory;

public sealed class Secret
{
    private Secret() { }

    public Secret(Guid id, Guid worldId, Guid memoryId, DateTimeOffset createdAtUtc)
    {
        Id = id;
        WorldId = worldId;
        MemoryId = memoryId;
        Status = SecretStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid MemoryId { get; private set; }
    public SecretStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public long Version { get; private set; }
}

public sealed class SecretKnower
{
    private SecretKnower() { }

    public SecretKnower(Guid worldId, Guid secretId, Guid characterId, DateTimeOffset learnedAtUtc)
    {
        WorldId = worldId;
        SecretId = secretId;
        CharacterId = characterId;
        Status = SecretStatus.Active;
        LearnedAtUtc = learnedAtUtc;
    }

    public Guid WorldId { get; private set; }
    public Guid SecretId { get; private set; }
    public Guid CharacterId { get; private set; }
    public SecretStatus Status { get; private set; }
    public DateTimeOffset LearnedAtUtc { get; private set; }
}
