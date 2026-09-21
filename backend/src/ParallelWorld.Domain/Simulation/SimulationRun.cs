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
    public decimal EffectiveTimeScale { get; private set; }
    public long Seed { get; private set; }
    public int RuleVersion { get; private set; }
    public SimulationRunStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string IdempotencyKey { get; private set; }
    public string? ErrorCode { get; private set; }
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
        Status = SimulationRunStatus.Completed;
        CompletedAt = completedAt;
        ErrorCode = null;
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
