using System.Text.Json.Serialization;
using ParallelWorld.Application.Social;

namespace ParallelWorld.Api.Endpoints;

public static class SocialFeedEndpoints
{
    private static readonly HashSet<string> AllowedFeedQueryParameters =
        new(StringComparer.OrdinalIgnoreCase) { "limit", "cursor" };

    public static IEndpointRouteBuilder MapSocialFeedEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var worldGroup = endpoints.MapGroup("/api/v1/worlds/{worldId:guid}")
            .RequireAuthorization();
        worldGroup.MapGet("/feed", GetFeedAsync);
        worldGroup.MapPost("/posts", CreatePostAsync);
        return endpoints;
    }

    private static async Task<IResult> GetFeedAsync(
        Guid worldId,
        HttpContext context,
        ISocialFeedService service,
        CancellationToken cancellationToken)
    {
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null)
        {
            return InvalidAuthentication(context);
        }

        var unknownParameter = context.Request.Query.Keys.FirstOrDefault(
            key => !AllowedFeedQueryParameters.Contains(key));
        if (unknownParameter is not null)
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                [unknownParameter] = ["This query parameter is not supported."],
            });
        }

        var limitValue = context.Request.Query["limit"].ToString();
        var limit = 20;
        if (limitValue.Length > 0 && !int.TryParse(limitValue, out limit)
            || limit is < 1 or > 50)
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                ["limit"] = ["Limit must be an integer between 1 and 50."],
            });
        }

        var result = await service.GetFeedAsync(
            userId.Value,
            worldId,
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

    private static async Task<IResult> CreatePostAsync(
        Guid worldId,
        CreatePostRequest request,
        HttpContext context,
        ISocialFeedService service,
        CancellationToken cancellationToken)
    {
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null)
        {
            return InvalidAuthentication(context);
        }

        var idempotencyKey = context.Request.Headers["Idempotency-Key"].ToString();
        var errors = new Dictionary<string, string[]>();
        if (!IsValidIdempotencyKey(idempotencyKey))
        {
            errors["Idempotency-Key"] = ["A valid 8-100 character idempotency key is required."];
        }

        if (request.ClientPostId == Guid.Empty)
        {
            errors["clientPostId"] = ["A non-empty client post ID is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            errors["content"] = ["Content is required and must not exceed 500 characters."];
        }

        if (errors.Count > 0)
        {
            return EndpointResults.Validation(context, errors);
        }

        var result = await service.CreatePostAsync(
            new CreatePostCommand(
                userId.Value,
                worldId,
                request.Content,
                request.ClientPostId,
                idempotencyKey),
            cancellationToken);
        if (!result.IsSuccess)
        {
            return EndpointResults.Failure(context, result.Failure!);
        }

        var created = result.Value!;
        if (created.IsIdempotencyReplay)
        {
            context.Response.Headers["Idempotency-Replayed"] = "true";
        }

        return Results.Json(
            ToResponse(created.Post),
            statusCode: StatusCodes.Status201Created);
    }

    private static object ToResponse(FeedPost post) => new
    {
        post.Id,
        post.WorldId,
        author = new
        {
            post.Author.ActorId,
            post.Author.DisplayName,
            post.Author.Handle,
            post.Author.ActorType,
        },
        post.Content,
        post.CreatedAtUtc,
        parent = post.ParentPostId is Guid parentPostId ? new { id = parentPostId } : null,
        counts = new { post.Counts.Likes, post.Counts.Replies },
        post.Visibility,
    };

    private static IResult InvalidAuthentication(HttpContext context) =>
        EndpointResults.Failure(context, new(
            "invalid_access_token",
            StatusCodes.Status401Unauthorized,
            "Authentication is required."));

    private static bool IsValidIdempotencyKey(string value) =>
        value.Length is >= 8 and <= 100
        && value.All(character => char.IsAsciiLetterOrDigit(character)
            || character is '.' or '_' or ':' or '-');
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreatePostRequest(string Content, Guid ClientPostId);
