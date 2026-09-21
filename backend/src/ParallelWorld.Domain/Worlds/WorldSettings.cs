namespace ParallelWorld.Domain.Worlds;

public sealed class WorldSettings
{
    private WorldSettings()
    {
        ContentSettingsJson = "{}";
        DisplayTimeZoneId = UtcTimeZoneId;
    }

    public WorldSettings(Guid id, Guid worldId, DateTimeOffset createdAt)
    {
        Id = id;
        WorldId = worldId;
        TimeScale = 1m;
        DisplayTimeZoneId = UtcTimeZoneId;
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

    public string DisplayTimeZoneId { get; private set; }

    public int ActionLimit { get; private set; }

    public int AiBudgetTokens { get; private set; }

    public string ContentSettingsJson { get; private set; }

    public int RuleVersion { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public long Version { get; private set; }

    public void SetTimeScale(decimal timeScale, DateTimeOffset updatedAtUtc)
    {
        if (timeScale is < MinimumTimeScale or > MaximumTimeScale
            || decimal.Round(timeScale, 4, MidpointRounding.ToEven) != timeScale)
        {
            throw new ArgumentOutOfRangeException(nameof(timeScale));
        }

        TimeScale = timeScale;
        UpdatedAt = updatedAtUtc;
    }

    public void SetDisplayTimeZoneId(string displayTimeZoneId, DateTimeOffset updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayTimeZoneId);
        if (!IsSupportedIanaTimeZone(displayTimeZoneId))
        {
            throw new ArgumentException(
                "The display timezone must be a supported IANA identifier.",
                nameof(displayTimeZoneId));
        }

        DisplayTimeZoneId = displayTimeZoneId;
        UpdatedAt = updatedAtUtc;
    }

    public static bool IsSupportedIanaTimeZone(string displayTimeZoneId)
    {
        if (string.Equals(displayTimeZoneId, UtcTimeZoneId, StringComparison.Ordinal))
        {
            return true;
        }

        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(displayTimeZoneId, out _))
        {
            return false;
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(displayTimeZoneId);
            return displayTimeZoneId.Contains('/', StringComparison.Ordinal);
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }

    public const string UtcTimeZoneId = "UTC";
    public const decimal MinimumTimeScale = 0.0001m;
    public const decimal MaximumTimeScale = 9999.9999m;
}
