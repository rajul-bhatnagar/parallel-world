using Microsoft.AspNetCore.Mvc;

namespace ParallelWorld.Api.Errors;

public static class ProblemDetailsConfiguration
{
    public static void Configure(ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = context =>
        {
            var problemDetails = context.ProblemDetails;
            problemDetails.Instance ??= context.HttpContext.Request.Path;
            problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            problemDetails.Extensions.TryAdd("code", GetCode(problemDetails.Status));
        };
    }

    private static string GetCode(int? status) => status switch
    {
        StatusCodes.Status404NotFound => "resource_not_available",
        StatusCodes.Status500InternalServerError => "unexpected_error",
        _ => "request_failed",
    };
}
