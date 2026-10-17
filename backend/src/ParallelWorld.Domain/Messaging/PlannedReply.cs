namespace ParallelWorld.Domain.Messaging;

public sealed class PlannedReply
{
    private PlannedReply() { ReasonCode = string.Empty; }

    public PlannedReply(Guid id, Guid worldId, Guid conversationId, Guid sourceMessageId,
        Guid recipientActorId, int urgency, int conflictAvoidancePenalty, int replyScore,
        int eligibilityRoll, bool eligible, DateTimeOffset dueAt, DateTimeOffset createdAt)
    {
        if (urgency is < 0 or > 100 || conflictAvoidancePenalty < 0
            || replyScore is < 0 or > 100 || eligibilityRoll is < 0 or > 99)
            throw new ArgumentOutOfRangeException(nameof(replyScore));
        Id = id;
        WorldId = worldId;
        ConversationId = conversationId;
        SourceMessageId = sourceMessageId;
        RecipientActorId = recipientActorId;
        Urgency = urgency;
        ConflictAvoidancePenalty = conflictAvoidancePenalty;
        ReplyScore = replyScore;
        EligibilityRoll = eligibilityRoll;
        Status = eligible ? PlannedReplyStatus.Planned : PlannedReplyStatus.NoResponse;
        ReasonCode = eligible ? "msg_02_eligible" : "msg_02_roll_failed";
        DueAt = dueAt;
        CreatedAt = createdAt;
        if (!eligible) CompletedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid SourceMessageId { get; private set; }
    public Guid RecipientActorId { get; private set; }
    public int Urgency { get; private set; }
    public int ConflictAvoidancePenalty { get; private set; }
    public int ReplyScore { get; private set; }
    public int EligibilityRoll { get; private set; }
    public PlannedReplyStatus Status { get; private set; }
    public string ReasonCode { get; private set; }
    public DateTimeOffset DueAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public long Version { get; private set; }

    public void Complete(bool fallbackUsed, DateTimeOffset completedAt)
    {
        if (Status != PlannedReplyStatus.Planned) return;
        Status = fallbackUsed ? PlannedReplyStatus.FallbackCompleted : PlannedReplyStatus.Completed;
        ReasonCode = fallbackUsed ? "msg_02_fallback_completed" : "msg_02_completed";
        CompletedAt = completedAt;
        Version++;
    }
}

public enum PlannedReplyStatus { Planned, Completed, FallbackCompleted, NoResponse }
