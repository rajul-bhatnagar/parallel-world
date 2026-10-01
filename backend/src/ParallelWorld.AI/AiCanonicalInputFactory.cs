using ParallelWorld.Application.AI;

namespace ParallelWorld.AI;

public sealed class AiCanonicalInputFactory : IAiCanonicalInputFactory
{
    public AiCanonicalInput Create(
        AiActionWordingProjection projection,
        AiGenerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(request);

        if (projection.WorldId != request.WorldId
            || projection.SimulationActionId != request.SimulationActionId)
        {
            throw new InvalidOperationException(
                "The resolved AI wording projection does not match the requested action identity.");
        }

        var actionType = projection.ActionType.Trim().ToLowerInvariant();
        var textKind = actionType switch
        {
            "post" => AiTextKind.Post,
            "reply" => AiTextKind.Reply,
            "react" => AiTextKind.ShortSocial,
            "follow" => AiTextKind.ShortSocial,
            "act" => AiTextKind.Event,
            _ => throw new InvalidOperationException("The action type is not eligible for M09 wording."),
        };

        var actor = AiSensitiveText.Sanitize(projection.ActorDisplayName);
        var target = AiSensitiveText.Sanitize(projection.TargetDisplayName);
        var topic = AiSensitiveText.Sanitize(projection.Topic);
        var mood = AiSensitiveText.Sanitize(projection.VisibleMood);
        var style = AiSensitiveText.Sanitize(projection.StyleHint);
        var stance = AiSensitiveText.Sanitize(projection.Stance);
        var tone = AiSensitiveText.Sanitize(projection.Tone);
        var outcome = AiSensitiveText.Sanitize(projection.FactualOutcome);
        var untrusted = AiSensitiveText.Sanitize(request.Presentation.UntrustedContext);
        var duplicates = request.Presentation.DuplicateTexts
            .Select(AiSensitiveText.Sanitize)
            .ToArray();
        var otherActors = projection.OtherActorDisplayNames
            .Select(AiSensitiveText.Sanitize)
            .ToArray();
        var sensitiveDetected = new[]
        {
            actor,
            target,
            topic,
            mood,
            style,
            stance,
            tone,
            outcome,
            untrusted,
        }.Any(value => value.SensitiveDetected)
            || duplicates.Any(value => value.SensitiveDetected)
            || otherActors.Any(value => value.SensitiveDetected);

        return new(
            textKind,
            actionType,
            actor.Value!,
            target.Value,
            topic.Value,
            mood.Value,
            style.Value,
            stance.Value,
            tone.Value,
            outcome.Value!,
            untrusted.Value,
            [],
            duplicates.Select(value => value.Value!).ToArray(),
            otherActors
                .Select(value => value.Value!)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            sensitiveDetected,
            request.MaxOutputLength);
    }
}
