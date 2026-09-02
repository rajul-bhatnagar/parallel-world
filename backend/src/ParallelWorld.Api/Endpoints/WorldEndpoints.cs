using System.Text.Json.Serialization;
using ParallelWorld.Application.Worlds;

namespace ParallelWorld.Api.Endpoints;

public static class WorldEndpoints
{
    public static IEndpointRouteBuilder MapWorldEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/worlds").RequireAuthorization();
        group.MapPost("", CreateAsync);
        group.MapGet("", ListAsync);
        group.MapGet("/current", GetCurrentAsync);
        group.MapGet("/{worldId:guid}", GetAsync);
        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateWorldRequest request,
        HttpContext context,
        IWorldService worldService,
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

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 80)
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                ["name"] = ["World name is required and must not exceed 80 characters."],
            });
        }

        var idempotencyKey = context.Request.Headers["Idempotency-Key"].ToString();
        if (!IsValidIdempotencyKey(idempotencyKey))
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                ["Idempotency-Key"] = ["A valid 8-100 character idempotency key is required."],
            });
        }

        var normalizedName = request.Name.Trim();
        var result = await worldService.CreateAsync(
            new CreateWorldCommand(userId.Value, normalizedName),
            cancellationToken);
        if (!result.IsSuccess)
        {
            return EndpointResults.Failure(context, result.Failure!);
        }

        var world = result.Value!;
        return Results.Created($"/api/v1/worlds/{world.Id}", EndpointResults.ToResponse(world));
    }

    private static async Task<IResult> ListAsync(
        HttpContext context,
        IWorldService worldService,
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

        var worlds = await worldService.ListAsync(userId.Value, cancellationToken);
        return Results.Ok(new
        {
            items = worlds.Select(EndpointResults.ToResponse),
            nextCursor = (string?)null,
            hasMore = false,
        });
    }

    private static async Task<IResult> GetCurrentAsync(
        HttpContext context,
        IWorldService worldService,
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

        var result = await worldService.GetCurrentAsync(userId.Value, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(EndpointResults.ToResponse(result.Value!))
            : EndpointResults.Failure(context, result.Failure!);
    }

    private static async Task<IResult> GetAsync(
        Guid worldId,
        HttpContext context,
        IWorldService worldService,
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

        var result = await worldService.GetAsync(userId.Value, worldId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(EndpointResults.ToResponse(result.Value!))
            : EndpointResults.Failure(context, result.Failure!);
    }

    private static bool IsValidIdempotencyKey(string value) =>
        value.Length is >= 8 and <= 100
        && value.All(character => char.IsAsciiLetterOrDigit(character)
            || character is '.' or '_' or ':' or '-');
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateWorldRequest(string Name);
