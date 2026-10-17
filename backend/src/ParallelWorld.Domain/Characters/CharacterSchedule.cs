namespace ParallelWorld.Domain.Characters;

public sealed class CharacterSchedule
{
    private CharacterSchedule()
    {
        Activity = string.Empty;
    }

    public CharacterSchedule(
        Guid id,
        Guid worldId,
        Guid characterId,
        int dayOfWeek,
        TimeOnly startLocalTime,
        TimeOnly endLocalTime,
        string activity,
        DateTimeOffset createdAt)
    {
        if (dayOfWeek is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(dayOfWeek));
        }

        if (endLocalTime <= startLocalTime)
        {
            throw new ArgumentOutOfRangeException(nameof(endLocalTime));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(activity);
        Id = id;
        WorldId = worldId;
        CharacterId = characterId;
        DayOfWeek = dayOfWeek;
        StartLocalTime = startLocalTime;
        EndLocalTime = endLocalTime;
        Activity = activity;
        Status = CharacterScheduleStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid CharacterId { get; private set; }
    public int DayOfWeek { get; private set; }
    public TimeOnly StartLocalTime { get; private set; }
    public TimeOnly EndLocalTime { get; private set; }
    public string Activity { get; private set; }
    public CharacterScheduleStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public long Version { get; private set; }
}

public enum CharacterScheduleStatus
{
    Active,
    Inactive,
}
