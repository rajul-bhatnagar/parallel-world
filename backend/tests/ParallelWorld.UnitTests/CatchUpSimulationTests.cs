using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Simulation;

namespace ParallelWorld.UnitTests;

public sealed class CatchUpSimulationTests
{
    private static readonly DateTimeOffset Start =
        new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TenDays_UsesOldestDailyThenNewestSixHourBuckets()
    {
        var buckets = SimulationService.BuildCatchUpBuckets(Start, Start.AddDays(10));

        Assert.Equal(31, buckets.Count);
        Assert.All(buckets.Take(3), bucket => Assert.Equal(CatchUpBucketKind.Daily, bucket.Kind));
        Assert.All(buckets.Skip(3), bucket => Assert.Equal(CatchUpBucketKind.Detailed, bucket.Kind));
        Assert.Equal(Start, buckets[0].Start);
        Assert.Equal(Start.AddDays(3), buckets[2].End);
        Assert.Equal(Start.AddDays(10), buckets[^1].End);
        Assert.True(buckets.Zip(buckets.Skip(1)).All(pair => pair.First.End == pair.Second.Start));
    }

    [Fact]
    public void ShortCatchUp_UsesOneBoundedRunWithDetailedBuckets()
    {
        var run = NewCatchUpRun(Start.AddHours(18));
        var buckets = SimulationService.BuildCatchUpBuckets(run.IntervalStart, run.IntervalEnd);

        Assert.Equal(SimulationRunType.CatchUp, run.RunType);
        Assert.Equal(72, run.RequestedIntervalCount);
        Assert.Equal(3, buckets.Count);
        Assert.All(buckets, bucket => Assert.Equal(TimeSpan.FromHours(6), bucket.End - bucket.Start));
    }

    [Fact]
    public void PartialRun_ResumesFromCommittedBucketWithoutChangingIdentityOrSeed()
    {
        var run = NewCatchUpRun(Start.AddHours(18));
        var owner = Guid.NewGuid();
        Assert.True(run.TryClaim(owner, Start, TimeSpan.FromMinutes(2)));

        run.AdvanceCatchUp(owner, Start, Start.AddHours(6));
        run.MarkPartial(owner, Start.AddMinutes(1));
        var seed = run.Seed;
        var id = run.Id;
        var secondOwner = Guid.NewGuid();
        Assert.True(run.TryClaim(secondOwner, Start.AddMinutes(2), TimeSpan.FromMinutes(2)));
        run.AdvanceCatchUp(secondOwner, Start.AddHours(6), Start.AddHours(12));

        Assert.Equal(id, run.Id);
        Assert.Equal(seed, run.Seed);
        Assert.Equal(48, run.ProcessedIntervalCount);
        Assert.Equal(24, run.RemainingIntervalCount);
        Assert.Equal(Start.AddHours(12), run.ProcessedThrough);
    }

    [Fact]
    public void ActiveLease_RejectsConcurrentCatchUpOwner()
    {
        var run = NewCatchUpRun(Start.AddHours(6));
        var owner = Guid.NewGuid();

        Assert.True(run.TryClaim(owner, Start, TimeSpan.FromMinutes(2)));
        Assert.False(run.TryClaim(Guid.NewGuid(), Start.AddMinutes(1), TimeSpan.FromMinutes(2)));
        Assert.True(run.TryClaim(Guid.NewGuid(), Start.AddMinutes(3), TimeSpan.FromMinutes(2)));
    }

    [Fact]
    public void CatchUpRange_AdvancesCursorAndWorldTimeByLogicalIntervalCount()
    {
        var world = new GameWorld(Guid.NewGuid(), Guid.NewGuid(), "World", 42, Start);
        var state = new WorldSimulationState(Guid.NewGuid(), world.Id, Start);
        var end = Start.AddHours(6);
        var intervalCount = 24;

        state.CompleteCatchUpRange(Start, end, Start.AddHours(7));
        world.AdvanceSimulation(
            end,
            SimulationService.CalculateWorldTimeDeltaTicks(2m, intervalCount),
            Start.AddHours(7));

        Assert.Equal(end, state.LastCompletedIntervalEnd);
        Assert.Equal(end.AddMinutes(15), state.NextDueAt);
        Assert.Equal(intervalCount, state.DeterministicSequence);
        Assert.Equal(Start.AddHours(12), world.CurrentWorldTime);
    }

    [Fact]
    public void CatchUpCommittedIdentities_AreStableAcrossRetry()
    {
        var worldId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var first = DeterministicSimulationIdentity.CreateCatchUpEventId(
            worldId, runId, 2, sourceId, targetId, true);
        var replay = DeterministicSimulationIdentity.CreateCatchUpEventId(
            worldId, runId, 2, sourceId, targetId, true);
        var nextBucket = DeterministicSimulationIdentity.CreateCatchUpEventId(
            worldId, runId, 3, sourceId, targetId, true);

        Assert.Equal(first, replay);
        Assert.NotEqual(first, nextBucket);
    }

    [Fact]
    public void SummaryFallback_UsesOnlyCommittedFactCount()
    {
        var summary = new CatchUpSummary(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Start, Start, "m15:test:summary");

        summary.Update(Start.AddHours(6), CatchUpSummaryStatus.Partial, Start.AddMinutes(1), 0);
        Assert.Equal("Your world advanced. There are no major changes to report.", summary.Text);

        summary.Update(Start.AddHours(12), CatchUpSummaryStatus.Completed, Start.AddMinutes(2), 20);
        Assert.Equal("Your world advanced with 20 meaningful changes.", summary.Text);
    }

    private static SimulationRun NewCatchUpRun(DateTimeOffset end) => new(
        DeterministicSimulationIdentity.CreateCatchUpRunId(Guid.Empty, Start, end, 1),
        Guid.Empty,
        SimulationRunType.CatchUp,
        Start,
        end,
        1m,
        42,
        1,
        Start,
        "m15:test");
}
