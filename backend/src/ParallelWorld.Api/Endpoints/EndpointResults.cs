using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ParallelWorld.Application.Common;

namespace ParallelWorld.Api.Endpoints;

internal static class EndpointResults
{
    public static Guid? GetUserId(ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue("sub"), out var userId) ? userId : null;

    public static IResult Failure(HttpContext context, ServiceFailure failure)
    {
        if (failure.RetryAfterSeconds is int retryAfterSeconds)
        {
            context.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
        }

        var extensions = new Dictionary<string, object?>
        {
            ["code"] = failure.Code,
            ["traceId"] = context.TraceIdentifier,
        };
        if (failure.RetryAfterSeconds is int seconds)
        {
            extensions["retryAfterSeconds"] = seconds;
        }

        return Results.Problem(
            statusCode: failure.StatusCode,
            title: failure.Title,
            type: $"https://errors.parallel-world.example/{failure.Code.Replace('_', '-')}",
            instance: context.Request.Path,
            extensions: extensions);
    }

    public static IResult Validation(
        HttpContext context,
        IReadOnlyDictionary<string, string[]> errors) => Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Request validation failed.",
            type: "https://errors.parallel-world.example/validation-failed",
            instance: context.Request.Path,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = "validation_failed",
                ["traceId"] = context.TraceIdentifier,
                ["errors"] = errors,
            });

    public static object ToResponse(ParallelWorld.Application.Authentication.WorldSummary world) => new
    {
        id = world.Id,
        name = world.Name,
        status = world.Status,
        currentGameTimeUtc = world.CurrentGameTime,
        player = new
        {
            actorId = world.Player.ActorId,
            displayName = world.Player.DisplayName,
        },
        createdAtUtc = world.CreatedAt,
    };
}
