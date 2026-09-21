using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Simulation;

public interface IWorldTimeProjector
{
    DateTimeOffset ToLocalWorldTime(
        DateTimeOffset currentWorldTimeUtc,
        string displayTimeZoneId);
}

public sealed class WorldTimeProjector : IWorldTimeProjector
{
    public DateTimeOffset ToLocalWorldTime(
        DateTimeOffset currentWorldTimeUtc,
        string displayTimeZoneId)
    {
        if (currentWorldTimeUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("World time must be a UTC instant.", nameof(currentWorldTimeUtc));
        }

        if (!WorldSettings.IsSupportedIanaTimeZone(displayTimeZoneId))
        {
            throw new ArgumentException(
                "The display timezone must be a supported IANA identifier.",
                nameof(displayTimeZoneId));
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(displayTimeZoneId);
        return TimeZoneInfo.ConvertTime(currentWorldTimeUtc, timeZone);
    }
}
