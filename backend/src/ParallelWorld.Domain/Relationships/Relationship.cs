namespace ParallelWorld.Domain.Relationships;

public sealed class Relationship
{
    private Relationship() { }
    public Relationship(Guid id, Guid worldId, Guid sourceActorId, Guid targetActorId, DateTimeOffset createdAt)
    { if (sourceActorId == targetActorId) throw new ArgumentException("A relationship must be directional between distinct actors."); Id = id; WorldId = worldId; SourceActorId = sourceActorId; TargetActorId = targetActorId; Set(RelationshipValues.Initial); CreatedAt = UpdatedAt = createdAt; }
    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid SourceActorId { get; private set; }
    public Guid TargetActorId { get; private set; }
    public int Familiarity { get; private set; }
    public int Trust { get; private set; }
    public int Respect { get; private set; }
    public int Affection { get; private set; }
    public int Comfort { get; private set; }
    public int Rivalry { get; private set; }
    public int Jealousy { get; private set; }
    public int Attraction { get; private set; }
    public int Commitment { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public long Version { get; private set; }
    public RelationshipValues Values => new(Familiarity, Trust, Respect, Affection, Comfort, Rivalry, Jealousy, Attraction, Commitment);
    public RelationshipValues Apply(RelationshipValues delta, DateTimeOffset at) { var before = Values; Set((before + delta).Clamp()); UpdatedAt = at; Version++; return before; }
    private void Set(RelationshipValues v) { Familiarity = v.Familiarity; Trust = v.Trust; Respect = v.Respect; Affection = v.Affection; Comfort = v.Comfort; Rivalry = v.Rivalry; Jealousy = v.Jealousy; Attraction = v.Attraction; Commitment = v.Commitment; }
}
