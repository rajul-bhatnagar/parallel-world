using System.Globalization;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Simulation;

namespace ParallelWorld.UnitTests;

public sealed class SimulationTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void DeterministicIdentities_AreRepeatableCultureIndependentAndWorldScoped()
    {
        var firstWorld = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondWorld = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var end = CreatedAt.AddMinutes(15);
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var first = DeterministicSimulationIdentity.CreateRunId(firstWorld, CreatedAt, end, 1);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var repeat = DeterministicSimulationIdentity.CreateRunId(firstWorld, CreatedAt, end, 1);
            var otherWorld = DeterministicSimulationIdentity.CreateRunId(secondWorld, CreatedAt, end, 1);

            Assert.Equal(first, repeat);
            Assert.NotEqual(first, otherWorld);
            Assert.Equal(
                DeterministicSimulationIdentity.CreateEvaluationId(firstWorld, first, SimulationRuleCodes.Act),
                DeterministicSimulationIdentity.CreateEvaluationId(firstWorld, repeat, SimulationRuleCodes.Act));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void RunSeed_UsesEveryCanonicalInputAndIsCultureIndependent()
    {
        const long worldSeed = 90210;
        const int ruleVersion = 7;
        const long runOrdinal = 3;
        var intervalEnd = CreatedAt.AddMinutes(15);
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var baseline = DeterministicSimulationIdentity.CreateRunSeed(
                worldSeed,
                ruleVersion,
                CreatedAt,
                intervalEnd,
                runOrdinal);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var repeat = DeterministicSimulationIdentity.CreateRunSeed(
                worldSeed,
                ruleVersion,
                CreatedAt,
                intervalEnd,
                runOrdinal);

            Assert.Equal(baseline, repeat);
            Assert.Equal(
                baseline,
                DeterministicSimulationIdentity.CreateRunSeed(
                    worldSeed,
                    ruleVersion,
                    CreatedAt.ToOffset(TimeSpan.FromHours(5.5)),
                    intervalEnd.ToOffset(TimeSpan.FromHours(5.5)),
                    runOrdinal));
            Assert.NotEqual(baseline, DeterministicSimulationIdentity.CreateRunSeed(
                worldSeed + 1, ruleVersion, CreatedAt, intervalEnd, runOrdinal));
            Assert.NotEqual(baseline, DeterministicSimulationIdentity.CreateRunSeed(
                worldSeed, ruleVersion + 1, CreatedAt, intervalEnd, runOrdinal));
            Assert.NotEqual(baseline, DeterministicSimulationIdentity.CreateRunSeed(
                worldSeed, ruleVersion, CreatedAt.AddMinutes(1), intervalEnd, runOrdinal));
            Assert.NotEqual(baseline, DeterministicSimulationIdentity.CreateRunSeed(
                worldSeed, ruleVersion, CreatedAt, intervalEnd.AddMinutes(1), runOrdinal));
            Assert.NotEqual(baseline, DeterministicSimulationIdentity.CreateRunSeed(
                worldSeed, ruleVersion, CreatedAt, intervalEnd, runOrdinal + 1));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void DeterministicRandomProvider_UsesCanonicalChoiceCoordinates()
    {
        var provider = new DeterministicRandomProvider();
        var actorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var targetId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var topicId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var baselineCoordinates = new DeterministicChoiceCoordinates(
            90210,
            SimulationRuleCodes.Act,
            actorId,
            targetId,
            topicId,
            4);
        var baseline = provider.CreateChoiceSeed(baselineCoordinates);

        Assert.NotEqual(baseline, provider.CreateChoiceSeed(baselineCoordinates with { RunSeed = 90211 }));
        Assert.NotEqual(baseline, provider.CreateChoiceSeed(baselineCoordinates with { RuleId = SimulationRuleCodes.Post }));
        Assert.NotEqual(baseline, provider.CreateChoiceSeed(baselineCoordinates with { ActorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") }));
        Assert.NotEqual(baseline, provider.CreateChoiceSeed(baselineCoordinates with { TargetId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }));
        Assert.NotEqual(baseline, provider.CreateChoiceSeed(baselineCoordinates with { TopicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc") }));
        Assert.NotEqual(baseline, provider.CreateChoiceSeed(baselineCoordinates with { StableOrdinal = 5 }));
    }

    [Fact]
    public void DeterministicRandomProvider_IsRepeatableAndCallOrderIndependent()
    {
        var firstProvider = new DeterministicRandomProvider();
        var secondProvider = new DeterministicRandomProvider();
        var choiceA = new DeterministicChoiceCoordinates(
            90210,
            SimulationRuleCodes.Act,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            null,
            null,
            0);
        var choiceB = choiceA with { RuleId = SimulationRuleCodes.Post, StableOrdinal = 1 };

        var firstA = firstProvider.NextInt(choiceA, 0, 100);
        var firstB = firstProvider.NextInt(choiceB, 0, 100);
        var repeatedA = firstProvider.NextInt(choiceA, 0, 100);
        var secondB = secondProvider.NextInt(choiceB, 0, 100);
        var secondA = secondProvider.NextInt(choiceA, 0, 100);

        Assert.Equal(firstA, repeatedA);
        Assert.Equal(firstA, secondA);
        Assert.Equal(firstB, secondB);
        Assert.InRange(firstA, 0, 99);
        Assert.InRange(firstB, 0, 99);
    }

    [Fact]
    public void DeterministicRandomProvider_EncodesNullCoordinatesUnambiguously()
    {
        var provider = new DeterministicRandomProvider();
        var identifier = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var noIdentifiers = new DeterministicChoiceCoordinates(
            90210,
            SimulationRuleCodes.Act,
            null,
            null,
            null,
            0);
        var emptyActor = noIdentifiers with { ActorId = Guid.Empty };
        var actor = noIdentifiers with { ActorId = identifier };
        var target = noIdentifiers with { TargetId = identifier };
        var topic = noIdentifiers with { TopicId = identifier };

        var seeds = new[]
        {
            provider.CreateChoiceSeed(noIdentifiers),
            provider.CreateChoiceSeed(emptyActor),
            provider.CreateChoiceSeed(actor),
            provider.CreateChoiceSeed(target),
            provider.CreateChoiceSeed(topic),
        };

        Assert.Equal(seeds.Length, seeds.Distinct().Count());
    }

    [Fact]
    public void M08Rules_ReturnStableUnavailableReasonsWithoutRandomness()
    {
        var throwingRandom = new ThrowingRandomProvider();
        var context = new SimulationRuleContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            CreatedAt,
            CreatedAt.AddMinutes(15),
            CreatedAt.AddMinutes(15),
            CreatedAt.AddMinutes(15),
            1m,
            1,
            42,
            SimulationInputAvailability.M08,
            throwingRandom);
        ISimulationRule[] rules =
        [
            new FollowSimulationRule(),
            new ReactSimulationRule(),
            new ReplySimulationRule(),
            new PostSimulationRule(),
            new ActSimulationRule(),
        ];

        var decisions = rules
            .OrderBy(rule => rule.Priority)
            .ThenBy(rule => rule.Code, StringComparer.Ordinal)
            .Select(rule => (rule.Code, Decision: rule.Evaluate(context)))
            .ToArray();

        Assert.Equal(
            [
                SimulationRuleCodes.Act,
                SimulationRuleCodes.Post,
                SimulationRuleCodes.Reply,
                SimulationRuleCodes.React,
                SimulationRuleCodes.Follow,
            ],
            decisions.Select(item => item.Code));
        Assert.All(decisions, item => Assert.Equal(SimulationRuleOutcome.Unavailable, item.Decision.Outcome));
        Assert.Equal(SimulationReasonCodes.GoalRelevanceUnavailable, decisions[0].Decision.ReasonCode);
        Assert.Equal(SimulationReasonCodes.GoalRelevanceUnavailable, decisions[1].Decision.ReasonCode);
        Assert.All(
            decisions.Skip(2),
            item => Assert.Equal(
                SimulationReasonCodes.RelationshipStateUnavailable,
                item.Decision.ReasonCode));
    }

    [Theory]
    [InlineData(false, false, false, SimulationReasonCodes.GoalRelevanceUnavailable)]
    [InlineData(true, false, false, SimulationReasonCodes.MoodActivationUnavailable)]
    [InlineData(true, true, false, SimulationReasonCodes.EventRelevanceUnavailable)]
    public void ActRule_UsesStableMandatoryInputPrecedence(
        bool hasGoalRelevance,
        bool hasMoodActivation,
        bool hasEventRelevance,
        string expectedReason)
    {
        var context = CreateContext(new(
            hasGoalRelevance,
            hasMoodActivation,
            hasEventRelevance,
            false,
            false));

        var decision = new ActSimulationRule().Evaluate(context);

        Assert.Equal(SimulationRuleOutcome.Unavailable, decision.Outcome);
        Assert.Equal(expectedReason, decision.ReasonCode);
    }

    [Theory]
    [InlineData(false, false, false, false, SimulationReasonCodes.GoalRelevanceUnavailable)]
    [InlineData(true, false, false, false, SimulationReasonCodes.MoodActivationUnavailable)]
    [InlineData(true, true, false, false, SimulationReasonCodes.EventRelevanceUnavailable)]
    [InlineData(true, true, true, false, SimulationReasonCodes.TopicInputsUnavailable)]
    public void PostRule_UsesStableMandatoryInputPrecedence(
        bool hasGoalRelevance,
        bool hasMoodActivation,
        bool hasEventRelevance,
        bool hasTopicInputs,
        string expectedReason)
    {
        var context = CreateContext(new(
            hasGoalRelevance,
            hasMoodActivation,
            hasEventRelevance,
            hasTopicInputs,
            false));

        var decision = new PostSimulationRule().Evaluate(context);

        Assert.Equal(SimulationRuleOutcome.Unavailable, decision.Outcome);
        Assert.Equal(expectedReason, decision.ReasonCode);
    }

    [Theory]
    [InlineData("1", 15 * TimeSpan.TicksPerMinute)]
    [InlineData("2", 30 * TimeSpan.TicksPerMinute)]
    [InlineData("0.5", 15 * TimeSpan.TicksPerMinute / 2)]
    public void WorldTimeDelta_UsesExactDecimalTicks(string scale, long expectedTicks) =>
        Assert.Equal(
            expectedTicks,
            SimulationService.CalculateWorldTimeDeltaTicks(decimal.Parse(scale, CultureInfo.InvariantCulture)));

    [Fact]
    public void WorldAndCursor_AdvanceAtomicallyRepresentableState()
    {
        var world = new GameWorld(Guid.NewGuid(), Guid.NewGuid(), "World", 42, CreatedAt);
        var state = new WorldSimulationState(Guid.NewGuid(), world.Id, CreatedAt);
        var end = CreatedAt.AddMinutes(15);
        var delta = SimulationService.CalculateWorldTimeDeltaTicks(2m);

        state.CompleteInterval(CreatedAt, end, end.AddMinutes(20));
        world.AdvanceSimulation(end, delta, end.AddMinutes(20));

        Assert.Equal(end, state.LastCompletedIntervalEnd);
        Assert.Equal(CreatedAt.AddMinutes(30), state.NextDueAt);
        Assert.Equal(end, world.LastSimulatedAt);
        Assert.Equal(CreatedAt.AddMinutes(30), world.CurrentWorldTime);
        Assert.Equal(1, state.DeterministicSequence);
    }

    [Fact]
    public void WorldSettings_DefaultValidateIanaAndPreserveExactScale()
    {
        var settings = new WorldSettings(Guid.NewGuid(), Guid.NewGuid(), CreatedAt);

        Assert.Equal("UTC", settings.DisplayTimeZoneId);
        Assert.Equal(1m, settings.TimeScale);
        settings.SetDisplayTimeZoneId("America/New_York", CreatedAt);
        settings.SetTimeScale(0.5m, CreatedAt);

        Assert.Equal("America/New_York", settings.DisplayTimeZoneId);
        Assert.Equal(0.5m, settings.TimeScale);
        Assert.Throws<ArgumentException>(() =>
            settings.SetDisplayTimeZoneId("Eastern Standard Time", CreatedAt));
        Assert.Throws<ArgumentException>(() =>
            settings.SetDisplayTimeZoneId("Not/A_Real_Zone", CreatedAt));
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.SetTimeScale(0m, CreatedAt));
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.SetTimeScale(1.00001m, CreatedAt));
    }

    [Fact]
    public void WorldTimeProjection_UsesIanaDstRulesFromUtcInstant()
    {
        var projector = new WorldTimeProjector();
        var before = projector.ToLocalWorldTime(
            new DateTimeOffset(2026, 3, 8, 6, 30, 0, TimeSpan.Zero),
            "America/New_York");
        var after = projector.ToLocalWorldTime(
            new DateTimeOffset(2026, 3, 8, 7, 30, 0, TimeSpan.Zero),
            "America/New_York");

        Assert.Equal(1, before.Hour);
        Assert.Equal(TimeSpan.FromHours(-5), before.Offset);
        Assert.Equal(3, after.Hour);
        Assert.Equal(TimeSpan.FromHours(-4), after.Offset);
    }

    [Fact]
    public void TemplateFallback_IsDeterministic()
    {
        var generator = new DeterministicTemplateGenerator();
        Assert.Equal(
            generator.Generate("post", "Avery", "technology"),
            generator.Generate("post", "Avery", "technology"));
    }

    [Theory]
    [InlineData(SimulationRunStatus.FailedRetryable)]
    [InlineData(SimulationRunStatus.FailedTerminal)]
    public void ActualRunFailure_UsesSafeErrorCodeWithoutClaimingProgress(
        SimulationRunStatus failureStatus)
    {
        var run = new SimulationRun(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SimulationRunType.ActiveTick,
            CreatedAt,
            CreatedAt.AddMinutes(15),
            1m,
            42,
            1,
            CreatedAt,
            "m08:test");

        run.Fail(failureStatus, "simulation_test_failure", CreatedAt.AddSeconds(1));

        Assert.Equal(failureStatus, run.Status);
        Assert.Equal("simulation_test_failure", run.ErrorCode);
        Assert.Equal(CreatedAt, run.ProcessedThrough);
        Assert.Throws<ArgumentException>(() =>
            new SimulationRun(
                Guid.NewGuid(),
                Guid.NewGuid(),
                SimulationRunType.ActiveTick,
                CreatedAt,
                CreatedAt.AddMinutes(15),
                1m,
                42,
                1,
                CreatedAt,
                "m08:test").Fail(failureStatus, "Unsafe Error", CreatedAt));
    }

    private static SimulationRuleContext CreateContext(SimulationInputAvailability availability) => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        CreatedAt,
        CreatedAt.AddMinutes(15),
        CreatedAt.AddMinutes(15),
        CreatedAt.AddMinutes(15),
        1m,
        1,
        42,
        availability,
        new ThrowingRandomProvider());

    private sealed class ThrowingRandomProvider : IDeterministicRandomProvider
    {
        public int NextInt(
            DeterministicChoiceCoordinates coordinates,
            int inclusiveMinimum,
            int exclusiveMaximum) =>
            throw new InvalidOperationException("Unavailable rules must not consume randomness.");
    }
}
