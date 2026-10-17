namespace ParallelWorld.AI;

public enum AiTextKind
{
    Post,
    Reply,
    ShortSocial,
    Event,
}

public sealed record AiPresentationContext(
    string? UntrustedContext,
    IReadOnlyList<string> DuplicateTexts);

public sealed record AiCanonicalInput(
    AiTextKind TextKind,
    string ActionType,
    string ActorDisplayName,
    string? TargetDisplayName,
    string? Topic,
    string? VisibleMood,
    string? StyleHint,
    string? Stance,
    string? Tone,
    string FactualOutcome,
    string? UntrustedContext,
    IReadOnlyList<string> ProhibitedClaims,
    IReadOnlyList<string> DuplicateTexts,
    IReadOnlyList<string> OtherActorDisplayNames,
    bool SensitiveInputDetected,
    int MaxOutputLength);

public sealed record AiGenerationRequest(
    Guid WorldId,
    Guid SimulationActionId,
    string IdempotencyKey,
    AiPresentationContext Presentation,
    int MaxOutputLength);

public sealed record AiGenerationResult(
    Guid GenerationId,
    string GeneratedText,
    bool FallbackUsed,
    string Provider,
    string Model,
    string PromptTemplateVersion,
    string? FailureCode,
    int AttemptCount);

public interface IAiTextGenerator
{
    Task<AiGenerationResult> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record AiPrompt(
    string SystemInstruction,
    string UserPrompt,
    string TemplateVersion,
    int MaxOutputLength);

public sealed record AiProviderRequest(
    string Model,
    AiPrompt Prompt);

public sealed record AiProviderResult(
    bool Succeeded,
    string? GeneratedText,
    bool IsTransientFailure,
    string? FailureCode,
    int? PromptTokenCount = null,
    int? OutputTokenCount = null)
{
    public static AiProviderResult Success(
        string generatedText,
        int? promptTokenCount = null,
        int? outputTokenCount = null) =>
        new(true, generatedText, false, null, promptTokenCount, outputTokenCount);

    public static AiProviderResult Failure(string failureCode, bool transient) =>
        new(false, null, transient, failureCode);
}

public interface IAiTextProvider
{
    Task<AiProviderResult> GenerateAsync(
        AiProviderRequest request,
        CancellationToken cancellationToken = default);
}

public interface IAiPromptBuilder
{
    AiPrompt Build(AiCanonicalInput input, string templateVersion);
}

public interface IAiFallbackRenderer
{
    string Render(AiCanonicalInput input, string templateVersion);
}

public interface IAiOutputValidator
{
    AiOutputValidationResult ValidateProviderOutput(
        string? output,
        AiCanonicalInput input,
        int configuredMaximumLength);

    AiOutputValidationResult ValidateFinalizedText(
        string? output,
        AiCanonicalInput input,
        int configuredMaximumLength);
}

public interface IAiCanonicalInputFactory
{
    AiCanonicalInput Create(
        ParallelWorld.Application.AI.AiActionWordingProjection projection,
        AiGenerationRequest request);
}

public sealed record AiOutputValidationResult(
    bool IsValid,
    string? NormalizedText,
    string? FailureCode)
{
    public static AiOutputValidationResult Valid(string normalizedText) =>
        new(true, normalizedText, null);

    public static AiOutputValidationResult Invalid(string failureCode) =>
        new(false, null, failureCode);
}
