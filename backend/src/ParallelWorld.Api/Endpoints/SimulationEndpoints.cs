using ParallelWorld.Application.Simulation;
using ParallelWorld.Application.Worlds;

namespace ParallelWorld.Api.Endpoints;

public static class SimulationEndpoints
{
    public static IEndpointRouteBuilder MapCatchUpEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/worlds/{worldId:guid}/catch-up")
            .RequireAuthorization()
            .RequireRateLimiting("catch-up");
        group.MapPost("", CatchUpAsync);
        group.MapGet("/latest", LatestCatchUpSummaryAsync);
        return endpoints;
    }

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

    private static async Task<IResult> CatchUpAsync(
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
                "invalid_access_token", StatusCodes.Status401Unauthorized,
                "Authentication is required."));
        }
        if (context.Request.Query.Count > 0)
        {
            return EndpointResults.Validation(context,
                context.Request.Query.Keys.ToDictionary(
                    key => key, _ => new[] { "CatchUp does not accept client-selected ranges." }));
        }

        var ownedWorld = await worldService.GetAsync(userId.Value, worldId, cancellationToken);
        if (!ownedWorld.IsSuccess)
        {
            return EndpointResults.Failure(context, ownedWorld.Failure!);
        }

        var result = await simulationService.ProcessCatchUpAsync(worldId, cancellationToken);
        return result.Disposition is CatchUpDisposition.Partial or CatchUpDisposition.Processing
            ? Results.Accepted(value: CatchUpPayload(result))
            : Results.Ok(CatchUpPayload(result));
    }

    private static async Task<IResult> LatestCatchUpSummaryAsync(
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
                "invalid_access_token", StatusCodes.Status401Unauthorized,
                "Authentication is required."));
        }
        if (context.Request.Query.Count > 0)
        {
            return EndpointResults.Validation(context,
                context.Request.Query.Keys.ToDictionary(
                    key => key, _ => new[] { "This query parameter is not supported." }));
        }

        var ownedWorld = await worldService.GetAsync(userId.Value, worldId, cancellationToken);
        if (!ownedWorld.IsSuccess)
        {
            return EndpointResults.Failure(context, ownedWorld.Failure!);
        }
        var summary = await simulationService.GetLatestCatchUpSummaryAsync(
            worldId, cancellationToken);
        return summary is null ? Results.NoContent() : Results.Ok(summary);
    }

    private static object CatchUpPayload(CatchUpResult result) => new
    {
        disposition = result.Disposition.ToString().ToLowerInvariant(),
        result.RunId,
        result.Status,
        result.RequestedIntervals,
        result.ProcessedIntervals,
        result.RemainingIntervals,
        result.ProcessedBuckets,
        result.ProcessedThroughUtc,
        result.NextDueAtUtc,
        result.CurrentWorldTimeUtc,
        result.Summary,
        result.ErrorCode,
    };
}
