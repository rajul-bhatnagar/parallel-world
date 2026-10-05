namespace ParallelWorld.Domain.Worlds;

public sealed class PlayerProfile
{
    private PlayerProfile()
    {
        DisplayName = string.Empty;
        Handle = string.Empty;
        Bio = string.Empty;
        RomancePreferenceMode = RomancePreferenceMode.AnyEligibleActor;
    }

    public PlayerProfile(Guid id, Guid worldId, DateTimeOffset createdAt)
    {
        Id = id;
        WorldId = worldId;
        DisplayName = "Player";
        Handle = "player";
        Bio = string.Empty;
        RomancePreferenceMode = RomancePreferenceMode.AnyEligibleActor;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid WorldId { get; private set; }

    public string DisplayName { get; private set; }

    public string Handle { get; private set; }

    public string Bio { get; private set; }

    public RomancePreferenceMode RomancePreferenceMode { get; private set; }

    public void SetRomancePreferenceMode(RomancePreferenceMode mode, DateTimeOffset updatedAtUtc)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        RomancePreferenceMode = mode;
        UpdatedAt = updatedAtUtc;
    }

    public int Reputation { get; private set; }

    public int Influence { get; private set; }

    public int FollowersCount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public long Version { get; private set; }
}
