namespace ParallelWorld.Domain.Characters;

public sealed class CharacterOpinion
{
    private CharacterOpinion()
    {
        TopicId = string.Empty;
    }

    public CharacterOpinion(
        Guid id,
        Guid worldId,
        Guid characterId,
        string topicId,
        int position,
        int confidence,
        int intensity,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicId);
        ValidateRange(position, -100, 100, nameof(position));
        ValidateRange(confidence, 0, 100, nameof(confidence));
        ValidateRange(intensity, 0, 100, nameof(intensity));

        Id = id;
        WorldId = worldId;
        CharacterId = characterId;
        TopicId = topicId;
        Position = position;
        Confidence = confidence;
        Intensity = intensity;
        LastChangedAt = createdAt;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid CharacterId { get; private set; }
    public string TopicId { get; private set; }
    public int Position { get; private set; }
    public int Confidence { get; private set; }
    public int Intensity { get; private set; }
    public DateTimeOffset LastChangedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public long Version { get; private set; }

    private static void ValidateRange(int value, int minimum, int maximum, string parameterName)
    {
        if (value < minimum || value > maximum)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
