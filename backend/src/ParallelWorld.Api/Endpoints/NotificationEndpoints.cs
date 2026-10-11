using ParallelWorld.Application.Notifications;

namespace ParallelWorld.Api.Endpoints;

public static class NotificationEndpoints
{
    private static readonly HashSet<string> PageParameters = new(StringComparer.OrdinalIgnoreCase)
        { "limit", "cursor" };

    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/worlds/{worldId:guid}/notifications")
            .RequireAuthorization();
        group.MapGet("", ListAsync);
        group.MapGet("/unread-count", UnreadCountAsync);
        group.MapPost("/{notificationId:guid}/read", ReadAsync);
        group.MapPost("/read-all", ReadAllAsync);
        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        Guid worldId, HttpContext context, INotificationService service, CancellationToken cancellationToken)
    {
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null) return Unauthorized(context);
        var page = Page(context);
        if (page.Error is not null) return page.Error;
        var result = await service.ListAsync(userId.Value, worldId, page.Limit,
            context.Request.Query["cursor"], cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { items = result.Value!.Items, result.Value.NextCursor, result.Value.HasMore })
            : EndpointResults.Failure(context, result.Failure!);
    }

    private static async Task<IResult> UnreadCountAsync(
        Guid worldId, HttpContext context, INotificationService service, CancellationToken cancellationToken)
    {
        if (context.Request.Query.Count > 0) return Unsupported(context);
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null) return Unauthorized(context);
        var result = await service.CountUnreadAsync(userId.Value, worldId, cancellationToken);
        return result.IsSuccess ? Results.Ok(new { count = result.Value })
            : EndpointResults.Failure(context, result.Failure!);
    }

    private static async Task<IResult> ReadAsync(
        Guid worldId, Guid notificationId, HttpContext context,
        INotificationService service, CancellationToken cancellationToken)
    {
        if (context.Request.Query.Count > 0) return Unsupported(context);
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null) return Unauthorized(context);
        var result = await service.MarkReadAsync(userId.Value, worldId,
            notificationId, cancellationToken);
        return result.IsSuccess ? Results.NoContent()
            : EndpointResults.Failure(context, result.Failure!);
    }

    private static async Task<IResult> ReadAllAsync(
        Guid worldId, HttpContext context, INotificationService service,
        CancellationToken cancellationToken)
    {
        if (context.Request.Query.Count > 0) return Unsupported(context);
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null) return Unauthorized(context);
        var result = await service.MarkAllReadAsync(userId.Value, worldId, cancellationToken);
        return result.IsSuccess ? Results.NoContent()
            : EndpointResults.Failure(context, result.Failure!);
    }

    private static (int Limit, IResult? Error) Page(HttpContext context)
    {
        var unknown = context.Request.Query.Keys.FirstOrDefault(x => !PageParameters.Contains(x));
        if (unknown is not null) return (0, EndpointResults.Validation(context,
            new Dictionary<string, string[]> { [unknown] = ["This query parameter is not supported."] }));
        var raw = context.Request.Query["limit"].ToString();
        var limit = 30;
        if ((raw.Length > 0 && !int.TryParse(raw, out limit)) || limit is < 1 or > 100)
            return (0, EndpointResults.Validation(context,
                new Dictionary<string, string[]> { ["limit"] = ["Limit must be an integer between 1 and 100."] }));
        return (limit, null);
    }

    private static IResult Unsupported(HttpContext context) => EndpointResults.Validation(
        context, context.Request.Query.Keys.ToDictionary(x => x,
            _ => new[] { "This query parameter is not supported." }));
    private static IResult Unauthorized(HttpContext context) => EndpointResults.Failure(
        context, new("invalid_access_token", 401, "Authentication is required."));
}
