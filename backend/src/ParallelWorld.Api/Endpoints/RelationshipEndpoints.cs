using System.Text.Json.Serialization;
using ParallelWorld.Application.Relationships;
namespace ParallelWorld.Api.Endpoints;

public static class RelationshipEndpoints
{
    public static IEndpointRouteBuilder MapRelationshipEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var world = endpoints.MapGroup("/api/v1/worlds/{worldId:guid}").RequireAuthorization();
        var group = world.MapGroup("/relationships");
        group.MapGet("", ListAsync); group.MapGet("/{actorId:guid}", FindAsync); group.MapGet("/{actorId:guid}/history", HistoryAsync);
        group.MapPost("/{actorId:guid}/date-invitations", InviteAsync);
        group.MapGet("/{actorId:guid}/romantic-history", RomanticHistoryAsync);
        world.MapGet("/date-invitations", InvitationsAsync);
        world.MapPost("/date-invitations/{invitationId:guid}/outcome", OutcomeAsync);
        return endpoints;
    }
    private static async Task<IResult> ListAsync(Guid worldId, HttpContext context, IRelationshipService service, CancellationToken ct)
    { var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context); if (context.Request.Query.Count > 0) return BadQuery(context); var result = await service.ListAsync(userId.Value, worldId, ct); return result.IsSuccess ? Results.Ok(new { items = result.Value }) : EndpointResults.Failure(context, result.Failure!); }
    private static async Task<IResult> FindAsync(Guid worldId, Guid actorId, HttpContext context, IRelationshipService service, CancellationToken ct)
    { var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context); if (context.Request.Query.Count > 0) return BadQuery(context); var result = await service.FindAsync(userId.Value, worldId, actorId, 10, ct); return result.IsSuccess ? Results.Ok(result.Value!.Summary) : EndpointResults.Failure(context, result.Failure!); }
    private static async Task<IResult> HistoryAsync(Guid worldId, Guid actorId, HttpContext context, IRelationshipService service, CancellationToken ct)
    { var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context); var limit = 20; if (context.Request.Query.Keys.Any(x => !string.Equals(x, "limit", StringComparison.OrdinalIgnoreCase)) || context.Request.Query.TryGetValue("limit", out var raw) && (!int.TryParse(raw, out limit) || limit is < 1 or > 50)) return BadQuery(context); var result = await service.FindAsync(userId.Value, worldId, actorId, limit, ct); return result.IsSuccess ? Results.Ok(new { items = result.Value!.History }) : EndpointResults.Failure(context, result.Failure!); }
    private static async Task<IResult> InviteAsync(Guid worldId, Guid actorId, EmptyDateInvitationRequest request, HttpContext context, IRomanceService service, CancellationToken ct)
    {
        var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context);
        if (context.Request.Query.Count > 0) return BadQuery(context);
        var key = context.Request.Headers["Idempotency-Key"].ToString();
        if (key.Length is < 8 or > 100) return EndpointResults.Validation(context, new Dictionary<string, string[]> { ["Idempotency-Key"] = ["A valid 8-100 character idempotency key is required."] });
        var result = await service.InviteCharacterAsync(userId.Value, worldId, actorId, key, ct);
        return result.IsSuccess ? Results.Json(result.Value, statusCode: StatusCodes.Status201Created) : EndpointResults.Failure(context, result.Failure!);
    }
    private static async Task<IResult> InvitationsAsync(Guid worldId, HttpContext context, IRomanceService service, CancellationToken ct)
    { var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context); if (context.Request.Query.Count > 0) return BadQuery(context); var result = await service.ListAsync(userId.Value, worldId, ct); return result.IsSuccess ? Results.Ok(new { items = result.Value }) : EndpointResults.Failure(context, result.Failure!); }
    private static async Task<IResult> OutcomeAsync(Guid worldId, Guid invitationId, DateInvitationOutcomeRequest request, HttpContext context, IRomanceService service, CancellationToken ct)
    { var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context); if (context.Request.Query.Count > 0) return BadQuery(context); var result = await service.ResolveAsync(userId.Value, worldId, invitationId, request.Decision, ct); return result.IsSuccess ? Results.Ok(result.Value) : EndpointResults.Failure(context, result.Failure!); }
    private static async Task<IResult> RomanticHistoryAsync(Guid worldId, Guid actorId, HttpContext context, IRomanceService service, CancellationToken ct)
    { var userId = EndpointResults.GetUserId(context.User); if (userId is null) return Unauthorized(context); if (context.Request.Query.Count > 0) return BadQuery(context); var result = await service.HistoryAsync(userId.Value, worldId, actorId, ct); return result.IsSuccess ? Results.Ok(new { items = result.Value }) : EndpointResults.Failure(context, result.Failure!); }
    private static IResult Unauthorized(HttpContext c) => EndpointResults.Failure(c, new("invalid_access_token", 401, "Authentication is required."));
    private static IResult BadQuery(HttpContext c) => EndpointResults.Validation(c, c.Request.Query.Keys.DefaultIfEmpty("query").ToDictionary(x => x, _ => new[] { "This query parameter is not supported or invalid." }));
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] public sealed record EmptyDateInvitationRequest;
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] public sealed record DateInvitationOutcomeRequest(string Decision);
