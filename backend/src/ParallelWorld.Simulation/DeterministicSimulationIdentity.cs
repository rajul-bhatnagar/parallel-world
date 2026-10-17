using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ParallelWorld.Simulation;

public static class DeterministicSimulationIdentity
{
    public static Guid CreateRunId(
        Guid worldId,
        DateTimeOffset intervalStartUtc,
        DateTimeOffset intervalEndUtc,
        int ruleVersion) =>
        CreateGuid(FormattableString.Invariant(
            $"m08|run|{worldId:N}|{intervalStartUtc:O}|{intervalEndUtc:O}|{ruleVersion}"));

    public static Guid CreateCatchUpRunId(
        Guid worldId,
        DateTimeOffset intervalStartUtc,
        DateTimeOffset intervalEndUtc,
        int ruleVersion) =>
        CreateGuid(FormattableString.Invariant(
            $"m15|catchup-run|{worldId:N}|{intervalStartUtc:O}|{intervalEndUtc:O}|{ruleVersion}"));

    public static Guid CreateCatchUpSummaryId(Guid worldId, Guid runId) =>
        CreateGuid(FormattableString.Invariant($"m15|summary|{worldId:N}|{runId:N}"));

    public static Guid CreateCatchUpSummaryItemId(
        Guid worldId,
        Guid runId,
        int ordinal,
        Guid gameplayEventId) =>
        CreateGuid(FormattableString.Invariant(
            $"m15|summary-item|{worldId:N}|{runId:N}|{ordinal}|{gameplayEventId:N}"));

    public static Guid CreateCatchUpEventId(
        Guid worldId,
        Guid runId,
        int bucketOrdinal,
        Guid sourceActorId,
        Guid targetActorId,
        bool desiredFollowing) =>
        CreateGuid(FormattableString.Invariant(
            $"m15|event|{worldId:N}|{runId:N}|{bucketOrdinal}|{sourceActorId:N}|{targetActorId:N}|{desiredFollowing}"));

    public static Guid CreateCatchUpFollowId(
        Guid worldId,
        Guid runId,
        int bucketOrdinal,
        Guid sourceActorId,
        Guid targetActorId) =>
        CreateGuid(FormattableString.Invariant(
            $"m15|follow|{worldId:N}|{runId:N}|{bucketOrdinal}|{sourceActorId:N}|{targetActorId:N}"));

    public static Guid CreateEvaluationId(Guid worldId, Guid runId, string ruleCode) =>
        CreateGuid(FormattableString.Invariant(
            $"m08|evaluation|{worldId:N}|{runId:N}|{ruleCode}"));

    public static Guid CreateCheckpointId(Guid worldId, Guid runId, int ordinal) =>
        CreateGuid(FormattableString.Invariant(
            $"m08|checkpoint|{worldId:N}|{runId:N}|{ordinal}"));

    public static Guid CreateActionId(Guid worldId, Guid runId, int ordinal) =>
        CreateGuid(FormattableString.Invariant(
            $"m08|action|{worldId:N}|{runId:N}|{ordinal}"));

    public static long CreateRunSeed(
        long worldSeed,
        int ruleVersion,
        DateTimeOffset intervalStartUtc,
        DateTimeOffset intervalEndUtc,
        long runOrdinal)
    {
        if (intervalEndUtc <= intervalStartUtc)
        {
            throw new ArgumentException("The simulation interval end must follow its start.");
        }

        if (runOrdinal < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(runOrdinal));
        }

        return ReadPositiveInt64(HashCanonical(
            "parallel-world.simulation.run-seed.v1",
            ("world-seed", worldSeed.ToString(CultureInfo.InvariantCulture)),
            ("rule-version", ruleVersion.ToString(CultureInfo.InvariantCulture)),
            ("interval-start-utc", intervalStartUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)),
            ("interval-end-utc", intervalEndUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)),
            ("run-ordinal", runOrdinal.ToString(CultureInfo.InvariantCulture))));
    }

    public static long CreateBucketSeed(long runSeed, Guid runId, int bucketOrdinal) =>
        ReadPositiveInt64(HashCanonical(
            "parallel-world.simulation.catchup-bucket-seed.v1",
            ("run-seed", runSeed.ToString(CultureInfo.InvariantCulture)),
            ("run-id", runId.ToString("N")),
            ("bucket-ordinal", bucketOrdinal.ToString(CultureInfo.InvariantCulture))));

    internal static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));

    internal static byte[] HashCanonical(
        string domain,
        params (string Name, string? Value)[] fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        using var canonical = new MemoryStream();
        WriteField(canonical, "domain", domain);
        foreach (var field in fields)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(field.Name);
            WriteField(canonical, field.Name, field.Value);
        }

        return SHA256.HashData(canonical.ToArray());
    }

    internal static long ReadPositiveInt64(byte[] hash) =>
        BinaryPrimitives.ReadInt64BigEndian(hash) & long.MaxValue;

    private static Guid CreateGuid(string value)
    {
        var bytes = Hash(value).AsSpan(0, 16).ToArray();
        bytes[7] = (byte)((bytes[7] & 0x0f) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        return new Guid(bytes);
    }

    private static void WriteField(Stream destination, string name, string? value)
    {
        WriteLengthPrefixed(destination, Encoding.UTF8.GetBytes(name));
        if (value is null)
        {
            Span<byte> nullLength = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(nullLength, -1);
            destination.Write(nullLength);
            return;
        }

        WriteLengthPrefixed(destination, Encoding.UTF8.GetBytes(value));
    }

    private static void WriteLengthPrefixed(Stream destination, byte[] value)
    {
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(length, value.Length);
        destination.Write(length);
        destination.Write(value);
    }
}
