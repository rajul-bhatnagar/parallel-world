using ParallelWorld.Domain.Relationships;

namespace ParallelWorld.Domain.Memory;

public static class MemoryMechanics
{
    public const int MaximumActiveMemories = 100;
    public const int MaximumRecallCount = 8;
    public const string ProtectedCapacityReason = "memory_capacity_protected";

    public static int Importance(MemoryType type) => type switch
    {
        MemoryType.Fact => 60,
        MemoryType.Preference => 60,
        MemoryType.Event => 50,
        MemoryType.Secret => 90,
        MemoryType.Promise => 90,
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    public static int Confidence(MemoryAuthorityType authority) => authority switch
    {
        MemoryAuthorityType.StructuredGameplayFact => 100,
        MemoryAuthorityType.StructuredPlayerStatement => 90,
        MemoryAuthorityType.StructuredPreference => 100,
        MemoryAuthorityType.GameplayEvent => 90,
        MemoryAuthorityType.StructuredSecret => 100,
        MemoryAuthorityType.StructuredPromise => 100,
        _ => throw new ArgumentOutOfRangeException(nameof(authority)),
    };

    public static bool IsApprovedMapping(MemoryType type, MemoryAuthorityType authority) =>
        (type, authority) switch
        {
            (MemoryType.Fact, MemoryAuthorityType.StructuredGameplayFact) => true,
            (MemoryType.Fact, MemoryAuthorityType.StructuredPlayerStatement) => true,
            (MemoryType.Preference, MemoryAuthorityType.StructuredPreference) => true,
            (MemoryType.Event, MemoryAuthorityType.GameplayEvent) => true,
            (MemoryType.Secret, MemoryAuthorityType.StructuredSecret) => true,
            (MemoryType.Promise, MemoryAuthorityType.StructuredPromise) => true,
            _ => false,
        };

    public static bool IsProtected(MemoryType type) =>
        type is MemoryType.Secret or MemoryType.Promise;

    public static int RecallScore(
        int subjectMatch,
        int topicMatch,
        int relationshipMatch,
        int importance)
    {
        ValidateScore(subjectMatch, nameof(subjectMatch));
        ValidateScore(topicMatch, nameof(topicMatch));
        ValidateScore(relationshipMatch, nameof(relationshipMatch));
        ValidateScore(importance, nameof(importance));

        return (int)Math.Round(
            0.40m * subjectMatch
            + 0.30m * topicMatch
            + 0.20m * relationshipMatch
            + 0.10m * importance,
            MidpointRounding.AwayFromZero);
    }

    public static int RelationshipMatch(RelationshipValues values) =>
        RelationshipMechanics.Relevance(values);

    private static void ValidateScore(int value, string name)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(name);
        }
    }
}
