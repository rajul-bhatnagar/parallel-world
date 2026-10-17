namespace ParallelWorld.Domain.Characters;

public sealed class CharacterTraits
{
    private CharacterTraits()
    {
    }

    public CharacterTraits(
        Guid id,
        Guid worldId,
        Guid characterId,
        IReadOnlyList<int> values,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count != 12 || values.Any(value => value is < 0 or > 100))
        {
            throw new ArgumentOutOfRangeException(nameof(values));
        }

        Id = id;
        WorldId = worldId;
        CharacterId = characterId;
        Humour = values[0];
        Confidence = values[1];
        Empathy = values[2];
        Aggression = values[3];
        Curiosity = values[4];
        Honesty = values[5];
        Sociability = values[6];
        Ambition = values[7];
        Patience = values[8];
        Optimism = values[9];
        Sensitivity = values[10];
        RomanticOpenness = values[11];
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid CharacterId { get; private set; }
    public int Humour { get; private set; }
    public int Confidence { get; private set; }
    public int Empathy { get; private set; }
    public int Aggression { get; private set; }
    public int Curiosity { get; private set; }
    public int Honesty { get; private set; }
    public int Sociability { get; private set; }
    public int Ambition { get; private set; }
    public int Patience { get; private set; }
    public int Optimism { get; private set; }
    public int Sensitivity { get; private set; }
    public int RomanticOpenness { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public long Version { get; private set; }
}
