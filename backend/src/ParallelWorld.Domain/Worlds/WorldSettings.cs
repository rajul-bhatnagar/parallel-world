namespace ParallelWorld.Domain.Worlds;

public sealed class WorldSettings
{
    private WorldSettings()
    {
        ContentSettingsJson = "{}";
    }

    public WorldSettings(Guid id, Guid worldId, DateTimeOffset createdAt)
    {
        Id = id;
        WorldId = worldId;
        TimeScale = 1m;
        ActionLimit = 0;
        AiBudgetTokens = 0;
        ContentSettingsJson = "{}";
        RuleVersion = 1;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid WorldId { get; private set; }

    public decimal TimeScale { get; private set; }

    public int ActionLimit { get; private set; }

    public int AiBudgetTokens { get; private set; }

    public string ContentSettingsJson { get; private set; }

    public int RuleVersion { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public long Version { get; private set; }
}
