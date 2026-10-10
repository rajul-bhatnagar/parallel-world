using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class CatchUpSimulationTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 10, 5, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ZeroAndTwelveHourBacklog_UseOneCatchUpRunAndOldestFirstCheckpoints()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);

        await using (var initialScope = factory.Services.CreateAsyncScope())
        {
            var initial = await initialScope.ServiceProvider
                .GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
            Assert.Equal(CatchUpDisposition.NotDue, initial.Disposition);
        }

        clock.Advance(TimeSpan.FromHours(12));
        CatchUpResult result;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            result = await scope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
        }

        Assert.Equal(CatchUpDisposition.Completed, result.Disposition);
        Assert.Equal(48, result.RequestedIntervals);
        Assert.Equal(48, result.ProcessedIntervals);
        Assert.Equal(0, result.RemainingIntervals);
        Assert.Equal(2, result.ProcessedBuckets);

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var run = await db.SimulationRuns.SingleAsync(x => x.WorldId == guest.World.Id);
        var checkpoints = await db.SimulationRunCheckpoints
            .Where(x => x.WorldId == guest.World.Id)
            .OrderBy(x => x.StableOrdinal)
            .ToListAsync();
        Assert.Equal(SimulationRunType.CatchUp, run.RunType);
        Assert.Equal(SimulationRunStatus.Completed, run.Status);
        Assert.Equal(2, checkpoints.Count);
        Assert.Equal(CreatedAt, checkpoints[0].BucketStart);
        Assert.Equal(checkpoints[0].BucketEnd, checkpoints[1].BucketStart);
        Assert.Equal(CreatedAt.AddHours(12), checkpoints[1].BucketEnd);
        Assert.Equal(CreatedAt.AddHours(12),
            (await db.GameWorlds.SingleAsync(x => x.Id == guest.World.Id)).CurrentWorldTime);
    }

    [Fact]
    public async Task BucketBudget_MarksPartialAndResumeCompletesSameRun()
    {
        var clock = new MutableTimeProvider(CreatedAt.AddDays(3));
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);
        await RebaseWorldAsync(factory, guest.World.Id, CreatedAt);

        CatchUpResult first;
        CatchUpResult second;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            first = await scope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
        }
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            second = await scope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
        }

        Assert.Equal(CatchUpDisposition.Partial, first.Disposition);
        Assert.Equal(CatchUpDisposition.Completed, second.Disposition);
        Assert.Equal(first.RunId, second.RunId);
        Assert.Equal(8, first.ProcessedBuckets);
        Assert.Equal(4, second.ProcessedBuckets);
        Assert.Equal(0, second.RemainingIntervals);

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var run = await db.SimulationRuns.SingleAsync(x => x.WorldId == guest.World.Id);
        Assert.Equal(2, run.AttemptCount);
        Assert.Equal(12, await db.SimulationRunCheckpoints.CountAsync(x => x.WorldId == guest.World.Id));
        Assert.Single(await db.CatchUpSummaries.Where(x => x.WorldId == guest.World.Id).ToListAsync());
    }

    [Fact]
    public async Task CatchUp_CapturesTimeScaleAndCapsOneRunAtThirtyDays()
    {
        var clock = new MutableTimeProvider(CreatedAt.AddDays(40));
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);
        await RebaseWorldAsync(factory, guest.World.Id, CreatedAt, 2m);

        CatchUpResult result = default!;
        for (var invocation = 0; invocation < 7; invocation++)
        {
            await using var scope = factory.Services.CreateAsyncScope();
            result = await scope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
        }

        Assert.Equal(CatchUpDisposition.Completed, result.Disposition);
        Assert.Equal(30 * 24 * 4, result.RequestedIntervals);
        Assert.Equal(CreatedAt.AddDays(30), result.ProcessedThroughUtc);
        Assert.Equal(CreatedAt.AddDays(60), result.CurrentWorldTimeUtc);

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var run = await db.SimulationRuns.SingleAsync(x => x.WorldId == guest.World.Id);
        Assert.Equal(2m, run.EffectiveTimeScale);
        Assert.Equal(51, await db.SimulationRunCheckpoints.CountAsync(x => x.WorldId == guest.World.Id));
        Assert.True((await db.WorldSimulationStates.SingleAsync(x => x.WorldId == guest.World.Id)).NextDueAt <= clock.GetUtcNow());
    }

    [Fact]
    public async Task FailedSecondBucket_PreservesFirstAndRetryResumesWithoutDuplication()
    {
        var clock = new MutableTimeProvider(CreatedAt.AddHours(18));
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);
        await RebaseWorldAsync(factory, guest.World.Id, CreatedAt);

        await using (var setupScope = factory.Services.CreateAsyncScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            await db.Database.ExecuteSqlRawAsync("""
                CREATE FUNCTION fail_m15_second_bucket() RETURNS trigger AS $$
                BEGIN
                    IF NEW.stable_ordinal = 1 THEN
                        RAISE EXCEPTION 'forced m15 bucket rollback';
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER fail_m15_second_bucket_trigger
                BEFORE INSERT ON simulation_run_checkpoints
                FOR EACH ROW EXECUTE FUNCTION fail_m15_second_bucket();
                """);
        }

        try
        {
            await using var scope = factory.Services.CreateAsyncScope();
            await Assert.ThrowsAnyAsync<Exception>(() => scope.ServiceProvider
                .GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id));
        }
        finally
        {
            await using var cleanupScope = factory.Services.CreateAsyncScope();
            var db = cleanupScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            await db.Database.ExecuteSqlRawAsync("DROP TRIGGER IF EXISTS fail_m15_second_bucket_trigger ON simulation_run_checkpoints");
            await db.Database.ExecuteSqlRawAsync("DROP FUNCTION IF EXISTS fail_m15_second_bucket()");
        }

        await using (var failedScope = factory.Services.CreateAsyncScope())
        {
            var db = failedScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            var run = await db.SimulationRuns.SingleAsync(x => x.WorldId == guest.World.Id);
            Assert.Equal(SimulationRunStatus.FailedRetryable, run.Status);
            Assert.Equal(CreatedAt.AddHours(6), run.ProcessedThrough);
            Assert.Single(await db.SimulationRunCheckpoints.Where(x => x.WorldId == guest.World.Id).ToListAsync());
        }

        await using (var retryScope = factory.Services.CreateAsyncScope())
        {
            var retry = await retryScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
            Assert.Equal(CatchUpDisposition.Completed, retry.Disposition);
            Assert.Equal(2, retry.ProcessedBuckets);
        }

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(3, await assertionDb.SimulationRunCheckpoints.CountAsync(x => x.WorldId == guest.World.Id));
        Assert.Single(await assertionDb.SimulationRuns.Where(x => x.WorldId == guest.World.Id).ToListAsync());
    }

    [Fact]
    public async Task CatchUpEndpoint_IsOwnerScopedAndLatestSummaryContainsCommittedFactsOnly()
    {
        var clock = new MutableTimeProvider(CreatedAt.AddHours(6));
        await using var factory = await CreateFactoryAsync(clock);
        using var ownerClient = factory.CreateClient();
        using var ownerBootstrap = await ownerClient.BootstrapAsync(M03TestClient.NewSecret());
        var owner = (await ownerBootstrap.Content.ReadFromJsonAsync<M03GuestResponse>())!;
        ownerClient.Authenticate(owner.AccessToken);
        await RebaseWorldAsync(factory, owner.World.Id, CreatedAt);

        using var response = await ownerClient.PostAsync(
            $"/api/v1/worlds/{owner.World.Id}/catch-up", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
        {
            Assert.Equal("completed", body.RootElement.GetProperty("disposition").GetString());
            Assert.Equal(24, body.RootElement.GetProperty("processedIntervals").GetInt32());
        }

        using var latest = await ownerClient.GetAsync(
            $"/api/v1/worlds/{owner.World.Id}/catch-up/latest");
        Assert.Equal(HttpStatusCode.OK, latest.StatusCode);

        using var foreignClient = factory.CreateClient();
        using var foreignBootstrap = await foreignClient.BootstrapAsync(M03TestClient.NewSecret());
        var foreign = (await foreignBootstrap.Content.ReadFromJsonAsync<M03GuestResponse>())!;
        foreignClient.Authenticate(foreign.AccessToken);
        using var forbidden = await foreignClient.GetAsync(
            $"/api/v1/worlds/{owner.World.Id}/catch-up/latest");
        Assert.Equal(HttpStatusCode.NotFound, forbidden.StatusCode);
    }

    [Fact]
    public async Task ConcurrentSameWorldTriggers_OwnOneRunAndNeverDuplicateCheckpoints()
    {
        var clock = new MutableTimeProvider(CreatedAt.AddHours(12));
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);
        await RebaseWorldAsync(factory, guest.World.Id, CreatedAt);

        async Task<CatchUpResult> ProcessAsync()
        {
            await using var scope = factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
        }

        var results = await Task.WhenAll(ProcessAsync(), ProcessAsync());

        Assert.Contains(results, result => result.Disposition == CatchUpDisposition.Completed);
        Assert.All(results, result => Assert.Contains(
            result.Disposition,
            new[]
            {
                CatchUpDisposition.Completed,
                CatchUpDisposition.Processing,
                CatchUpDisposition.NotDue,
            }));

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Single(await db.SimulationRuns
            .Where(x => x.WorldId == guest.World.Id && x.RunType == SimulationRunType.CatchUp)
            .ToListAsync());
        var checkpoints = await db.SimulationRunCheckpoints
            .Where(x => x.WorldId == guest.World.Id)
            .ToListAsync();
        Assert.Equal(2, checkpoints.Count);
        Assert.Equal(2, checkpoints.Select(x => x.StableOrdinal).Distinct().Count());
    }

    [Theory]
    [InlineData("paused")]
    [InlineData("archived")]
    public async Task InactiveWorld_DoesNotCreateOrAdvanceCatchUp(string status)
    {
        var clock = new MutableTimeProvider(CreatedAt.AddDays(1));
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);
        await RebaseWorldAsync(factory, guest.World.Id, CreatedAt);
        await using (var setupScope = factory.Services.CreateAsyncScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE game_worlds SET status = {status} WHERE id = {guest.World.Id}");
        }

        CatchUpResult result;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            result = await scope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
        }

        Assert.Equal(CatchUpDisposition.WorldUnavailable, result.Disposition);
        await using var assertionScope = factory.Services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Empty(await assertionDb.SimulationRuns
            .Where(x => x.WorldId == guest.World.Id)
            .ToListAsync());
        Assert.Equal(CreatedAt,
            (await assertionDb.GameWorlds.SingleAsync(x => x.Id == guest.World.Id))
            .CurrentWorldTime);
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync(TimeProvider timeProvider)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        return await M03ApiFactory.CreateAsync(connectionString, timeProvider);
    }

    private static async Task<M03GuestResponse> BootstrapAsync(M03ApiFactory factory)
    {
        var response = await factory.CreateClient().BootstrapAsync(M03TestClient.NewSecret());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<M03GuestResponse>())!;
    }

    private static async Task RebaseWorldAsync(
        M03ApiFactory factory,
        Guid worldId,
        DateTimeOffset createdAt,
        decimal timeScale = 1m)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE game_worlds
            SET created_at = {createdAt}, current_world_time = {createdAt},
                last_simulated_at = {createdAt}, updated_at = {createdAt}
            WHERE id = {worldId};
            UPDATE world_simulation_states
            SET created_at = {createdAt}, updated_at = {createdAt},
                next_due_at = {createdAt.AddMinutes(15)},
                last_completed_interval_end = NULL, deterministic_sequence = 0
            WHERE world_id = {worldId};
            UPDATE world_settings SET time_scale = {timeScale}, updated_at = {createdAt}
            WHERE world_id = {worldId};
            """);
    }
}
