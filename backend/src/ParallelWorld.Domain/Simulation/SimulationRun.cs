using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Domain.Simulation;

public sealed class SimulationRun
{
    private SimulationRun()
    {
        IdempotencyKey = string.Empty;
    }

    public SimulationRun(
        Guid id,
        Guid worldId,
        SimulationRunType runType,
        DateTimeOffset intervalStart,
        DateTimeOffset intervalEnd,
        decimal effectiveTimeScale,
        long seed,
        int ruleVersion,
        DateTimeOffset startedAt,
        string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (intervalEnd <= intervalStart)
        {
            throw new ArgumentException("The simulation interval end must follow its start.");
        }

        if (runType == SimulationRunType.ActiveTick
            && intervalEnd - intervalStart != TimeSpan.FromMinutes(15))
        {
            throw new ArgumentException("An active tick must cover exactly 15 minutes.");
        }

        if (effectiveTimeScale is < 0.0001m or > 9999.9999m
            || decimal.Round(effectiveTimeScale, 4, MidpointRounding.ToEven) != effectiveTimeScale)
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveTimeScale));
        }

        Id = id;
        WorldId = worldId;
        RunType = runType;
        IntervalStart = intervalStart;
        IntervalEnd = intervalEnd;
        ProcessedThrough = intervalStart;
        RequestedIntervalCount = checked((int)((intervalEnd - intervalStart).Ticks
            / WorldSimulationState.ActiveIntervalDuration.Ticks));
        if (RequestedIntervalCount <= 0
            || intervalStart.AddTicks(
                RequestedIntervalCount * WorldSimulationState.ActiveIntervalDuration.Ticks) != intervalEnd)
        {
            throw new ArgumentException("A simulation run must contain complete 15-minute logical intervals.");
        }
        RemainingIntervalCount = RequestedIntervalCount;
        EffectiveTimeScale = effectiveTimeScale;
        Seed = seed;
        RuleVersion = ruleVersion;
        Status = SimulationRunStatus.Running;
        StartedAt = startedAt;
        IdempotencyKey = idempotencyKey;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public SimulationRunType RunType { get; private set; }
    public DateTimeOffset IntervalStart { get; private set; }
    public DateTimeOffset IntervalEnd { get; private set; }
    public DateTimeOffset ProcessedThrough { get; private set; }
    public int RequestedIntervalCount { get; private set; }
    public int ProcessedIntervalCount { get; private set; }
    public int RemainingIntervalCount { get; private set; }
    public decimal EffectiveTimeScale { get; private set; }
    public long Seed { get; private set; }
    public int RuleVersion { get; private set; }
    public SimulationRunStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string IdempotencyKey { get; private set; }
    public string? ErrorCode { get; private set; }
    public Guid? LeaseOwnerId { get; private set; }
    public DateTimeOffset? LeaseExpiresAt { get; private set; }
    public int AttemptCount { get; private set; }
    public long Version { get; private set; }

    public void Complete(DateTimeOffset completedAt)
    {
        if (Status == SimulationRunStatus.Completed)
        {
            return;
        }

        if (Status != SimulationRunStatus.Running)
        {
            throw new InvalidOperationException("Only a running simulation can complete.");
        }

        ProcessedThrough = IntervalEnd;
        ProcessedIntervalCount = RequestedIntervalCount;
        RemainingIntervalCount = 0;
        Status = SimulationRunStatus.Completed;
        CompletedAt = completedAt;
        ErrorCode = null;
        LeaseOwnerId = null;
        LeaseExpiresAt = null;
    }

    public bool TryClaim(Guid ownerId, DateTimeOffset claimedAt, TimeSpan leaseDuration)
    {
        if (RunType != SimulationRunType.CatchUp || leaseDuration <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Only CatchUp runs support bounded processing leases.");
        }

        if (Status == SimulationRunStatus.Completed || Status == SimulationRunStatus.FailedTerminal)
        {
            return false;
        }

        if (LeaseOwnerId is not null && LeaseExpiresAt > claimedAt && LeaseOwnerId != ownerId)
        {
            return false;
        }

        LeaseOwnerId = ownerId;
        LeaseExpiresAt = claimedAt.Add(leaseDuration);
        AttemptCount = checked(AttemptCount + 1);
        Status = SimulationRunStatus.Running;
        CompletedAt = null;
        ErrorCode = null;
        Version++;
        return true;
    }

    public void RenewLease(Guid ownerId, DateTimeOffset renewedAt, TimeSpan leaseDuration)
    {
        EnsureLeaseOwner(ownerId);
        LeaseExpiresAt = renewedAt.Add(leaseDuration);
        Version++;
    }

    public void AdvanceCatchUp(
        Guid ownerId,
        DateTimeOffset bucketStart,
        DateTimeOffset bucketEnd)
    {
        EnsureLeaseOwner(ownerId);
        if (RunType != SimulationRunType.CatchUp || Status != SimulationRunStatus.Running)
        {
            throw new InvalidOperationException("Only a running CatchUp run can advance a bucket.");
        }
        if (bucketStart != ProcessedThrough || bucketEnd <= bucketStart || bucketEnd > IntervalEnd)
        {
            throw new InvalidOperationException("The CatchUp bucket must continue from the committed checkpoint.");
        }

        var intervalCount = checked((int)((bucketEnd - bucketStart).Ticks
            / WorldSimulationState.ActiveIntervalDuration.Ticks));
        if (intervalCount <= 0
            || bucketStart.AddTicks(
                intervalCount * WorldSimulationState.ActiveIntervalDuration.Ticks) != bucketEnd)
        {
            throw new ArgumentException("A CatchUp bucket must contain complete 15-minute intervals.");
        }

        ProcessedThrough = bucketEnd;
        ProcessedIntervalCount = checked(ProcessedIntervalCount + intervalCount);
        RemainingIntervalCount = RequestedIntervalCount - ProcessedIntervalCount;
        Version++;
    }

    public void MarkPartial(Guid ownerId, DateTimeOffset stoppedAt)
    {
        EnsureLeaseOwner(ownerId);
        if (RunType != SimulationRunType.CatchUp || ProcessedThrough >= IntervalEnd)
        {
            throw new InvalidOperationException("Only an incomplete CatchUp run can become Partial.");
        }

        Status = SimulationRunStatus.Partial;
        CompletedAt = stoppedAt;
        ErrorCode = null;
        LeaseOwnerId = null;
        LeaseExpiresAt = null;
        Version++;
    }

    public void CompleteCatchUp(Guid ownerId, DateTimeOffset completedAt)
    {
        EnsureLeaseOwner(ownerId);
        if (RunType != SimulationRunType.CatchUp || ProcessedThrough != IntervalEnd)
        {
            throw new InvalidOperationException("A CatchUp run can complete only at its requested boundary.");
        }

        Complete(completedAt);
        Version++;
    }

    public void FailCatchUpRetryable(Guid ownerId, string errorCode, DateTimeOffset failedAt)
    {
        EnsureLeaseOwner(ownerId);
        Fail(SimulationRunStatus.FailedRetryable, errorCode, failedAt);
        LeaseOwnerId = null;
        LeaseExpiresAt = null;
        Version++;
    }

    public void Fail(
        SimulationRunStatus failureStatus,
        string errorCode,
        DateTimeOffset failedAt)
    {
        if (failureStatus is not SimulationRunStatus.FailedRetryable
            and not SimulationRunStatus.FailedTerminal)
        {
            throw new ArgumentOutOfRangeException(nameof(failureStatus));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        if (errorCode.Length > 80
            || errorCode.Any(character => !char.IsAsciiLetterLower(character)
                && !char.IsAsciiDigit(character)
                && character != '_'))
        {
            throw new ArgumentException(
                "A simulation error code must be lowercase ASCII and no more than 80 characters.",
                nameof(errorCode));
        }

        if (Status != SimulationRunStatus.Running)
        {
            throw new InvalidOperationException("Only a running simulation can fail.");
        }

        Status = failureStatus;
        CompletedAt = failedAt;
        ErrorCode = errorCode;
    }

    private void EnsureLeaseOwner(Guid ownerId)
    {
        if (LeaseOwnerId != ownerId)
        {
            throw new InvalidOperationException("The caller does not own the CatchUp processing lease.");
        }
    }
}

public enum SimulationRunType
{
    ActiveTick,
    CatchUp,
}

public enum SimulationRunStatus
{
    Pending,
    Running,
    Partial,
    Completed,
    FailedRetryable,
    FailedTerminal,
}
