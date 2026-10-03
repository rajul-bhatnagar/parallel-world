using ParallelWorld.Application.Relationships;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Characters;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Simulation;

namespace ParallelWorld.UnitTests;

public sealed class RelationshipTests
{
    [Fact]
    public void InitialValues_HaveExactlyNineApprovedDimensions()
    {
        Assert.Equal(new RelationshipValues(10, 50, 50, 20, 15, 0, 0, 0, 0), RelationshipValues.Initial);
        Assert.Equal("stranger", RelationshipMechanics.Label(RelationshipValues.Initial));
    }

    [Theory]
    [InlineData(new string[] { "a" }, new string[] { "a" }, 100)]
    [InlineData(new string[] { "a" }, new string[] { "b" }, 0)]
    [InlineData(new string[] { "a", "b" }, new string[] { "b", "c" }, 33)]
    [InlineData(new string[] { }, new string[] { "a" }, 0)]
    public void InterestOverlap_UsesSetJaccardAndApprovedRounding(string[] left, string[] right, int expected)
        => Assert.Equal(expected, RelationshipMechanics.InterestOverlap(left, right));

    [Fact]
    public void InterestOverlap_IgnoresDuplicatesAndOrder()
    {
        Assert.Equal(67, RelationshipMechanics.InterestOverlap(["b", "a", "a"], ["c", "b", "a"]));
        Assert.Equal(0, RelationshipMechanics.InterestOverlap([], []));
    }

    [Fact]
    public void Relevance_UsesAllNineDirectionalDimensions()
        => Assert.Equal(58, RelationshipMechanics.Relevance(new(80, 70, 60, 50, 40, 30, 20, 10, 100)));

    [Fact]
    public void Relevance_IncludesConflictAndClampsDefensively()
    {
        Assert.Equal(15, RelationshipMechanics.Relevance(new(0, 0, 0, 0, 0, 100, 100, 0, 0)));
        Assert.Equal(100, RelationshipMechanics.Relevance(new(100, 100, 100, 100, 100, 100, 100, 100, 100)));
    }

    [Fact]
    public void PublicNegativeAndRepetitionModifiers_AreDimensionSpecific()
    {
        Assert.True(RelationshipEventMatrix.TryGet("Insult", out var insult));
        Assert.Equal(new RelationshipValues(1, -5, -5, -3, -3, 6, 0, -2, 0), RelationshipMechanics.ApplyModifiers(insult, true, 0));
        Assert.Equal(new RelationshipValues(1, -3, -2, -2, -2, 3, 0, -1, 0), RelationshipMechanics.ApplyModifiers(insult, false, 3));
    }

    [Fact]
    public void DailyCap_PreservesDirectionAndLimitsAbsoluteChange()
    {
        var result = RelationshipMechanics.ApplyDailyCap(new(5, -5, 5, -5, 5, -5, 5, -5, 5), new(18, 19, 20, 0, 10, 20, 17, 19, 18));
        Assert.Equal(new RelationshipValues(2, -1, 0, -5, 5, 0, 3, -1, 2), result);
    }

    [Fact]
    public void Relationship_ClampsEveryDimensionAtBothBounds()
    {
        var relationship = new Relationship(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UnixEpoch);
        relationship.Apply(new(200, 200, 200, 200, 200, 200, 200, 200, 200), DateTimeOffset.UnixEpoch);
        Assert.Equal(new(100, 100, 100, 100, 100, 100, 100, 100, 100), relationship.Values);
        relationship.Apply(new(-200, -200, -200, -200, -200, -200, -200, -200, -200), DateTimeOffset.UnixEpoch);
        Assert.Equal(new(0, 0, 0, 0, 0, 0, 0, 0, 0), relationship.Values);
    }

    [Theory]
    [InlineData("Initial Follow")]
    [InlineData("Like")]
    [InlineData("Unlike")]
    [InlineData("Generic Reply")]
    [InlineData("Post")]
    [InlineData("provider-generated prose")]
    public void NoEffectActionsAndWording_HaveNoRelationshipMatrixRow(string eventType)
        => Assert.False(RelationshipEventMatrix.TryGet(eventType, out _));

    [Fact]
    public void FollowRule_UsesThresholdFormulaAndDeterministicRoll()
    {
        var candidate = new FollowRuleCandidate(Guid.NewGuid(), Guid.NewGuid(), new(60, 70, 50, 50, 20, 0, 0, 0, 0), 80, 50, false, false, 0, 0);
        var context = new SimulationRuleContext(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch.AddMinutes(15), DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, 1, 1, 42, SimulationInputAvailability.M10, new ZeroRandom(), [candidate]);
        var result = new FollowSimulationRule().Evaluate(context);
        Assert.Equal(SimulationRuleOutcome.Executed, result.Outcome);
        Assert.Equal(64, result.FollowAction!.Score);
    }

    [Fact]
    public void FollowRule_HandlesBelowThresholdExactThresholdAndFailedRoll()
    {
        var source = Guid.NewGuid();
        var target = Guid.NewGuid();
        var below = Candidate(new(20, 50, 50, 20, 15, 0, 0, 0, 0), 40, 50);
        Assert.Equal(36, RelationshipMechanics.FollowProbability(below.Relationship, below.InterestOverlap, below.TargetReputation));
        Assert.Equal(SimulationRuleOutcome.Ineligible, Evaluate(below, new FixedRandom(0)).Outcome);

        var exact = Candidate(new(100, 50, 50, 50, 15, 0, 0, 0, 0), 40, 50);
        Assert.Equal(60, RelationshipMechanics.FollowProbability(exact.Relationship, exact.InterestOverlap, exact.TargetReputation));
        Assert.Equal(SimulationRuleOutcome.Executed, Evaluate(exact with { SourceActorId = source, TargetActorId = target }, new FixedRandom(59)).Outcome);
        Assert.Equal(SimulationRuleOutcome.Eligible, Evaluate(exact with { SourceActorId = source, TargetActorId = target }, new FixedRandom(60)).Outcome);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    public void FollowRule_UsesQualifiedNegativeEventThreshold(int eventCount, bool shouldUnfollow)
    {
        var candidate = Candidate(RelationshipValues.Initial, 0, 50) with
        {
            IsFollowing = true,
            QualifiedNegativeEventCount = eventCount,
        };

        var result = Evaluate(candidate, new ZeroRandom());
        Assert.Equal(shouldUnfollow ? SimulationRuleOutcome.Executed : SimulationRuleOutcome.Ineligible, result.Outcome);
        Assert.Equal(shouldUnfollow, result.FollowAction?.DesiredFollowing == false);
    }

    [Fact]
    public void QualifiedNegativeClassification_UsesCanonicalEventTypesOnly()
    {
        Assert.True(RelationshipEventMatrix.IsQualifiedNegative("Insult"));
        Assert.True(RelationshipEventMatrix.IsQualifiedNegative("Broken promise"));
        Assert.False(RelationshipEventMatrix.IsQualifiedNegative("Helpful reply"));
        Assert.False(RelationshipEventMatrix.IsQualifiedNegative("AI says this sounds negative"));
    }

    [Fact]
    public void M10ReplyAndReact_RetainApprovedGates()
    {
        var context = new SimulationRuleContext(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch.AddMinutes(15), DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, 1, 1, 42, SimulationInputAvailability.M10, new ZeroRandom());
        Assert.Equal(SimulationReasonCodes.MoodActivationUnavailable, new ReplySimulationRule().Evaluate(context).ReasonCode);
        Assert.Equal(SimulationReasonCodes.PositiveMoodUnavailable, new ReactSimulationRule().Evaluate(context).ReasonCode);
    }

    [Fact]
    public void ReactGate_PreservesLaterApprovedPrecedence()
    {
        var goalMissing = Context(new(false, false, false, false, true, true, true, false));
        Assert.Equal(SimulationReasonCodes.GoalRelevanceUnavailable, new ReactSimulationRule().Evaluate(goalMissing).ReasonCode);
        var repetitionMissing = Context(new(true, false, false, false, true, true, true, false));
        Assert.Equal(SimulationReasonCodes.RepetitionSemanticsUnavailable, new ReactSimulationRule().Evaluate(repetitionMissing).ReasonCode);
    }

    [Fact]
    public void CharacterReputation_IsBounded()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Character(Guid.NewGuid(), Guid.NewGuid(), "A", "a", "", 20, "p", "a", "w", 50, 50, 50, 101, MoodType.Calm, DateTimeOffset.UnixEpoch));
    }

    private static FollowRuleCandidate Candidate(RelationshipValues values, int overlap, int reputation) =>
        new(Guid.NewGuid(), Guid.NewGuid(), values, overlap, reputation, false, false, 0, 0);

    private static SimulationRuleDecision Evaluate(FollowRuleCandidate candidate, IDeterministicRandomProvider random) =>
        new FollowSimulationRule().Evaluate(Context(SimulationInputAvailability.M10, random, [candidate]));

    private static SimulationRuleContext Context(
        SimulationInputAvailability availability,
        IDeterministicRandomProvider? random = null,
        IReadOnlyList<FollowRuleCandidate>? candidates = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch.AddMinutes(15), DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, 1, 1, 42, availability, random ?? new ZeroRandom(), candidates);

    private sealed class ZeroRandom : IDeterministicRandomProvider
    { public int NextInt(DeterministicChoiceCoordinates coordinates, int inclusiveMinimum, int exclusiveMaximum) => inclusiveMinimum; }
    private sealed class FixedRandom(int value) : IDeterministicRandomProvider
    { public int NextInt(DeterministicChoiceCoordinates coordinates, int inclusiveMinimum, int exclusiveMaximum) => value; }
}
