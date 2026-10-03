namespace ParallelWorld.Domain.Relationships;

public readonly record struct RelationshipValues(int Familiarity, int Trust, int Respect, int Affection,
    int Comfort, int Rivalry, int Jealousy, int Attraction, int Commitment)
{
    public static RelationshipValues Initial { get; } = new(10, 50, 50, 20, 15, 0, 0, 0, 0);
    public static RelationshipValues Zero { get; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0);
    public RelationshipValues Clamp(int minimum = 0, int maximum = 100) => new(
        Math.Clamp(Familiarity, minimum, maximum), Math.Clamp(Trust, minimum, maximum), Math.Clamp(Respect, minimum, maximum),
        Math.Clamp(Affection, minimum, maximum), Math.Clamp(Comfort, minimum, maximum), Math.Clamp(Rivalry, minimum, maximum),
        Math.Clamp(Jealousy, minimum, maximum), Math.Clamp(Attraction, minimum, maximum), Math.Clamp(Commitment, minimum, maximum));
    public static RelationshipValues operator +(RelationshipValues l, RelationshipValues r) => new(
        l.Familiarity + r.Familiarity, l.Trust + r.Trust, l.Respect + r.Respect, l.Affection + r.Affection, l.Comfort + r.Comfort,
        l.Rivalry + r.Rivalry, l.Jealousy + r.Jealousy, l.Attraction + r.Attraction, l.Commitment + r.Commitment);
    public RelationshipValues Absolute() => new(Math.Abs(Familiarity), Math.Abs(Trust), Math.Abs(Respect), Math.Abs(Affection),
        Math.Abs(Comfort), Math.Abs(Rivalry), Math.Abs(Jealousy), Math.Abs(Attraction), Math.Abs(Commitment));
}
