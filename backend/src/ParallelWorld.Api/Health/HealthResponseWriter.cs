using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ParallelWorld.Api.Health;

public static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(new HealthResponse(report.Status.ToString()), SerializerOptions),
            context.RequestAborted);
    }

    private sealed record HealthResponse(string Status);
}
