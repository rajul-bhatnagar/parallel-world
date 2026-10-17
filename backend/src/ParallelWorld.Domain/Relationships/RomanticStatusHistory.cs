namespace ParallelWorld.Domain.Relationships;

public sealed class RomanticStatusHistory
{
    private RomanticStatusHistory() { ReasonCode = IdempotencyKey = string.Empty; }

    public RomanticStatusHistory(
        Guid id, Guid worldId, Guid romanticRelationshipId, Guid episodeId,
        Guid? romanticInvitationId, RomanticStatus fromStatus, RomanticStatus toStatus,
        Guid? initiatorActorId, string reasonCode, DateTimeOffset occurredAtUtc,
        DateTimeOffset occurredAtWorldTime, int ruleVersion, string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        Id = id;
        WorldId = worldId;
        RomanticRelationshipId = romanticRelationshipId;
        EpisodeId = episodeId;
        RomanticInvitationId = romanticInvitationId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        InitiatorActorId = initiatorActorId;
        ReasonCode = reasonCode;
        OccurredAtUtc = occurredAtUtc;
        OccurredAtWorldTime = occurredAtWorldTime;
        RuleVersion = ruleVersion;
        IdempotencyKey = idempotencyKey;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid RomanticRelationshipId { get; private set; }
    public Guid EpisodeId { get; private set; }
    public Guid? RomanticInvitationId { get; private set; }
    public RomanticStatus FromStatus { get; private set; }
    public RomanticStatus ToStatus { get; private set; }
    public Guid? InitiatorActorId { get; private set; }
    public string ReasonCode { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset OccurredAtWorldTime { get; private set; }
    public int RuleVersion { get; private set; }
    public string IdempotencyKey { get; private set; }
}
