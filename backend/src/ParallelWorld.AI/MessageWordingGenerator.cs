using System.Text.Json;
using Microsoft.Extensions.Options;
using ParallelWorld.Application.Messaging;

namespace ParallelWorld.AI;

public sealed class MessageWordingGenerator(IAiTextProvider provider, IAiPromptBuilder promptBuilder,
    IAiFallbackRenderer fallbackRenderer, IAiOutputValidator validator, IOptions<AiGenerationOptions> options)
    : IMessageWordingGenerator
{
    private readonly AiGenerationOptions settings = options.Value;

    public async Task<MessageWordingResult> GenerateAsync(MessageWordingRequest request, CancellationToken ct)
    {
        var actor = AiSensitiveText.Sanitize(request.CharacterDisplayName);
        var target = AiSensitiveText.Sanitize(request.PlayerDisplayName);
        var mood = AiSensitiveText.Sanitize(request.VisibleMood);
        var style = AiSensitiveText.Sanitize(request.StyleHint);
        var untrusted = AiSensitiveText.Sanitize(request.SourceMessageBody);
        var memories = request.MemoryContext.Select(AiSensitiveText.Sanitize).ToArray();
        var sensitive = actor.SensitiveDetected || target.SensitiveDetected || mood.SensitiveDetected
            || style.SensitiveDetected || untrusted.SensitiveDetected
            || memories.Any(memory => memory.SensitiveDetected);
        var untrustedContext = JsonSerializer.Serialize(new
        {
            sourceMessage = untrusted.Value,
            selectedMemories = memories.Select(memory => memory.Value).ToArray(),
        });
        var input = new AiCanonicalInput(AiTextKind.Reply, "reply", actor.Value ?? "Character",
            target.Value, null, mood.Value, style.Value, "responsive", "conversational",
            "replied to the player's private message", untrustedContext, [], [], [], sensitive,
            Math.Min(request.MaxOutputLength, settings.MaxOutputLength));
        string? text = null;
        string? failure = sensitive ? "sensitive_input" : settings.Enabled ? null : "provider_disabled";
        if (settings.Enabled && !sensitive)
        {
            var prompt = promptBuilder.Build(input, settings.PromptTemplateVersion);
            for (var attempt = 0; attempt < 2; attempt++)
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(settings.Timeout);
                AiProviderResult result;
                try { result = await provider.GenerateAsync(new(settings.Model, prompt), timeout.Token); }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested) { result = AiProviderResult.Failure("provider_timeout", true); }
                if (result.Succeeded)
                {
                    var checkedOutput = validator.ValidateProviderOutput(result.GeneratedText, input, settings.MaxOutputLength);
                    if (checkedOutput.IsValid) text = checkedOutput.NormalizedText;
                    else failure = checkedOutput.FailureCode;
                    break;
                }
                failure = result.FailureCode;
                if (!result.IsTransientFailure) break;
            }
        }
        if (text is not null) return new(text, false, null);
        var fallback = fallbackRenderer.Render(input, settings.PromptTemplateVersion);
        var validated = validator.ValidateFinalizedText(fallback, input, settings.MaxOutputLength);
        if (!validated.IsValid) throw new InvalidOperationException("The deterministic messaging fallback is invalid.");
        return new(validated.NormalizedText!, true, failure);
    }
}
