using System.Diagnostics;
using System.Text.RegularExpressions;
using Serilog.Context;

namespace ParallelWorld.Api.Observability;

public sealed partial class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var suppliedValue = context.Request.Headers[HeaderName].ToString();
        var correlationId = IsValid(suppliedValue)
            ? suppliedValue
            : Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static bool IsValid(string value) =>
        value.Length is >= 8 and <= 64 && CorrelationIdPattern().IsMatch(value);

    [GeneratedRegex("^[A-Za-z0-9._:-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex CorrelationIdPattern();
}
