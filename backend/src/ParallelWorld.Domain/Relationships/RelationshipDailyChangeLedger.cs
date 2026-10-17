namespace ParallelWorld.Domain.Relationships;

public sealed class RelationshipDailyChangeLedger
{
    private RelationshipDailyChangeLedger() { }
    public RelationshipDailyChangeLedger(Guid worldId, Guid sourceActorId, Guid targetActorId, DateOnly gameDate, DateTimeOffset now) { if (sourceActorId == targetActorId) throw new ArgumentException("Ledger actors must be distinct."); WorldId = worldId; SourceActorId = sourceActorId; TargetActorId = targetActorId; GameDate = gameDate; UpdatedAt = now; }
    public Guid WorldId { get; private set; }
    public Guid SourceActorId { get; private set; }
    public Guid TargetActorId { get; private set; }
    public DateOnly GameDate { get; private set; }
    public int Familiarity { get; private set; }
    public int Trust { get; private set; }
    public int Respect { get; private set; }
    public int Affection { get; private set; }
    public int Comfort { get; private set; }
    public int Rivalry { get; private set; }
    public int Jealousy { get; private set; }
    public int Attraction { get; private set; }
    public int Commitment { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public long Version { get; private set; }
    public bool SevereBypassUsed { get; private set; }
    public RelationshipValues Used => new(Familiarity, Trust, Respect, Affection, Comfort, Rivalry, Jealousy, Attraction, Commitment);
    public void Add(RelationshipValues delta, DateTimeOffset now) { var v = Used + delta.Absolute(); Familiarity = v.Familiarity; Trust = v.Trust; Respect = v.Respect; Affection = v.Affection; Comfort = v.Comfort; Rivalry = v.Rivalry; Jealousy = v.Jealousy; Attraction = v.Attraction; Commitment = v.Commitment; UpdatedAt = now; Version++; }
    public void UseSevereBypass(DateTimeOffset now) { if (SevereBypassUsed) throw new InvalidOperationException("The severe daily bypass was already used."); SevereBypassUsed = true; UpdatedAt = now; Version++; }
}
