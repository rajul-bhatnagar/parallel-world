using System.Globalization;

namespace ParallelWorld.Domain.Simulation;

public sealed class AiGenerationRequest
{
    private AiGenerationRequest()
    {
        Provider = string.Empty;
        Model = string.Empty;
        InputHash = string.Empty;
        OutputHash = string.Empty;
        PromptTemplateVersion = string.Empty;
        FinalizedText = string.Empty;
        IdempotencyKey = string.Empty;
    }

    public AiGenerationRequest(
        Guid id,
        Guid worldId,
        Guid simulationActionId,
        string provider,
        string model,
        int attemptCount,
        string inputHash,
        string outputHash,
        int? promptTokenCount,
        int? outputTokenCount,
        long latencyMilliseconds,
        string? failureCode,
        bool fallbackUsed,
        string promptTemplateVersion,
        string finalizedText,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc,
        string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(promptTemplateVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(finalizedText);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (attemptCount is < 0 or > 2)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptCount));
        }

        if (inputHash.Length != 64 || outputHash.Length != 64)
        {
            throw new ArgumentException("AI generation hashes must be lowercase SHA-256 hex values.");
        }

        if (!IsLowercaseHex(inputHash) || !IsLowercaseHex(outputHash))
        {
            throw new ArgumentException("AI generation hashes must be lowercase SHA-256 hex values.");
        }

        if (promptTokenCount < 0 || outputTokenCount < 0 || latencyMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(latencyMilliseconds));
        }

        if (failureCode is not null
            && (failureCode.Length > 80
                || failureCode.Any(character => !char.IsAsciiLetterLower(character)
                    && !char.IsAsciiDigit(character)
                    && character != '_')))
        {
            throw new ArgumentException("An AI failure code must be lowercase ASCII.", nameof(failureCode));
        }

        if (startedAtUtc.Offset != TimeSpan.Zero
            || completedAtUtc.Offset != TimeSpan.Zero
            || completedAtUtc < startedAtUtc)
        {
            throw new ArgumentException("AI generation timestamps must be ordered UTC instants.");
        }

        var normalizedText = finalizedText.Trim();
        if (new StringInfo(normalizedText).LengthInTextElements > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(finalizedText));
        }

        Id = id;
        WorldId = worldId;
        SimulationActionId = simulationActionId;
        Provider = provider;
        Model = model;
        Status = AiGenerationStatus.Completed;
        AttemptCount = attemptCount;
        InputHash = inputHash;
        OutputHash = outputHash;
        PromptTokenCount = promptTokenCount;
        OutputTokenCount = outputTokenCount;
        LatencyMilliseconds = latencyMilliseconds;
        FailureCode = failureCode;
        FallbackUsed = fallbackUsed;
        PromptTemplateVersion = promptTemplateVersion;
        FinalizedText = normalizedText;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        IdempotencyKey = idempotencyKey;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid SimulationActionId { get; private set; }
    public string Provider { get; private set; }
    public string Model { get; private set; }
    public AiGenerationStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public string InputHash { get; private set; }
    public string OutputHash { get; private set; }
    public int? PromptTokenCount { get; private set; }
    public int? OutputTokenCount { get; private set; }
    public long LatencyMilliseconds { get; private set; }
    public string? FailureCode { get; private set; }
    public bool FallbackUsed { get; private set; }
    public string PromptTemplateVersion { get; private set; }
    public string FinalizedText { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset CompletedAtUtc { get; private set; }
    public string IdempotencyKey { get; private set; }
    public long Version { get; private set; }

    private static bool IsLowercaseHex(string value) =>
        value.All(character => char.IsAsciiHexDigit(character)
            && (!char.IsAsciiLetter(character) || char.IsAsciiLetterLower(character)));
}

public enum AiGenerationStatus
{
    Completed,
}
