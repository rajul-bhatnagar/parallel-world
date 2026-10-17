using System.Globalization;

namespace ParallelWorld.AI;

public sealed class DeterministicAiFallbackRenderer : IAiFallbackRenderer
{
    public string Render(AiCanonicalInput input, string templateVersion)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateVersion);

        if (input.SensitiveInputDetected)
        {
            return "An update was shared.";
        }

        var actor = input.ActorDisplayName.Trim();
        var target = input.TargetDisplayName?.Trim();
        var topic = input.Topic?.Trim();
        var text = input.TextKind switch
        {
            AiTextKind.Post when !string.IsNullOrWhiteSpace(topic) =>
                $"{actor} shared an update about {topic}.",
            AiTextKind.Reply when !string.IsNullOrWhiteSpace(target) =>
                $"{actor} responded to {target}.",
            AiTextKind.ShortSocial when input.ActionType == "follow" && !string.IsNullOrWhiteSpace(target) =>
                $"{actor} followed {target}.",
            AiTextKind.ShortSocial when input.ActionType == "react" => $"{actor} reacted.",
            AiTextKind.Event => $"{actor}: {input.FactualOutcome.Trim()}.",
            _ => $"{actor} shared an update.",
        };

        if (new StringInfo(text).LengthInTextElements <= input.MaxOutputLength)
        {
            return text;
        }

        var actorOnly = $"{actor} posted.";
        return new StringInfo(actorOnly).LengthInTextElements <= input.MaxOutputLength
            ? actorOnly
            : "An update was shared.";
    }
}
