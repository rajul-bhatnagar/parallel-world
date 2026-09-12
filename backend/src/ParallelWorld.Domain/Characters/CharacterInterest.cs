namespace ParallelWorld.Domain.Characters;

public sealed class CharacterInterest
{
    private CharacterInterest()
    {
        TopicId = string.Empty;
    }

    public CharacterInterest(
        Guid id,
        Guid worldId,
        Guid characterId,
        string topicId,
        int strength,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicId);
        if (strength is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(strength));
        }

        Id = id;
        WorldId = worldId;
        CharacterId = characterId;
        TopicId = topicId;
        Strength = strength;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid CharacterId { get; private set; }
    public string TopicId { get; private set; }
    public int Strength { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public long Version { get; private set; }
}
