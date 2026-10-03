namespace ParallelWorld.Domain.Relationships;

public sealed class RelationshipEvent
{
    private RelationshipEvent() { ReasonCode = IdempotencyKey = EventType = BeforeValues = AfterValues = string.Empty; }
    public RelationshipEvent(Guid id, Guid worldId, Guid relationshipId, Guid sourceActorId, Guid targetActorId, Guid gameplayEventId, string eventType, RelationshipValues baseDelta, RelationshipValues finalDelta, string beforeValues, string afterValues, string reasonCode, DateTimeOffset occurredAt, DateOnly occurredGameDate, bool isQualifiedNegative, int ruleVersion, string idempotencyKey)
    { if (sourceActorId == targetActorId) throw new ArgumentException("Relationship event actors must be distinct."); ArgumentException.ThrowIfNullOrWhiteSpace(eventType); ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode); ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey); Id = id; WorldId = worldId; RelationshipId = relationshipId; SourceActorId = sourceActorId; TargetActorId = targetActorId; GameplayEventId = gameplayEventId; EventType = eventType; SetBase(baseDelta); SetFinal(finalDelta); BeforeValues = beforeValues; AfterValues = afterValues; ReasonCode = reasonCode; OccurredAt = occurredAt; OccurredGameDate = occurredGameDate; IsQualifiedNegative = isQualifiedNegative; RuleVersion = ruleVersion; IdempotencyKey = idempotencyKey; }
    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid RelationshipId { get; private set; }
    public Guid SourceActorId { get; private set; }
    public Guid TargetActorId { get; private set; }
    public Guid GameplayEventId { get; private set; }
    public string EventType { get; private set; }
    public int BaseFamiliarity { get; private set; }
    public int BaseTrust { get; private set; }
    public int BaseRespect { get; private set; }
    public int BaseAffection { get; private set; }
    public int BaseComfort { get; private set; }
    public int BaseRivalry { get; private set; }
    public int BaseJealousy { get; private set; }
    public int BaseAttraction { get; private set; }
    public int BaseCommitment { get; private set; }
    public int FamiliarityDelta { get; private set; }
    public int TrustDelta { get; private set; }
    public int RespectDelta { get; private set; }
    public int AffectionDelta { get; private set; }
    public int ComfortDelta { get; private set; }
    public int RivalryDelta { get; private set; }
    public int JealousyDelta { get; private set; }
    public int AttractionDelta { get; private set; }
    public int CommitmentDelta { get; private set; }
    public string BeforeValues { get; private set; }
    public string AfterValues { get; private set; }
    public string ReasonCode { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateOnly OccurredGameDate { get; private set; }
    public bool IsQualifiedNegative { get; private set; }
    public int RuleVersion { get; private set; }
    public string IdempotencyKey { get; private set; }
    private void SetBase(RelationshipValues v) { BaseFamiliarity = v.Familiarity; BaseTrust = v.Trust; BaseRespect = v.Respect; BaseAffection = v.Affection; BaseComfort = v.Comfort; BaseRivalry = v.Rivalry; BaseJealousy = v.Jealousy; BaseAttraction = v.Attraction; BaseCommitment = v.Commitment; }
    private void SetFinal(RelationshipValues v) { FamiliarityDelta = v.Familiarity; TrustDelta = v.Trust; RespectDelta = v.Respect; AffectionDelta = v.Affection; ComfortDelta = v.Comfort; RivalryDelta = v.Rivalry; JealousyDelta = v.Jealousy; AttractionDelta = v.Attraction; CommitmentDelta = v.Commitment; }
}
