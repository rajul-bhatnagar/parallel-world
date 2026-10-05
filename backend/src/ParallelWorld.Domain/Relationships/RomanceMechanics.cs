using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Domain.Relationships;

public static class RomanceReasonCodes
{
    public const string RomanceDisabled = "romance_disabled";
    public const string InitiatorNotOpen = "initiator_not_open";
    public const string TargetNotOpen = "target_not_open";
    public const string RomanceIncompatible = "romance_incompatible";
    public const string ExclusivityConflict = "exclusivity_conflict";
    public const string RomanticStateConflict = "romantic_state_conflict";
    public const string FamiliarityBelowMinimum = "familiarity_below_minimum";
    public const string TrustBelowMinimum = "trust_below_minimum";
    public const string AttractionBelowMinimum = "attraction_below_minimum";
    public const string ComfortBelowMinimum = "comfort_below_minimum";
    public const string RomanticOpennessBelowMinimum = "romantic_openness_below_minimum";
    public const string InitiationScoreBelowMinimum = "initiation_score_below_minimum";
    public const string AcceptanceScoreBelowMinimum = "acceptance_score_below_minimum";
    public const string CooldownActive = "invitation_cooldown_active";
    public const string DeterministicRollFailed = "deterministic_roll_failed";
    public const string PlayerAccepted = "player_accepted";
    public const string PlayerRejected = "player_rejected";
    public const string InvitationExpired = "invitation_expired";
    public const string CharacterAccepted = "character_accepted";
}

public readonly record struct RomanceGateInput(
    bool RomanceEnabled,
    RomancePreferenceMode InitiatorMode,
    RomancePreferenceMode TargetMode,
    bool InitiatorActive,
    bool TargetActive,
    bool SameWorld,
    bool DistinctActors,
    bool ExclusivityConflict,
    bool RomanticStateConflict);

public readonly record struct RomanceEvaluation(bool Eligible, int Score, string? ReasonCode);

public static class RomanceMechanics
{
    public const int GoalRelevance = 50;
    public const int PositiveMood = 50;
    public const int MoodModifier = 0;
    public const int ConflictPenalty = 0;
    public const int Compatibility = 100;
    public const int PlayerRomanticOpenness = 100;
    public const int FamiliarityMinimum = 55;
    public const int TrustMinimum = 45;
    public const int AttractionMinimum = 60;
    public const int ComfortMinimum = 40;
    public const int RomanticOpennessMinimum = 40;
    public const int InitiationMinimum = 60;
    public const int AcceptanceMinimum = 60;
    public const int CommitmentOnDating = 10;
    public static readonly TimeSpan InvitationLifetime = TimeSpan.FromHours(24);
    public static readonly TimeSpan InvitationCooldown = TimeSpan.FromDays(14);

    public static string? Gate(RomanceGateInput input)
    {
        if (!input.RomanceEnabled) return RomanceReasonCodes.RomanceDisabled;
        if (input.InitiatorMode != RomancePreferenceMode.AnyEligibleActor) return RomanceReasonCodes.InitiatorNotOpen;
        if (input.TargetMode != RomancePreferenceMode.AnyEligibleActor) return RomanceReasonCodes.TargetNotOpen;
        if (!input.InitiatorActive || !input.TargetActive || !input.SameWorld || !input.DistinctActors)
            return RomanceReasonCodes.RomanceIncompatible;
        if (input.ExclusivityConflict) return RomanceReasonCodes.ExclusivityConflict;
        if (input.RomanticStateConflict) return RomanceReasonCodes.RomanticStateConflict;
        return null;
    }

    public static RomanceEvaluation EvaluateInitiation(RelationshipValues values, int romanticOpenness)
    {
        var threshold = ThresholdFailure(values, romanticOpenness, includeFamiliarity: true);
        if (threshold is not null) return new(false, 0, threshold);
        var score = Round(
            .30m * values.Attraction + .20m * values.Trust + .15m * values.Comfort
            + .10m * values.Familiarity + .10m * romanticOpenness + .10m * GoalRelevance
            + .05m * PositiveMood - ConflictPenalty);
        return score >= InitiationMinimum
            ? new(true, score, null)
            : new(false, score, RomanceReasonCodes.InitiationScoreBelowMinimum);
    }

    public static RomanceEvaluation EvaluateAcceptance(RelationshipValues values, int romanticOpenness, int seededOffset)
    {
        if (seededOffset is < -5 or > 5) throw new ArgumentOutOfRangeException(nameof(seededOffset));
        var threshold = ThresholdFailure(values, romanticOpenness, includeFamiliarity: false);
        if (threshold is not null) return new(false, 0, threshold);
        var score = Math.Clamp(Round(
            .35m * values.Attraction + .25m * values.Trust + .15m * values.Comfort
            + .10m * Compatibility + .10m * romanticOpenness + .05m * GoalRelevance
            + MoodModifier - ConflictPenalty + seededOffset), 0, 100);
        return score >= AcceptanceMinimum
            ? new(true, score, null)
            : new(false, score, RomanceReasonCodes.AcceptanceScoreBelowMinimum);
    }

    private static string? ThresholdFailure(RelationshipValues values, int romanticOpenness, bool includeFamiliarity)
    {
        if (includeFamiliarity && values.Familiarity < FamiliarityMinimum) return RomanceReasonCodes.FamiliarityBelowMinimum;
        if (values.Trust < TrustMinimum) return RomanceReasonCodes.TrustBelowMinimum;
        if (values.Attraction < AttractionMinimum) return RomanceReasonCodes.AttractionBelowMinimum;
        if (values.Comfort < ComfortMinimum) return RomanceReasonCodes.ComfortBelowMinimum;
        if (romanticOpenness < RomanticOpennessMinimum) return RomanceReasonCodes.RomanticOpennessBelowMinimum;
        return null;
    }

    private static int Round(decimal value) =>
        (int)Math.Round(value, MidpointRounding.AwayFromZero);
}
