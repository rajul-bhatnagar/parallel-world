namespace ParallelWorld.Domain.Characters;

public sealed class Character
{
    private Character()
    {
        DisplayName = string.Empty;
        Handle = string.Empty;
        Bio = string.Empty;
        Profession = string.Empty;
        Archetype = string.Empty;
        WritingStyle = string.Empty;
    }

    public Character(
        Guid id,
        Guid worldId,
        string displayName,
        string handle,
        string bio,
        int age,
        string profession,
        string archetype,
        string writingStyle,
        int activityLevel,
        int influence,
        int popularity,
        MoodType currentMoodType,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(handle);
        ArgumentException.ThrowIfNullOrWhiteSpace(profession);
        ArgumentException.ThrowIfNullOrWhiteSpace(archetype);
        ArgumentException.ThrowIfNullOrWhiteSpace(writingStyle);
        ValidateRange(age, 0, int.MaxValue, nameof(age));
        ValidateRange(activityLevel, 0, 100, nameof(activityLevel));
        ValidateRange(influence, 0, 100, nameof(influence));
        ValidateRange(popularity, 0, 100, nameof(popularity));

        Id = id;
        WorldId = worldId;
        DisplayName = displayName;
        Handle = handle;
        Bio = bio;
        Age = age;
        Profession = profession;
        Archetype = archetype;
        WritingStyle = writingStyle;
        ActivityLevel = activityLevel;
        Influence = influence;
        Popularity = popularity;
        CurrentMoodType = currentMoodType;
        Status = CharacterStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public string DisplayName { get; private set; }
    public string Handle { get; private set; }
    public string Bio { get; private set; }
    public int Age { get; private set; }
    public string Profession { get; private set; }
    public string Archetype { get; private set; }
    public string WritingStyle { get; private set; }
    public int ActivityLevel { get; private set; }
    public int Influence { get; private set; }
    public int Popularity { get; private set; }
    public MoodType CurrentMoodType { get; private set; }
    public CharacterStatus Status { get; private set; }
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
