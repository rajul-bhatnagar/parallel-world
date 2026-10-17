using System.Text.Json.Serialization;
using ParallelWorld.Application.Messaging;

namespace ParallelWorld.Api.Endpoints;

public static class MessagingEndpoints
{
    private static readonly HashSet<string> PageParameters = new(StringComparer.OrdinalIgnoreCase) { "limit", "cursor" };

    public static IEndpointRouteBuilder MapMessagingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/worlds/{worldId:guid}/conversations").RequireAuthorization();
        group.MapGet("", ListAsync);
        group.MapPost("/direct", DirectAsync);
        group.MapGet("/{conversationId:guid}", GetAsync);
        group.MapGet("/{conversationId:guid}/messages", MessagesAsync);
        group.MapPost("/{conversationId:guid}/messages", SendAsync);
        group.MapPost("/{conversationId:guid}/read", ReadAsync);
        return endpoints;
    }

    private static async Task<IResult> ListAsync(Guid worldId, HttpContext context, IMessagingService service, CancellationToken ct)
    {
        var auth = User(context); if (auth.Error is not null) return auth.Error;
        var page = Page(context); if (page.Error is not null) return page.Error;
        var result = await service.ListAsync(auth.Id, worldId, page.Limit, context.Request.Query["cursor"], ct);
        return result.IsSuccess ? Results.Ok(new { items = result.Value!.Items.Select(Summary), result.Value.NextCursor, result.Value.HasMore }) : EndpointResults.Failure(context, result.Failure!);
    }

    private static async Task<IResult> DirectAsync(Guid worldId, DirectConversationRequest request, HttpContext context, IMessagingService service, CancellationToken ct)
    {
        var auth = User(context); if (auth.Error is not null) return auth.Error;
        var result = await service.CreateOrGetDirectAsync(auth.Id, worldId, request.CharacterId, context.Request.Headers["Idempotency-Key"].ToString(), ct);
        if (!result.IsSuccess) return EndpointResults.Failure(context, result.Failure!);
        return Results.Json(Details(result.Value!.Conversation), statusCode: result.Value.Created ? 201 : 200);
    }

    private static async Task<IResult> GetAsync(Guid worldId, Guid conversationId, HttpContext context, IMessagingService service, CancellationToken ct)
    {
        var auth = User(context); if (auth.Error is not null) return auth.Error;
        if (context.Request.Query.Count > 0) return Unsupported(context);
        var result = await service.GetAsync(auth.Id, worldId, conversationId, ct);
        return result.IsSuccess ? Results.Ok(Details(result.Value!)) : EndpointResults.Failure(context, result.Failure!);
    }

    private static async Task<IResult> MessagesAsync(Guid worldId, Guid conversationId, HttpContext context, IMessagingService service, CancellationToken ct)
    {
        var auth = User(context); if (auth.Error is not null) return auth.Error;
        var page = Page(context); if (page.Error is not null) return page.Error;
        var result = await service.GetMessagesAsync(auth.Id, worldId, conversationId, page.Limit, context.Request.Query["cursor"], ct);
        return result.IsSuccess ? Results.Ok(new { items = result.Value!.Items.Select(Message), result.Value.NextCursor, result.Value.HasMore }) : EndpointResults.Failure(context, result.Failure!);
    }

    private static async Task<IResult> SendAsync(Guid worldId, Guid conversationId, SendMessageRequest request, HttpContext context, IMessagingService service, CancellationToken ct)
    {
        var auth = User(context); if (auth.Error is not null) return auth.Error;
        if (request.ReplyToMessageId is not null) return EndpointResults.Validation(context, new Dictionary<string, string[]> { ["replyToMessageId"] = ["Message replies are not supported in M11."] });
        var result = await service.SendAsync(auth.Id, worldId, conversationId, request.Body, request.ClientMessageId, context.Request.Headers["Idempotency-Key"].ToString(), ct);
        if (!result.IsSuccess) return EndpointResults.Failure(context, result.Failure!);
        if (result.Value!.IsReplay) context.Response.Headers["Idempotency-Replayed"] = "true";
        return Results.Json(new { message = Message(result.Value.Message), result.Value.CharacterReplyStatus }, statusCode: 201);
    }

    private static async Task<IResult> ReadAsync(Guid worldId, Guid conversationId, ReadConversationRequest request, HttpContext context, IMessagingService service, CancellationToken ct)
    {
        var auth = User(context); if (auth.Error is not null) return auth.Error;
        var result = await service.MarkReadAsync(auth.Id, worldId, conversationId, request.LastReadMessageId, ct);
        return result.IsSuccess ? Results.NoContent() : EndpointResults.Failure(context, result.Failure!);
    }

    private static object Summary(ConversationSummary x) => new { x.Id, x.Type, character = new { id = x.Character.CharacterId, actorId = x.Character.ActorId, x.Character.DisplayName, x.Character.Handle }, x.CreatedAtUtc, x.LastMessageAtUtc, x.LastMessagePreview, x.UnreadCount, x.CharacterReplyStatus };
    private static object Details(ConversationDetails x) => new
    {
        x.Conversation.Id,
        x.Conversation.Type,
        character = new { id = x.Conversation.Character.CharacterId, actorId = x.Conversation.Character.ActorId, x.Conversation.Character.DisplayName, x.Conversation.Character.Handle },
        x.Conversation.CreatedAtUtc,
        x.Conversation.LastMessageAtUtc,
        x.Conversation.LastMessagePreview,
        x.Conversation.UnreadCount,
        x.Conversation.CharacterReplyStatus,
        x.PlayerActorId,
        x.LastReadMessageId,
        x.LastReadAtUtc,
    };
    private static object Message(ConversationMessage x) => new { x.Id, x.ConversationId, x.SenderActorId, x.SenderType, body = x.Body, x.CreatedAtUtc, x.DeliveryStatus, x.ClientMessageId };
    private static (Guid Id, IResult? Error) User(HttpContext c) { var id = EndpointResults.GetUserId(c.User); return id is null ? (Guid.Empty, EndpointResults.Failure(c, new("invalid_access_token", 401, "Authentication is required."))) : (id.Value, null); }
    private static (int Limit, IResult? Error) Page(HttpContext c)
    {
        var unknown = c.Request.Query.Keys.FirstOrDefault(x => !PageParameters.Contains(x));
        if (unknown is not null) return (0, EndpointResults.Validation(c, new Dictionary<string, string[]> { [unknown] = ["This query parameter is not supported."] }));
        var raw = c.Request.Query["limit"].ToString(); var limit = 20;
        if (raw.Length > 0 && !int.TryParse(raw, out limit) || limit is < 1 or > 50) return (0, EndpointResults.Validation(c, new Dictionary<string, string[]> { ["limit"] = ["Limit must be an integer between 1 and 50."] }));
        return (limit, null);
    }
    private static IResult Unsupported(HttpContext c) => EndpointResults.Validation(c, c.Request.Query.Keys.ToDictionary(x => x, _ => new[] { "This query parameter is not supported." }));
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] public sealed record DirectConversationRequest(Guid CharacterId);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] public sealed record SendMessageRequest(string Body, Guid ClientMessageId, Guid? ReplyToMessageId);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] public sealed record ReadConversationRequest(Guid LastReadMessageId);
