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
    IDeterministicRandomProvider Random);

public sealed record SimulationInputAvailability(
    bool HasGoalRelevance,
    bool HasMoodActivation,
    bool HasEventRelevance,
    bool HasTopicInputs,
    bool HasRelationshipState)
{
    public static SimulationInputAvailability M08 { get; } = new(false, false, false, false, false);
}

public sealed record SimulationRuleDecision(
    SimulationRuleOutcome Outcome,
    string ReasonCode);

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
        RelationshipDecision(context, Code);

    private static SimulationRuleDecision RelationshipDecision(
        SimulationRuleContext context,
        string ruleCode) =>
        !context.InputAvailability.HasRelationshipState
            ? new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.RelationshipStateUnavailable)
            : throw new InvalidOperationException($"{ruleCode} cannot activate before M10.");
}

public sealed class ReactSimulationRule : ISimulationRule
{
    public string Code => SimulationRuleCodes.React;
    public int Priority => 400;

    public SimulationRuleDecision Evaluate(SimulationRuleContext context) =>
        !context.InputAvailability.HasRelationshipState
            ? new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.RelationshipStateUnavailable)
            : throw new InvalidOperationException($"{Code} cannot activate before M10.");
}

public sealed class FollowSimulationRule : ISimulationRule
{
    public string Code => SimulationRuleCodes.Follow;
    public int Priority => 500;

    public SimulationRuleDecision Evaluate(SimulationRuleContext context) =>
        !context.InputAvailability.HasRelationshipState
            ? new(SimulationRuleOutcome.Unavailable, SimulationReasonCodes.RelationshipStateUnavailable)
            : throw new InvalidOperationException($"{Code} cannot activate before M10.");
}
