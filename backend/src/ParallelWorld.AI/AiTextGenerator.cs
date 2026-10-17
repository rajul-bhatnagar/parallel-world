using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParallelWorld.Application.AI;

namespace ParallelWorld.AI;

public sealed class AiTextGenerator(
    IAiTextProvider provider,
    IAiPromptBuilder promptBuilder,
    IAiFallbackRenderer fallbackRenderer,
    IAiOutputValidator outputValidator,
    IAiCanonicalInputFactory canonicalInputFactory,
    IAiGenerationRepository repository,
    IOptions<AiGenerationOptions> options,
    TimeProvider timeProvider,
    ILogger<AiTextGenerator> logger) : IAiTextGenerator
{
    private readonly AiGenerationOptions _options = options.Value;

    public async Task<AiGenerationResult> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        cancellationToken.ThrowIfCancellationRequested();

        var projection = await repository.ResolveActionWordingProjectionAsync(
            request.WorldId,
            request.SimulationActionId,
            cancellationToken);
        if (projection is null)
        {
            throw new KeyNotFoundException(
                "The requested simulation action is not eligible for AI wording.");
        }

        var input = canonicalInputFactory.Create(projection, request);
        ValidateCanonicalInput(input);
        var inputHash = AiHashing.InputHash(input, _options.PromptTemplateVersion);
        var byIdempotency = await repository.FindByIdempotencyKeyAsync(
            request.WorldId,
            request.IdempotencyKey,
            cancellationToken);
        if (byIdempotency is not null)
        {
            if (!string.Equals(byIdempotency.InputHash, inputHash, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The AI generation idempotency key was already used for different authoritative facts.");
            }

            return FromRecord(byIdempotency);
        }

        var existing = await repository.FindByActionAndInputAsync(
            request.WorldId,
            request.SimulationActionId,
            inputHash,
            cancellationToken);
        if (existing is not null)
        {
            return FromRecord(existing);
        }

        var startedAtUtc = timeProvider.GetUtcNow();
        var timestamp = timeProvider.GetTimestamp();
        var attempts = 0;
        string? failureCode = null;
        AiProviderResult? providerResult = null;
        string? finalizedText = null;
        var fallbackUsed = true;

        if (_options.Enabled && !input.SensitiveInputDetected)
        {
            var prompt = promptBuilder.Build(input, _options.PromptTemplateVersion);
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                attempts = attempt;
                using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutSource.CancelAfter(_options.Timeout);
                try
                {
                    providerResult = await provider.GenerateAsync(
                        new(_options.Model, prompt),
                        timeoutSource.Token);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    providerResult = AiProviderResult.Failure("provider_timeout", transient: true);
                }

                if (providerResult.Succeeded)
                {
                    var validation = outputValidator.ValidateProviderOutput(
                        providerResult.GeneratedText,
                        input,
                        _options.MaxOutputLength);
                    if (validation.IsValid)
                    {
                        finalizedText = validation.NormalizedText;
                        fallbackUsed = false;
                        failureCode = null;
                    }
                    else
                    {
                        failureCode = validation.FailureCode;
                    }

                    break;
                }

                failureCode = providerResult.FailureCode;
                if (!providerResult.IsTransientFailure || attempt == 2)
                {
                    break;
                }
            }
        }
        else
        {
            failureCode = input.SensitiveInputDetected ? "sensitive_input" : "provider_disabled";
        }

        if (finalizedText is null)
        {
            var fallback = fallbackRenderer.Render(input, _options.PromptTemplateVersion);
            var validation = outputValidator.ValidateFinalizedText(
                fallback,
                input,
                _options.MaxOutputLength);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException("The deterministic AI fallback violated its output contract.");
            }

            finalizedText = validation.NormalizedText;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var completedText = finalizedText
            ?? throw new InvalidOperationException("AI generation did not produce finalized wording.");
        var completedAtUtc = timeProvider.GetUtcNow();
        var latencyMilliseconds = (long)timeProvider.GetElapsedTime(timestamp).TotalMilliseconds;
        var data = new AiGenerationPersistenceData(
            AiHashing.GenerationId(request.WorldId, request.SimulationActionId, inputHash),
            request.WorldId,
            request.SimulationActionId,
            "ollama",
            _options.Model,
            attempts,
            inputHash,
            AiHashing.TextHash(completedText),
            providerResult?.PromptTokenCount,
            providerResult?.OutputTokenCount,
            Math.Max(0, latencyMilliseconds),
            failureCode,
            fallbackUsed,
            _options.PromptTemplateVersion,
            completedText,
            startedAtUtc,
            completedAtUtc,
            request.IdempotencyKey);
        var persisted = await repository.PersistAsync(data, cancellationToken);

        logger.LogInformation(
            "AI wording completed for provider {Provider}, model {Model}, template {TemplateVersion}, "
                + "attempts {AttemptCount}, fallback {FallbackUsed}, failure {FailureCode}, duration {DurationMs}ms.",
            persisted.Provider,
            persisted.Model,
            persisted.PromptTemplateVersion,
            persisted.AttemptCount,
            persisted.FallbackUsed,
            persisted.FailureCode ?? "none",
            persisted.LatencyMilliseconds);

        return FromRecord(persisted);
    }

    private void ValidateRequest(AiGenerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.IdempotencyKey);
        ArgumentNullException.ThrowIfNull(request.Presentation);
        ArgumentNullException.ThrowIfNull(request.Presentation.DuplicateTexts);
        if (request.IdempotencyKey.Length > 200
            || request.Presentation.UntrustedContext?.Length > 1000
            || request.Presentation.DuplicateTexts.Count > 8
            || request.Presentation.DuplicateTexts.Any(value => value is null || value.Length > 500))
        {
            throw new ArgumentOutOfRangeException(nameof(request));
        }

        if (request.MaxOutputLength is < 40 or > AiGenerationOptions.MaximumContentCharacters
            || request.MaxOutputLength > _options.MaxOutputLength)
        {
            throw new ArgumentOutOfRangeException(nameof(request.MaxOutputLength));
        }
    }

    private static void ValidateCanonicalInput(AiCanonicalInput input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input.ActionType);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.ActorDisplayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.FactualOutcome);
        if (input.ActionType.Length > 50
            || input.ActorDisplayName.Length > 60
            || input.TargetDisplayName?.Length > 60
            || input.Topic?.Length > 100
            || input.VisibleMood?.Length > 40
            || input.StyleHint?.Length > 120
            || input.Stance?.Length > 40
            || input.Tone?.Length > 40
            || input.FactualOutcome.Length > 200
            || input.OtherActorDisplayNames.Count > 100)
        {
            throw new InvalidOperationException("The authoritative action wording projection is invalid.");
        }
    }

    private static AiGenerationResult FromRecord(AiGenerationRecord record) => new(
        record.Id,
        record.FinalizedText,
        record.FallbackUsed,
        record.Provider,
        record.Model,
        record.PromptTemplateVersion,
        record.FailureCode,
        record.AttemptCount);
}
