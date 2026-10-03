using ParallelWorld.Application.Relationships;
namespace ParallelWorld.Api.Endpoints;

public static class RelationshipEndpoints
{
    public static IEndpointRouteBuilder MapRelationshipEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/worlds/{worldId:guid}/relationships").RequireAuthorization();
        group.MapGet("", ListAsync); group.MapGet("/{actorId:guid}", FindAsync); group.MapGet("/{actorId:guid}/history", HistoryAsync); return endpoints;
    }
    private static async Task<IResult> ListAsync(Guid worldId, HttpContext context, IRelationshipService service, CancellationToken ct)
    { var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context); if (context.Request.Query.Count > 0) return BadQuery(context); var result = await service.ListAsync(userId.Value, worldId, ct); return result.IsSuccess ? Results.Ok(new { items = result.Value }) : EndpointResults.Failure(context, result.Failure!); }
    private static async Task<IResult> FindAsync(Guid worldId, Guid actorId, HttpContext context, IRelationshipService service, CancellationToken ct)
    { var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context); if (context.Request.Query.Count > 0) return BadQuery(context); var result = await service.FindAsync(userId.Value, worldId, actorId, 10, ct); return result.IsSuccess ? Results.Ok(result.Value!.Summary) : EndpointResults.Failure(context, result.Failure!); }
    private static async Task<IResult> HistoryAsync(Guid worldId, Guid actorId, HttpContext context, IRelationshipService service, CancellationToken ct)
    { var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context); var limit = 20; if (context.Request.Query.Keys.Any(x => !string.Equals(x, "limit", StringComparison.OrdinalIgnoreCase)) || context.Request.Query.TryGetValue("limit", out var raw) && (!int.TryParse(raw, out limit) || limit is < 1 or > 50)) return BadQuery(context); var result = await service.FindAsync(userId.Value, worldId, actorId, limit, ct); return result.IsSuccess ? Results.Ok(new { items = result.Value!.History }) : EndpointResults.Failure(context, result.Failure!); }
    private static IResult Unauthorized(HttpContext c) => EndpointResults.Failure(c, new("invalid_access_token", 401, "Authentication is required."));
    private static IResult BadQuery(HttpContext c) => EndpointResults.Validation(c, c.Request.Query.Keys.DefaultIfEmpty("query").ToDictionary(x => x, _ => new[] { "This query parameter is not supported or invalid." }));
}
