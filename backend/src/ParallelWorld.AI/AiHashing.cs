using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace ParallelWorld.AI;

internal static class AiHashing
{
    public static string InputHash(AiCanonicalInput input, string templateVersion)
    {
        var fields = new List<(string Name, string? Value)>
        {
            ("template-version", templateVersion),
            ("text-kind", input.TextKind.ToString()),
            ("action-type", input.ActionType),
            ("actor-display-name", input.ActorDisplayName),
            ("target-display-name", input.TargetDisplayName),
            ("topic", input.Topic),
            ("visible-mood", input.VisibleMood),
            ("style-hint", input.StyleHint),
            ("stance", input.Stance),
            ("tone", input.Tone),
            ("factual-outcome", input.FactualOutcome),
            ("untrusted-context", input.UntrustedContext),
            ("sensitive-input", input.SensitiveInputDetected.ToString()),
            ("maximum-output-length", input.MaxOutputLength.ToString(System.Globalization.CultureInfo.InvariantCulture)),
        };
        fields.AddRange(input.ProhibitedClaims
            .Order(StringComparer.Ordinal)
            .Select((value, index) => ($"prohibited-{index}", (string?)value)));
        fields.AddRange(input.DuplicateTexts
            .Order(StringComparer.Ordinal)
            .Select((value, index) => ($"duplicate-{index}", (string?)value)));
        fields.AddRange(input.OtherActorDisplayNames
            .Order(StringComparer.Ordinal)
            .Select((value, index) => ($"other-actor-{index}", (string?)value)));
        return HashCanonical("parallel-world.ai.generation-input.v1", fields);
    }

    public static string TextHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    public static Guid GenerationId(Guid worldId, Guid actionId, string inputHash)
    {
        var hash = HashCanonical(
            "parallel-world.ai.generation-id.v1",
            [
                ("world-id", worldId.ToString("N")),
                ("action-id", actionId.ToString("N")),
                ("input-hash", inputHash),
            ]);
        var bytes = Convert.FromHexString(hash).AsSpan(0, 16).ToArray();
        bytes[7] = (byte)((bytes[7] & 0x0f) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        return new Guid(bytes);
    }

    private static string HashCanonical(
        string domain,
        IEnumerable<(string Name, string? Value)> fields)
    {
        using var canonical = new MemoryStream();
        WriteField(canonical, "domain", domain);
        foreach (var field in fields)
        {
            WriteField(canonical, field.Name, field.Value);
        }

        return Convert.ToHexStringLower(SHA256.HashData(canonical.ToArray()));
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
