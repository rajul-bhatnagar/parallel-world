using ParallelWorld.Application.Characters;
using ParallelWorld.Domain.Characters;

namespace ParallelWorld.Api.Endpoints;

public static class CharacterEndpoints
{
    private static readonly HashSet<string> AllowedQueryParameters =
        new(StringComparer.OrdinalIgnoreCase) { "status", "limit", "cursor" };

    public static IEndpointRouteBuilder MapCharacterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/worlds/{worldId:guid}/characters")
            .RequireAuthorization();
        group.MapGet("", ListAsync);
        group.MapGet("/{characterId:guid}", GetAsync);
        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        Guid worldId,
        HttpContext context,
        ICharacterCatalogueService service,
        CancellationToken cancellationToken)
    {
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null)
        {
            return InvalidAuthentication(context);
        }

        var unknownParameter = context.Request.Query.Keys.FirstOrDefault(
            key => !AllowedQueryParameters.Contains(key));
        if (unknownParameter is not null)
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                [unknownParameter] = ["This query parameter is not supported."],
            });
        }

        var statusValue = context.Request.Query["status"].ToString();
        CharacterStatus? status = statusValue.ToLowerInvariant() switch
        {
            "" => CharacterStatus.Active,
            "active" => CharacterStatus.Active,
            "inactive" => CharacterStatus.Inactive,
            _ => null,
        };
        if (status is null)
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                ["status"] = ["Status must be active or inactive."],
            });
        }

        var limitValue = context.Request.Query["limit"].ToString();
        var limit = 20;
        if (limitValue.Length > 0 && !int.TryParse(limitValue, out limit))
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                ["limit"] = ["Limit must be an integer between 1 and 100."],
            });
        }

        if (limit is < 1 or > 100)
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                ["limit"] = ["Limit must be an integer between 1 and 100."],
            });
        }

        var result = await service.ListAsync(
            userId.Value,
            worldId,
            status,
            limit,
            context.Request.Query["cursor"].ToString(),
            cancellationToken);
        if (!result.IsSuccess)
        {
            return EndpointResults.Failure(context, result.Failure!);
        }

        return Results.Ok(new
        {
            items = result.Value!.Items.Select(ToResponse),
            result.Value.NextCursor,
            result.Value.HasMore,
        });
    }

    private static async Task<IResult> GetAsync(
        Guid worldId,
        Guid characterId,
        HttpContext context,
        ICharacterCatalogueService service,
        CancellationToken cancellationToken)
    {
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null)
        {
            return InvalidAuthentication(context);
        }

        var result = await service.GetAsync(
            userId.Value,
            worldId,
            characterId,
            cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ToResponse(result.Value!))
            : EndpointResults.Failure(context, result.Failure!);
    }

    private static IResult InvalidAuthentication(HttpContext context) =>
        EndpointResults.Failure(context, new(
            "invalid_access_token",
            StatusCodes.Status401Unauthorized,
            "Authentication is required."));

    private static object ToResponse(CharacterSummary character) => new
    {
        character.Id,
        character.DisplayName,
        character.Handle,
        character.Profession,
        character.VisibleMood,
        character.IsFollowed,
    };

    private static object ToResponse(CharacterDetails character) => new
    {
        character.Id,
        character.DisplayName,
        character.Handle,
        character.Bio,
        character.Age,
        character.Profession,
        character.Archetype,
        character.VisibleMood,
        interests = character.Interests.Select(interest => new { interest.TopicId }),
        schedule = character.Schedule.Select(item => new
        {
            item.DayOfWeek,
            startLocalTime = item.StartLocalTime.ToString("HH:mm"),
            endLocalTime = item.EndLocalTime.ToString("HH:mm"),
            item.Activity,
        }),
    };
}
