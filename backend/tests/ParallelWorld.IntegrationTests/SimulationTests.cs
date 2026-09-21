using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Infrastructure.Persistence;
using ParallelWorld.Simulation;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class SimulationTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task FirstTick_UsesCreationBoundaryAndPersistsFiveUnavailableEvaluations()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);

        await using (var beforeScope = factory.Services.CreateAsyncScope())
        {
            var service = beforeScope.ServiceProvider.GetRequiredService<ISimulationService>();
            var before = await service.ProcessOldestDueIntervalAsync(guest.World.Id);
            Assert.Equal(SimulationTickDisposition.NotDue, before.Disposition);
        }

        clock.Advance(TimeSpan.FromMinutes(15));
        SimulationTickResult result;
        await using (var dueScope = factory.Services.CreateAsyncScope())
        {
            var service = dueScope.ServiceProvider.GetRequiredService<ISimulationService>();
            result = await service.ProcessOldestDueIntervalAsync(guest.World.Id);
        }

        Assert.Equal(SimulationTickDisposition.Completed, result.Disposition);
        Assert.Equal(CreatedAt, result.IntervalStartUtc);
        Assert.Equal(CreatedAt.AddMinutes(15), result.IntervalEndUtc);
        Assert.Equal(CreatedAt.AddMinutes(15), result.LastCompletedIntervalEndUtc);
        Assert.Equal(CreatedAt.AddMinutes(30), result.NextDueAtUtc);
        Assert.Equal(CreatedAt.AddMinutes(15), result.CurrentWorldTimeUtc);
        Assert.Equal(
            [
                SimulationRuleCodes.Act,
                SimulationRuleCodes.Post,
                SimulationRuleCodes.Reply,
                SimulationRuleCodes.React,
                SimulationRuleCodes.Follow,
            ],
            result.Evaluations.Select(evaluation => evaluation.RuleCode));

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var run = await db.SimulationRuns.SingleAsync(item => item.WorldId == guest.World.Id);
        var evaluations = await db.SimulationRuleEvaluations
            .Where(item => item.WorldId == guest.World.Id)
            .OrderBy(item => item.RuleCode)
            .ToListAsync();
        var world = await db.GameWorlds.SingleAsync(item => item.Id == guest.World.Id);
        var state = await db.WorldSimulationStates.SingleAsync(item => item.WorldId == guest.World.Id);

        Assert.Equal(1m, run.EffectiveTimeScale);
        Assert.Equal(SimulationRunStatus.Completed, run.Status);
        Assert.Null(run.ErrorCode);
        Assert.Equal(run.IntervalEnd, run.ProcessedThrough);
        Assert.Equal(
            DeterministicSimulationIdentity.CreateRunSeed(
                world.Seed,
                run.RuleVersion,
                run.IntervalStart,
                run.IntervalEnd,
                0),
            run.Seed);
        Assert.Equal(5, evaluations.Count);
        Assert.All(evaluations, evaluation => Assert.Equal(SimulationRuleOutcome.Unavailable, evaluation.Outcome));
        Assert.Equal(2, evaluations.Count(evaluation =>
            evaluation.ReasonCode == SimulationReasonCodes.GoalRelevanceUnavailable));
        Assert.Equal(3, evaluations.Count(evaluation =>
            evaluation.ReasonCode == SimulationReasonCodes.RelationshipStateUnavailable));
        Assert.Equal(state.LastCompletedIntervalEnd, world.LastSimulatedAt);
        Assert.Equal(CreatedAt.AddMinutes(15), world.CurrentWorldTime);
        Assert.Empty(await db.SimulationActions.Where(item => item.WorldId == guest.World.Id).ToListAsync());
        Assert.Empty(await db.GameplayEvents.Where(item => item.WorldId == guest.World.Id).ToListAsync());
        Assert.Empty(await db.Posts.Where(item => item.WorldId == guest.World.Id).ToListAsync());
        Assert.Empty(await db.PostReactions.Where(item => item.WorldId == guest.World.Id).ToListAsync());
        Assert.Empty(await db.Follows.Where(item => item.WorldId == guest.World.Id).ToListAsync());
    }

    [Fact]
    public async Task ThirtyOneMinutes_ProcessesOneIntervalPerInvocationWithoutReplayAdvance()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);
        clock.Advance(TimeSpan.FromMinutes(31));

        SimulationTickResult first;
        SimulationTickResult second;
        SimulationTickResult third;
        await using (var firstScope = factory.Services.CreateAsyncScope())
        {
            first = await firstScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessOldestDueIntervalAsync(guest.World.Id);
        }
        await using (var secondScope = factory.Services.CreateAsyncScope())
        {
            second = await secondScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessOldestDueIntervalAsync(guest.World.Id);
        }
        await using (var thirdScope = factory.Services.CreateAsyncScope())
        {
            third = await thirdScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessOldestDueIntervalAsync(guest.World.Id);
        }

        Assert.Equal(SimulationTickDisposition.Completed, first.Disposition);
        Assert.Equal(CreatedAt, first.IntervalStartUtc);
        Assert.Equal(CreatedAt.AddMinutes(15), first.IntervalEndUtc);
        Assert.Equal(CreatedAt.AddMinutes(30), first.NextDueAtUtc);
        Assert.Equal(SimulationTickDisposition.Completed, second.Disposition);
        Assert.Equal(CreatedAt.AddMinutes(15), second.IntervalStartUtc);
        Assert.Equal(CreatedAt.AddMinutes(30), second.IntervalEndUtc);
        Assert.Equal(CreatedAt.AddMinutes(45), second.NextDueAtUtc);
        Assert.Equal(SimulationTickDisposition.NotDue, third.Disposition);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(2, await db.SimulationRuns.CountAsync(item => item.WorldId == guest.World.Id));
        Assert.Equal(10, await db.SimulationRuleEvaluations.CountAsync(item => item.WorldId == guest.World.Id));
        Assert.Equal(CreatedAt.AddMinutes(30),
            (await db.GameWorlds.SingleAsync(item => item.Id == guest.World.Id)).CurrentWorldTime);
    }

    [Fact]
    public async Task EffectiveTimeScale_IsCapturedAndLaterChangeAffectsOnlyNextInterval()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);
        await SetTimeScaleAsync(factory, guest.World.Id, 2m, CreatedAt);
        clock.Advance(TimeSpan.FromMinutes(31));

        await using (var firstScope = factory.Services.CreateAsyncScope())
        {
            var first = await firstScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessOldestDueIntervalAsync(guest.World.Id);
            Assert.Equal(CreatedAt.AddMinutes(30), first.CurrentWorldTimeUtc);
        }

        await SetTimeScaleAsync(factory, guest.World.Id, 0.5m, CreatedAt.AddMinutes(31));
        await using (var secondScope = factory.Services.CreateAsyncScope())
        {
            var second = await secondScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessOldestDueIntervalAsync(guest.World.Id);
            Assert.Equal(CreatedAt.AddMinutes(37).AddSeconds(30), second.CurrentWorldTimeUtc);
        }

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var runs = await db.SimulationRuns
            .Where(item => item.WorldId == guest.World.Id)
            .OrderBy(item => item.IntervalStart)
            .ToListAsync();
        Assert.Equal([2m, 0.5m], runs.Select(run => run.EffectiveTimeScale));
        Assert.Equal(CreatedAt.AddMinutes(30), runs[1].IntervalEnd);
        Assert.Equal(CreatedAt.AddMinutes(30),
            (await db.GameWorlds.SingleAsync(item => item.Id == guest.World.Id)).LastSimulatedAt);
    }

    [Fact]
    public async Task SameWorldConcurrentTriggers_CommitOneRunAndLoserDoesNotConsumeBacklog()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);
        clock.Advance(TimeSpan.FromMinutes(31));

        await using var blockerScope = factory.Services.CreateAsyncScope();
        var blockerDb = blockerScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        await using var blockerTransaction = await blockerDb.Database.BeginTransactionAsync();
        _ = await blockerDb.WorldSimulationStates.FromSqlInterpolated(
                $"SELECT * FROM world_simulation_states WHERE world_id = {guest.World.Id} FOR UPDATE")
            .SingleAsync();

        await using var firstScope = factory.Services.CreateAsyncScope();
        await using var secondScope = factory.Services.CreateAsyncScope();
        var firstTask = firstScope.ServiceProvider.GetRequiredService<ISimulationService>()
            .ProcessOldestDueIntervalAsync(guest.World.Id);
        var secondTask = secondScope.ServiceProvider.GetRequiredService<ISimulationService>()
            .ProcessOldestDueIntervalAsync(guest.World.Id);
        await Task.Delay(250);
        await blockerTransaction.CommitAsync();
        var results = await Task.WhenAll(firstTask, secondTask);

        Assert.Single(results, result => result.Disposition == SimulationTickDisposition.Completed);
        Assert.Single(results, result => result.Disposition == SimulationTickDisposition.AlreadyProcessed);
        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Single(await db.SimulationRuns.Where(item => item.WorldId == guest.World.Id).ToListAsync());
        Assert.Equal(5, await db.SimulationRuleEvaluations.CountAsync(item => item.WorldId == guest.World.Id));
        var state = await db.WorldSimulationStates.SingleAsync(item => item.WorldId == guest.World.Id);
        Assert.Equal(CreatedAt.AddMinutes(15), state.LastCompletedIntervalEnd);
        Assert.Equal(CreatedAt.AddMinutes(30), state.NextDueAt);
    }

    [Fact]
    public async Task DifferentWorlds_ProcessIndependentlyWhileAnotherWorldIsLocked()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock);
        var first = await BootstrapAsync(factory);
        var second = await BootstrapAsync(factory);
        clock.Advance(TimeSpan.FromMinutes(15));

        await using var blockerScope = factory.Services.CreateAsyncScope();
        var blockerDb = blockerScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        await using var blockerTransaction = await blockerDb.Database.BeginTransactionAsync();
        _ = await blockerDb.WorldSimulationStates.FromSqlInterpolated(
                $"SELECT * FROM world_simulation_states WHERE world_id = {first.World.Id} FOR UPDATE")
            .SingleAsync();

        await using var firstScope = factory.Services.CreateAsyncScope();
        await using var secondScope = factory.Services.CreateAsyncScope();
        var blockedTask = firstScope.ServiceProvider.GetRequiredService<ISimulationService>()
            .ProcessOldestDueIntervalAsync(first.World.Id);
        var independentTask = secondScope.ServiceProvider.GetRequiredService<ISimulationService>()
            .ProcessOldestDueIntervalAsync(second.World.Id);
        var independent = await independentTask.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(SimulationTickDisposition.Completed, independent.Disposition);
        Assert.False(blockedTask.IsCompleted);

        await blockerTransaction.CommitAsync();
        Assert.Equal(
            SimulationTickDisposition.Completed,
            (await blockedTask.WaitAsync(TimeSpan.FromSeconds(10))).Disposition);
    }

    [Fact]
    public async Task FailedEvaluationInsert_RollsBackRunCursorAndWorldTime()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);
        clock.Advance(TimeSpan.FromMinutes(15));

        await using (var setupScope = factory.Services.CreateAsyncScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            await db.Database.ExecuteSqlRawAsync("""
                CREATE FUNCTION fail_m08_evaluation() RETURNS trigger AS $$
                BEGIN
                    RAISE EXCEPTION 'forced m08 rollback';
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER fail_m08_evaluation_trigger
                BEFORE INSERT ON simulation_rule_evaluations
                FOR EACH ROW EXECUTE FUNCTION fail_m08_evaluation();
                """);
        }

        try
        {
            await using var serviceScope = factory.Services.CreateAsyncScope();
            var service = serviceScope.ServiceProvider.GetRequiredService<ISimulationService>();
            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.ProcessOldestDueIntervalAsync(guest.World.Id));
        }
        finally
        {
            await using var cleanupScope = factory.Services.CreateAsyncScope();
            var db = cleanupScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            await db.Database.ExecuteSqlRawAsync("DROP TRIGGER IF EXISTS fail_m08_evaluation_trigger ON simulation_rule_evaluations");
            await db.Database.ExecuteSqlRawAsync("DROP FUNCTION IF EXISTS fail_m08_evaluation()");
        }

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var world = await assertionDb.GameWorlds.SingleAsync(item => item.Id == guest.World.Id);
        var state = await assertionDb.WorldSimulationStates.SingleAsync(item => item.WorldId == guest.World.Id);
        Assert.Equal(CreatedAt, world.CurrentWorldTime);
        Assert.Equal(CreatedAt, world.LastSimulatedAt);
        Assert.Null(state.LastCompletedIntervalEnd);
        Assert.Equal(CreatedAt.AddMinutes(15), state.NextDueAt);
        Assert.Empty(await assertionDb.SimulationRuns.Where(item => item.WorldId == guest.World.Id).ToListAsync());
        Assert.Empty(await assertionDb.SimulationRuleEvaluations.Where(item => item.WorldId == guest.World.Id).ToListAsync());
        Assert.Empty(await assertionDb.SimulationRunCheckpoints.Where(item => item.WorldId == guest.World.Id).ToListAsync());
    }

    [Fact]
    public async Task EvaluationForeignWorldRunReference_IsRejected()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock);
        var first = await BootstrapAsync(factory);
        var second = await BootstrapAsync(factory);
        clock.Advance(TimeSpan.FromMinutes(15));
        await using (var runScope = factory.Services.CreateAsyncScope())
        {
            await runScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessOldestDueIntervalAsync(first.World.Id);
        }

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var runId = await db.SimulationRuns
            .Where(item => item.WorldId == first.World.Id)
            .Select(item => item.Id)
            .SingleAsync();
        db.SimulationRuleEvaluations.Add(new SimulationRuleEvaluation(
            Guid.NewGuid(),
            second.World.Id,
            runId,
            "TEST-01",
            SimulationRuleOutcome.Unavailable,
            "test_unavailable",
            1,
            CreatedAt));

        var failure = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, FindPostgres(failure).SqlState);
    }

    [Fact]
    public async Task M08Migration_NormalizesLegacyFirstTickAndDefaultsTimeZone()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock);
        var guest = await BootstrapAsync(factory);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        await db.Database.MigrateAsync("20260916132321_AddM07SocialActions");
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE world_simulation_states AS state
            SET next_due_at = world.created_at,
                last_completed_interval_end = NULL
            FROM game_worlds AS world
            WHERE state.world_id = world.id
              AND state.world_id = {guest.World.Id}
            """);

        await db.Database.MigrateAsync();
        db.ChangeTracker.Clear();

        var state = await db.WorldSimulationStates.SingleAsync(item => item.WorldId == guest.World.Id);
        var settings = await db.WorldSettings.SingleAsync(item => item.WorldId == guest.World.Id);
        Assert.Equal(CreatedAt.AddMinutes(15), state.NextDueAt);
        Assert.Null(state.LastCompletedIntervalEnd);
        Assert.Equal("UTC", settings.DisplayTimeZoneId);
        Assert.Contains(
            "20260920133535_AddM08RuleBasedSimulation",
            await db.Database.GetAppliedMigrationsAsync());
    }

    [Fact]
    public async Task DevelopmentTrigger_IsAuthenticatedOwnerScopedAndProcessesOneTick()
    {
        var clock = new MutableTimeProvider(CreatedAt);
        await using var factory = await CreateFactoryAsync(clock, "Development");
        using var ownerClient = factory.CreateClient();
        using var ownerBootstrap = await ownerClient.BootstrapAsync(M03TestClient.NewSecret());
        var owner = await ownerBootstrap.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(owner);
        ownerClient.Authenticate(owner.AccessToken);

        using var beforeDue = await ownerClient.PostAsync(
            $"/api/v1/dev/worlds/{owner.World.Id}/simulate",
            null);
        Assert.Equal(HttpStatusCode.Accepted, beforeDue.StatusCode);
        using (var body = JsonDocument.Parse(await beforeDue.Content.ReadAsStringAsync()))
        {
            Assert.Equal("notdue", body.RootElement.GetProperty("disposition").GetString());
        }

        clock.Advance(TimeSpan.FromMinutes(15));
        using var due = await ownerClient.PostAsync(
            $"/api/v1/dev/worlds/{owner.World.Id}/simulate",
            null);
        Assert.Equal(HttpStatusCode.Accepted, due.StatusCode);
        using (var body = JsonDocument.Parse(await due.Content.ReadAsStringAsync()))
        {
            Assert.Equal("completed", body.RootElement.GetProperty("disposition").GetString());
            Assert.Equal(5, body.RootElement.GetProperty("evaluations").GetArrayLength());
        }

        for (var request = 0; request < 3; request++)
        {
            using var accepted = await ownerClient.PostAsync(
                $"/api/v1/dev/worlds/{owner.World.Id}/simulate",
                null);
            Assert.Equal(HttpStatusCode.Accepted, accepted.StatusCode);
        }

        using var rateLimited = await ownerClient.PostAsync(
            $"/api/v1/dev/worlds/{owner.World.Id}/simulate",
            null);
        Assert.Equal(HttpStatusCode.TooManyRequests, rateLimited.StatusCode);

        using var foreignClient = factory.CreateClient();
        using var foreignBootstrap = await foreignClient.BootstrapAsync(M03TestClient.NewSecret());
        var foreign = await foreignBootstrap.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(foreign);
        foreignClient.Authenticate(foreign.AccessToken);
        using var forbidden = await foreignClient.PostAsync(
            $"/api/v1/dev/worlds/{owner.World.Id}/simulate",
            null);
        Assert.Equal(HttpStatusCode.NotFound, forbidden.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Single(await db.SimulationRuns.Where(item => item.WorldId == owner.World.Id).ToListAsync());
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync(
        TimeProvider timeProvider,
        string environmentName = "Testing")
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        return await M03ApiFactory.CreateAsync(
            connectionString,
            timeProvider,
            environmentName: environmentName);
    }

    private static async Task<M03GuestResponse> BootstrapAsync(M03ApiFactory factory)
    {
        var response = await factory.CreateClient().BootstrapAsync(M03TestClient.NewSecret());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<M03GuestResponse>())!;
    }

    private static async Task SetTimeScaleAsync(
        M03ApiFactory factory,
        Guid worldId,
        decimal scale,
        DateTimeOffset updatedAt)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var settings = await db.WorldSettings.SingleAsync(item => item.WorldId == worldId);
        settings.SetTimeScale(scale, updatedAt);
        await db.SaveChangesAsync();
    }

    private static PostgresException FindPostgres(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException!)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }

            if (current.InnerException is null)
            {
                break;
            }
        }

        throw new InvalidOperationException("A PostgreSQL exception was expected.");
    }
}
