namespace ParallelWorld.Application.AI;

public interface IAiGenerationRepository
{
    Task<AiActionWordingProjection?> ResolveActionWordingProjectionAsync(
        Guid worldId,
        Guid simulationActionId,
        CancellationToken cancellationToken = default);

    Task<AiGenerationRecord?> FindByIdempotencyKeyAsync(
        Guid worldId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<AiGenerationRecord?> FindByActionAndInputAsync(
        Guid worldId,
        Guid simulationActionId,
        string inputHash,
        CancellationToken cancellationToken = default);

    Task<AiGenerationRecord> PersistAsync(
        AiGenerationPersistenceData data,
        CancellationToken cancellationToken = default);
}

public sealed record AiActionWordingProjection(
    Guid WorldId,
    Guid SimulationActionId,
    string ActionType,
    string ActorDisplayName,
    string? TargetDisplayName,
    string? Topic,
    string? VisibleMood,
    string? StyleHint,
    string? Stance,
    string? Tone,
    string FactualOutcome,
    IReadOnlyList<string> OtherActorDisplayNames);

public sealed record AiGenerationPersistenceData(
    Guid Id,
    Guid WorldId,
    Guid SimulationActionId,
    string Provider,
    string Model,
    int AttemptCount,
    string InputHash,
    string OutputHash,
    int? PromptTokenCount,
    int? OutputTokenCount,
    long LatencyMilliseconds,
    string? FailureCode,
    bool FallbackUsed,
    string PromptTemplateVersion,
    string FinalizedText,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    string IdempotencyKey);

public sealed record AiGenerationRecord(
    Guid Id,
    Guid WorldId,
    Guid SimulationActionId,
    string Provider,
    string Model,
    int AttemptCount,
    string InputHash,
    string OutputHash,
    int? PromptTokenCount,
    int? OutputTokenCount,
    long LatencyMilliseconds,
    string? FailureCode,
    bool FallbackUsed,
    string PromptTemplateVersion,
    string FinalizedText,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    string IdempotencyKey);
