namespace ParallelWorld.Domain.Relationships;

public enum RomanticInvitationStatus
{
    Pending,
    Accepted,
    Rejected,
    Expired,
}

public sealed class RomanticInvitation
{
    private RomanticInvitation() { IdempotencyKey = DateType = ReasonCode = string.Empty; }

    public RomanticInvitation(
        Guid id, Guid worldId, Guid romanticRelationshipId, Guid episodeId,
        Guid initiatorActorId, Guid targetActorId, int initiationScore,
        DateTimeOffset createdAtUtc, DateTimeOffset createdAtWorldTime,
        int ruleVersion, string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (initiatorActorId == targetActorId) throw new ArgumentException("An invitation requires distinct actors.");
        Id = id;
        WorldId = worldId;
        RomanticRelationshipId = romanticRelationshipId;
        EpisodeId = episodeId;
        InitiatorActorId = initiatorActorId;
        TargetActorId = targetActorId;
        DateType = "CasualDate";
        Status = RomanticInvitationStatus.Pending;
        InitiationScore = initiationScore;
        ReasonCode = "invitation_pending";
        CreatedAtUtc = createdAtUtc;
        CreatedAtWorldTime = createdAtWorldTime;
        ExpiresAtWorldTime = createdAtWorldTime.Add(RomanceMechanics.InvitationLifetime);
        RuleVersion = ruleVersion;
        IdempotencyKey = idempotencyKey;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid RomanticRelationshipId { get; private set; }
    public Guid EpisodeId { get; private set; }
    public Guid InitiatorActorId { get; private set; }
    public Guid TargetActorId { get; private set; }
    public string DateType { get; private set; }
    public RomanticInvitationStatus Status { get; private set; }
    public int InitiationScore { get; private set; }
    public int? AcceptanceScore { get; private set; }
    public int? SeededOffset { get; private set; }
    public string ReasonCode { get; private set; }
    public bool CommitmentApplied { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtWorldTime { get; private set; }
    public DateTimeOffset ExpiresAtWorldTime { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    public DateTimeOffset? ResolvedAtWorldTime { get; private set; }
    public int RuleVersion { get; private set; }
    public string IdempotencyKey { get; private set; }
    public long Version { get; private set; }

    public void Accept(int? score, int? offset, string reason, DateTimeOffset utcNow, DateTimeOffset worldTime)
        => Resolve(RomanticInvitationStatus.Accepted, score, offset, reason, utcNow, worldTime);

    public void Reject(int? score, int? offset, string reason, DateTimeOffset utcNow, DateTimeOffset worldTime)
        => Resolve(RomanticInvitationStatus.Rejected, score, offset, reason, utcNow, worldTime);

    public void Expire(DateTimeOffset utcNow, DateTimeOffset worldTime)
        => Resolve(RomanticInvitationStatus.Expired, null, null, RomanceReasonCodes.InvitationExpired, utcNow, worldTime);

    public void MarkCommitmentApplied()
    {
        if (Status != RomanticInvitationStatus.Accepted) throw new InvalidOperationException("Only an accepted invitation can apply Commitment.");
        if (CommitmentApplied) return;
        CommitmentApplied = true;
        Version++;
    }

    private void Resolve(RomanticInvitationStatus status, int? score, int? offset, string reason, DateTimeOffset utcNow, DateTimeOffset worldTime)
    {
        if (Status != RomanticInvitationStatus.Pending) throw new InvalidOperationException("The invitation already has a terminal outcome.");
        Status = status;
        AcceptanceScore = score;
        SeededOffset = offset;
        ReasonCode = reason;
        ResolvedAtUtc = utcNow;
        ResolvedAtWorldTime = worldTime;
        Version++;
    }
}
