using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Simulation;

internal sealed class SimulationRepository(ParallelWorldDbContext dbContext) : ISimulationRepository
{
    public Task<SimulationWorldSnapshot?> FindSnapshotAsync(
        Guid worldId,
        CancellationToken cancellationToken) =>
        (from world in dbContext.GameWorlds.AsNoTracking()
         join settings in dbContext.WorldSettings.AsNoTracking() on world.Id equals settings.WorldId
         join state in dbContext.WorldSimulationStates.AsNoTracking() on world.Id equals state.WorldId
         where world.Id == worldId
         select new SimulationWorldSnapshot(
             world.Id,
             world.Status,
             world.CreatedAt,
             world.CurrentWorldTime,
             world.LastSimulatedAt,
             state.LastCompletedIntervalEnd,
             state.NextDueAt,
             settings.TimeScale,
             settings.RuleVersion,
             settings.DisplayTimeZoneId))
        .SingleOrDefaultAsync(cancellationToken);

    public async Task<LockedSimulationWorld?> LockWorldAsync(
        Guid worldId,
        CancellationToken cancellationToken)
    {
        var state = await dbContext.WorldSimulationStates.FromSqlInterpolated(
                $"SELECT * FROM world_simulation_states WHERE world_id = {worldId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (state is null)
        {
            return null;
        }

        var world = await dbContext.GameWorlds.FromSqlInterpolated(
                $"SELECT * FROM game_worlds WHERE id = {worldId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        var settings = await dbContext.WorldSettings.FromSqlInterpolated(
                $"SELECT * FROM world_settings WHERE world_id = {worldId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        return world is null || settings is null
            ? null
            : new LockedSimulationWorld(world, settings, state);
    }

    public Task<SimulationRun?> FindRunAsync(
        Guid worldId,
        Guid runId,
        CancellationToken cancellationToken) =>
        dbContext.SimulationRuns.SingleOrDefaultAsync(
            run => run.WorldId == worldId && run.Id == runId,
            cancellationToken);

    public void AddRun(
        SimulationRun run,
        SimulationRunCheckpoint checkpoint,
        IReadOnlyCollection<SimulationRuleEvaluation> evaluations)
    {
        dbContext.SimulationRuns.Add(run);
        dbContext.SimulationRunCheckpoints.Add(checkpoint);
        dbContext.SimulationRuleEvaluations.AddRange(evaluations);
    }
}
