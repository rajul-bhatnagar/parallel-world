using System.Globalization;

namespace ParallelWorld.Simulation;

public readonly record struct DeterministicChoiceCoordinates(
    long RunSeed,
    string RuleId,
    Guid? ActorId,
    Guid? TargetId,
    Guid? TopicId,
    int StableOrdinal);

public interface IDeterministicRandomProvider
{
    int NextInt(
        DeterministicChoiceCoordinates coordinates,
        int inclusiveMinimum,
        int exclusiveMaximum);
}

public sealed class DeterministicRandomProvider : IDeterministicRandomProvider
{
    public long CreateChoiceSeed(DeterministicChoiceCoordinates coordinates)
    {
        if (coordinates.RunSeed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(coordinates));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(coordinates.RuleId);
        if (coordinates.StableOrdinal < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(coordinates));
        }

        return DeterministicSimulationIdentity.ReadPositiveInt64(
            DeterministicSimulationIdentity.HashCanonical(
                "parallel-world.simulation.choice-seed.v1",
                ("run-seed", coordinates.RunSeed.ToString(CultureInfo.InvariantCulture)),
                ("rule-id", coordinates.RuleId),
                ("actor-id", CanonicalGuid(coordinates.ActorId)),
                ("target-id", CanonicalGuid(coordinates.TargetId)),
                ("topic-id", CanonicalGuid(coordinates.TopicId)),
                ("stable-ordinal", coordinates.StableOrdinal.ToString(CultureInfo.InvariantCulture))));
    }

    public int NextInt(
        DeterministicChoiceCoordinates coordinates,
        int inclusiveMinimum,
        int exclusiveMaximum)
    {
        if (exclusiveMaximum <= inclusiveMinimum)
        {
            throw new ArgumentOutOfRangeException(nameof(exclusiveMaximum));
        }

        var choiceSeed = (ulong)CreateChoiceSeed(coordinates);
        var range = (ulong)((long)exclusiveMaximum - inclusiveMinimum);
        return (int)(inclusiveMinimum + (long)(choiceSeed % range));
    }

    private static string? CanonicalGuid(Guid? value) => value?.ToString("N");
}
