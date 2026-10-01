using System.Globalization;
using System.Text.RegularExpressions;

namespace ParallelWorld.AI;

public sealed partial class AiOutputValidator : IAiOutputValidator
{
    private static readonly string[] ForbiddenControlClaims =
    [
        "actor_id", "target_id", "world_id", "relationship_delta", "simulation_time",
        "ignore previous instructions", "change the outcome", "change actor", "change target",
    ];

    private static readonly string[] ForbiddenActionOrOutcomeClaims =
    [
        "posted", "published", "shared a post", "replied", "responded", "reacted", "liked",
        "disliked", "followed", "unfollowed", "attacked", "won", "lost", "succeeded", "failed",
        "was successful", "was defeated", "relationship", "trust increased", "affection", "rivalry",
        "became friends", "started dating", "broke up", "married",
    ];

    public AiOutputValidationResult ValidateProviderOutput(
        string? output,
        AiCanonicalInput input,
        int configuredMaximumLength)
    {
        var fragmentValidation = ValidateCommon(output, input, validateClaims: true);
        if (!fragmentValidation.IsValid)
        {
            return fragmentValidation;
        }

        var fragment = fragmentValidation.NormalizedText!;
        var finalized = input.TextKind switch
        {
            AiTextKind.Post => $"{input.ActorDisplayName} posted: {fragment}",
            AiTextKind.Reply when !string.IsNullOrWhiteSpace(input.TargetDisplayName) =>
                $"{input.ActorDisplayName} replied to {input.TargetDisplayName}: {fragment}",
            AiTextKind.Reply => $"{input.ActorDisplayName} replied: {fragment}",
            AiTextKind.ShortSocial when input.ActionType == "follow"
                && !string.IsNullOrWhiteSpace(input.TargetDisplayName) =>
                $"{input.ActorDisplayName} followed {input.TargetDisplayName}. {fragment}",
            AiTextKind.ShortSocial => $"{input.ActorDisplayName} reacted: {fragment}",
            _ => $"{input.ActorDisplayName} acted: {fragment}",
        };

        return ValidateFinalized(finalized, input, configuredMaximumLength, checkDuplicates: true);
    }

    public AiOutputValidationResult ValidateFinalizedText(
        string? output,
        AiCanonicalInput input,
        int configuredMaximumLength) =>
        ValidateFinalized(output, input, configuredMaximumLength, checkDuplicates: false);

    private static AiOutputValidationResult ValidateCommon(
        string? output,
        AiCanonicalInput input,
        bool validateClaims)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return AiOutputValidationResult.Invalid("empty_response");
        }

        var normalized = output.Trim();
        if (!HasValidUnicodeAndControls(normalized))
        {
            return AiOutputValidationResult.Invalid("invalid_unicode");
        }

        if (AiSensitiveText.ContainsSensitiveMaterial(normalized))
        {
            return AiOutputValidationResult.Invalid("sensitive_output");
        }

        if (validateClaims
            && (ForbiddenControlClaims.Concat(ForbiddenActionOrOutcomeClaims)
                    .Concat(input.ProhibitedClaims)
                    .Any(claim => ContainsClaim(normalized, claim))
                || EnumerateActorNames(input).Any(name => ContainsClaim(normalized, name))))
        {
            return AiOutputValidationResult.Invalid("prohibited_claim");
        }

        return AiOutputValidationResult.Valid(normalized);
    }

    private static AiOutputValidationResult ValidateFinalized(
        string? output,
        AiCanonicalInput input,
        int configuredMaximumLength,
        bool checkDuplicates)
    {
        var common = ValidateCommon(output, input, validateClaims: false);
        if (!common.IsValid)
        {
            return common;
        }

        var normalized = common.NormalizedText!;
        var maximumLength = Math.Min(input.MaxOutputLength, configuredMaximumLength);
        if (new StringInfo(normalized).LengthInTextElements > maximumLength)
        {
            return AiOutputValidationResult.Invalid("output_too_long");
        }

        if (checkDuplicates)
        {
            var duplicateKey = NormalizeForDuplicateComparison(normalized);
            if (input.DuplicateTexts.Any(item =>
                    NormalizeForDuplicateComparison(item) == duplicateKey))
            {
                return AiOutputValidationResult.Invalid("duplicate_output");
            }
        }

        return AiOutputValidationResult.Valid(normalized);
    }

    private static IEnumerable<string> EnumerateActorNames(AiCanonicalInput input)
    {
        yield return input.ActorDisplayName;
        if (!string.IsNullOrWhiteSpace(input.TargetDisplayName))
        {
            yield return input.TargetDisplayName;
        }

        foreach (var name in input.OtherActorDisplayNames)
        {
            yield return name;
        }
    }

    private static bool ContainsClaim(string output, string? claim) =>
        !string.IsNullOrWhiteSpace(claim)
        && output.Contains(claim, StringComparison.OrdinalIgnoreCase);

    private static bool HasValidUnicodeAndControls(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                {
                    return false;
                }

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character)
                || (char.IsControl(character) && character is not '\r' and not '\n' and not '\t'))
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeForDuplicateComparison(string value) =>
        WhitespacePattern().Replace(value.Trim(), " ").ToUpperInvariant();

    [GeneratedRegex("\\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespacePattern();
}
