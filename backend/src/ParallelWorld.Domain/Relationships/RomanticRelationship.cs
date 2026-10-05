namespace ParallelWorld.Domain.Relationships;

public enum RomanticStatus
{
    None,
    RomanticInterest,
    InvitationPending,
    Dating,
    FormerPartner,
}

public sealed class RomanticRelationship
{
    private RomanticRelationship() { }

    public RomanticRelationship(Guid id, Guid worldId, Guid firstActorId, Guid secondActorId, DateTimeOffset worldTime, DateTimeOffset utcNow)
    {
        if (firstActorId == secondActorId) throw new ArgumentException("A romantic pair requires distinct actors.");
        Id = id;
        WorldId = worldId;
        (ActorAId, ActorBId) = Canonical(firstActorId, secondActorId);
        Status = RomanticStatus.None;
        StatusSinceWorldTime = worldTime;
        UpdatedAtUtc = utcNow;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid ActorAId { get; private set; }
    public Guid ActorBId { get; private set; }
    public RomanticStatus Status { get; private set; }
    public DateTimeOffset StatusSinceWorldTime { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public long Version { get; private set; }

    public void MarkInvitationPending(DateTimeOffset worldTime, DateTimeOffset utcNow)
    {
        if (Status is not RomanticStatus.None and not RomanticStatus.RomanticInterest)
            throw new InvalidOperationException("The romantic pair cannot enter InvitationPending from its current state.");
        Transition(RomanticStatus.InvitationPending, worldTime, utcNow);
    }

    public void RejectOrExpire(DateTimeOffset worldTime, DateTimeOffset utcNow)
    {
        if (Status != RomanticStatus.InvitationPending) throw new InvalidOperationException("Only a pending invitation can resolve without Dating.");
        Transition(RomanticStatus.None, worldTime, utcNow);
    }

    public void StartDating(DateTimeOffset worldTime, DateTimeOffset utcNow)
    {
        if (Status != RomanticStatus.InvitationPending) throw new InvalidOperationException("Only a pending invitation can start Dating.");
        Transition(RomanticStatus.Dating, worldTime, utcNow);
    }

    public static (Guid ActorAId, Guid ActorBId) Canonical(Guid left, Guid right) =>
        left.CompareTo(right) < 0 ? (left, right) : (right, left);

    private void Transition(RomanticStatus status, DateTimeOffset worldTime, DateTimeOffset utcNow)
    {
        Status = status;
        StatusSinceWorldTime = worldTime;
        UpdatedAtUtc = utcNow;
        Version++;
    }
}
