using ParallelWorld.Application.Simulation;
using ParallelWorld.Application.Worlds;

namespace ParallelWorld.Api.Endpoints;

public static class SimulationEndpoints
{
    public static IEndpointRouteBuilder MapDevelopmentSimulationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/dev/worlds/{worldId:guid}/simulate", SimulateAsync)
            .RequireAuthorization()
            .RequireRateLimiting("development-simulation");
        return endpoints;
    }

    private static async Task<IResult> SimulateAsync(
        Guid worldId,
        HttpContext context,
        IWorldService worldService,
        ISimulationService simulationService,
        CancellationToken cancellationToken)
    {
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null)
        {
            return EndpointResults.Failure(context, new(
                "invalid_access_token",
                StatusCodes.Status401Unauthorized,
                "Authentication is required."));
        }

        var ownedWorld = await worldService.GetAsync(userId.Value, worldId, cancellationToken);
        if (!ownedWorld.IsSuccess)
        {
            return EndpointResults.Failure(context, ownedWorld.Failure!);
        }

        var result = await simulationService.ProcessOldestDueIntervalAsync(worldId, cancellationToken);
        return Results.Accepted(value: new
        {
            disposition = result.Disposition.ToString().ToLowerInvariant(),
            result.RunId,
            result.IntervalStartUtc,
            result.IntervalEndUtc,
            result.LastCompletedIntervalEndUtc,
            result.NextDueAtUtc,
            result.CurrentWorldTimeUtc,
            evaluations = result.Evaluations.Select(evaluation => new
            {
                evaluation.Id,
                evaluation.RuleCode,
                outcome = evaluation.Outcome.ToLowerInvariant(),
                evaluation.ReasonCode,
            }),
            result.ErrorCode,
        });
    }
}
