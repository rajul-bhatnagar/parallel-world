using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using ParallelWorld.Domain.Relationships;

namespace ParallelWorld.Domain.Messaging;

public static class MessageReplyMechanics
{
    public const int Urgency = 50;
    public const int ConflictAvoidancePenalty = 0;

    public static int Score(RelationshipValues relationship, int sociability) => Math.Clamp(
        (int)Math.Round(35m + .20m * Urgency + .20m * relationship.Familiarity
            + .15m * relationship.Trust + .15m * relationship.Comfort
            + .10m * sociability - ConflictAvoidancePenalty,
            MidpointRounding.AwayFromZero), 0, 100);

    public static int Roll(Guid worldId, long worldSeed, int ruleVersion, Guid sourceMessageId,
        Guid characterActorId, Guid playerActorId)
    {
        var canonical = $"parallel-world.msg-02.eligibility.v1|{worldId:N}|{worldSeed}|{ruleVersion}|{sourceMessageId:N}|{characterActorId:N}|{playerActorId:N}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return (int)((ulong)(BinaryPrimitives.ReadInt64BigEndian(hash) & long.MaxValue) % 100UL);
    }
}
