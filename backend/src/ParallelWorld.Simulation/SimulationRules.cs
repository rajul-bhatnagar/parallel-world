using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Simulation;

namespace ParallelWorld.Simulation;

public static class SimulationRuleCodes
{
    public const string Act = "ACT-01";
    public const string Post = "POST-01";
    public const string Reply = "REPLY-01";
    public const string React = "REACT-01";
    public const string Follow = "FOLLOW-01";
}

public static class SimulationReasonCodes
{
    public const string RelationshipStateUnavailable = "relationship_state_unavailable";
    public const string GoalRelevanceUnavailable = "goal_relevance_unavailable";
    public const string MoodActivationUnavailable = "mood_activation_unavailable";
    public const string EventRelevanceUnavailable = "event_relevance_unavailable";
    public const string TopicInputsUnavailable = "topic_inputs_unavailable";
    public const string QuietHours = "quiet_hours";
    public const string ScheduleIneligible = "schedule_ineligible";
    public const string PositiveMoodUnavailable = "positive_mood_unavailable";
    public const string RepetitionSemanticsUnavailable = "repetition_semantics_unavailable";
    public const string FollowCandidateIneligible = "follow_candidate_ineligible";
    public const string FollowThresholdIneligible = "follow_threshold_ineligible";
    public const string FollowRollIneligible = "follow_roll_ineligible";
    public const string FollowExecuted = "follow_executed";
}

public sealed record SimulationRuleContext(
    Guid WorldId,
    Guid RunId,
    DateTimeOffset IntervalStartUtc,
    DateTimeOffset IntervalEndUtc,
    DateTimeOffset ResultingWorldTimeUtc,
    DateTimeOffset LocalWorldTime,
    decimal EffectiveTimeScale,
    int RuleVersion,
    long Seed,
    SimulationInputAvailability InputAvailability,
    IDeterministicRandomProvider Random,
    IReadOnlyList<FollowRuleCandidate>? FollowCandidates = null);

public sealed record SimulationInputAvailability(
    bool HasGoalRelevance,
    bool HasMoodActivation,
    bool HasEventRelevance,
    bool HasTopicInputs,
    bool HasRelationshipState,
    bool HasAuthorReputation = false,
    bool HasPositiveMood = false,
    bool HasRepetitionSemantics = false)
{
    public static SimulationInputAvailability M08 { get; } = new(false, false, false, false, false);
    public static SimulationInputAvailability M10 { get; } = new(false, false, false, false, true, true, false, false);
}

public sealed record SimulationRuleDecision(
    SimulationRuleOutcome Outcome,
    string ReasonCode,
    FollowRuleAction? FollowAction = null);

public interface ISimulationRule
{
    string Code { get; }
    int Priority { get; }
    SimulationRuleDecision Evaluate(SimulationRuleContext context);
}

public sealed class ActSimulationRule : ISimulationRule
{
    public string Code => SimulationRuleCodes.Act;
    public int Priority => 100;

    public SimulationRuleDecision Evaluate(SimulationRuleContext context)
    {
        if (!context.InputAvailability.HasGoalRelevance)
        {
            return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.GoalRelevanceUnavailable);
        }

        if (!context.InputAvailability.HasMoodActivation)
        {
            return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.MoodActivationUnavailable);
        }

        if (!context.InputAvailability.HasEventRelevance)
        {
            return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.EventRelevanceUnavailable);
        }

        throw new InvalidOperationException("ACT-01 cannot activate before its approved inputs are implemented.");
    }
}

public sealed class PostSimulationRule : ISimulationRule
{
    public string Code => SimulationRuleCodes.Post;
    public int Priority => 200;

    public SimulationRuleDecision Evaluate(SimulationRuleContext context)
    {
        if (!context.InputAvailability.HasGoalRelevance)
        {
            return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.GoalRelevanceUnavailable);
        }

        if (!context.InputAvailability.HasMoodActivation)
        {
            return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.MoodActivationUnavailable);
        }

        if (!context.InputAvailability.HasEventRelevance)
        {
            return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.EventRelevanceUnavailable);
        }

        if (!context.InputAvailability.HasTopicInputs)
        {
            return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.TopicInputsUnavailable);
        }

        throw new InvalidOperationException("POST-01 cannot activate before its approved inputs are implemented.");
    }
}

public sealed class ReplySimulationRule : ISimulationRule
{
    public string Code => SimulationRuleCodes.Reply;
    public int Priority => 300;

    public SimulationRuleDecision Evaluate(SimulationRuleContext context) =>
        !context.InputAvailability.HasRelationshipState ? new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.RelationshipStateUnavailable)
        : new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.MoodActivationUnavailable);
}

public sealed class ReactSimulationRule : ISimulationRule
{
    public string Code => SimulationRuleCodes.React;
    public int Priority => 400;

    public SimulationRuleDecision Evaluate(SimulationRuleContext context)
    {
        if (!context.InputAvailability.HasRelationshipState) return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.RelationshipStateUnavailable);
        if (!context.InputAvailability.HasAuthorReputation) return new(SimulationRuleOutcome.Unavailable, "author_reputation_unavailable");
        if (!context.InputAvailability.HasPositiveMood) return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.PositiveMoodUnavailable);
        if (!context.InputAvailability.HasGoalRelevance) return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.GoalRelevanceUnavailable);
        if (!context.InputAvailability.HasRepetitionSemantics) return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.RepetitionSemanticsUnavailable);
        throw new InvalidOperationException("REACT-01 has no approved M10 execution contract beyond repetition.");
    }
}

public sealed class FollowSimulationRule : ISimulationRule
{
    public string Code => SimulationRuleCodes.Follow;
    public int Priority => 500;

    public SimulationRuleDecision Evaluate(SimulationRuleContext context)
    {
        if (!context.InputAvailability.HasRelationshipState) return new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.RelationshipStateUnavailable);
        foreach (var c in context.FollowCandidates ?? [])
        {
            if (c.IsFollowing)
            {
                if (c.Relationship.Rivalry >= 60 || c.Relationship.Trust <= 25 || c.QualifiedNegativeEventCount >= 3)
                {
                    return new(SimulationRuleOutcome.Executed, SimulationReasonCodes.FollowExecuted,
                        new(c.SourceActorId, c.TargetActorId, false, "Unfollow", 0, 0, c.StableOrdinal));
                }
                continue;
            }
            if (c.Relationship.Familiarity < 20 || (c.InterestOverlap < 40 && c.TargetReputation < 70)) continue;
            var score = RelationshipMechanics.FollowProbability(c.Relationship, c.InterestOverlap, c.TargetReputation);
            if (score < 60) continue;
            var roll = context.Random.NextInt(new(context.Seed, Code, c.SourceActorId, c.TargetActorId, null, c.StableOrdinal), 0, 100);
            if (roll < score) return new(SimulationRuleOutcome.Executed, SimulationReasonCodes.FollowExecuted, new(c.SourceActorId, c.TargetActorId, true, c.HasHistoricalFollow ? "Re-follow" : null, score, roll, c.StableOrdinal));
            return new(SimulationRuleOutcome.Eligible, SimulationReasonCodes.FollowRollIneligible);
        }
        return new(SimulationRuleOutcome.Ineligible, SimulationReasonCodes.FollowCandidateIneligible);
    }
}
