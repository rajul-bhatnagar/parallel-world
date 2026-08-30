using ParallelWorld.Api.Observability;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ParallelWorld.IntegrationTests;

public sealed class SensitiveDataEnricherTests
{
    [Theory]
    [InlineData("Authorization")]
    [InlineData("Cookie")]
    [InlineData("SetCookie")]
    [InlineData("DatabasePassword")]
    [InlineData("AccessToken")]
    [InlineData("RefreshToken")]
    [InlineData("SigningSecret")]
    [InlineData("api_key")]
    [InlineData("ConnectionString")]
    public void SensitiveTopLevelProperty_ThroughSerilogPipeline_IsRedacted(string propertyName)
    {
        var sink = new CollectingSink();
        using var logger = CreateLogger(sink);

        logger.Write(
            LogEventLevel.Information,
            $"Test {{{propertyName}}}",
            "private-value");

        var logEvent = Assert.Single(sink.Events);
        var value = Assert.IsType<ScalarValue>(logEvent.Properties[propertyName]);
        Assert.Equal(SensitiveDataEnricher.RedactedValue, value.Value);
        Assert.DoesNotContain("private-value", logEvent.RenderMessage(), StringComparison.Ordinal);
    }

    [Fact]
    public void NestedStructuredAndDictionaryProperties_ThroughSerilogPipeline_AreRedacted()
    {
        var sink = new CollectingSink();
        using var logger = CreateLogger(sink);
        var context = new
        {
            SafeOperation = "health-check",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer private-access-token",
                ["Cookie"] = "session=private-cookie",
                ["Set-Cookie"] = "session=private-set-cookie",
            },
            Credentials = new
            {
                ApiKey = "private-api-key",
                Password = "private-password",
            },
            Nested = new[]
            {
                new { RefreshToken = "private-refresh-token" },
            },
        };

        logger.Information("Request context {@Context}", context);

        var renderedMessage = Assert.Single(sink.Events).RenderMessage();
        Assert.Contains("health-check", renderedMessage, StringComparison.Ordinal);
        Assert.Contains(SensitiveDataEnricher.RedactedValue, renderedMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("private-", renderedMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void NonSensitiveStructuredProperty_ThroughSerilogPipeline_RemainsUsable()
    {
        var sink = new CollectingSink();
        using var logger = CreateLogger(sink);

        logger.Information("Operation {@Operation}", new { Name = "readiness", AttemptCount = 2 });

        var renderedMessage = Assert.Single(sink.Events).RenderMessage();
        Assert.Contains("readiness", renderedMessage, StringComparison.Ordinal);
        Assert.Contains("2", renderedMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void SensitiveExceptionData_ThroughSerilogPipeline_IsRedactedRecursively()
    {
        var sink = new CollectingSink();
        using var logger = CreateLogger(sink);
        var nestedData = new Dictionary<string, object?>
        {
            ["RefreshToken"] = "private-refresh-token",
        };
        var exception = new InvalidOperationException("Safe failure category");
        exception.Data["ApiKey"] = "private-api-key";
        exception.Data["Nested"] = nestedData;

        logger.Error(exception, "Operation failed");

        Assert.Single(sink.Events);
        Assert.Equal(SensitiveDataEnricher.RedactedValue, exception.Data["ApiKey"]);
        Assert.Equal(SensitiveDataEnricher.RedactedValue, nestedData["RefreshToken"]);
    }

    private static Logger CreateLogger(CollectingSink sink) =>
        new LoggerConfiguration()
            .Enrich.With<SensitiveDataEnricher>()
            .WriteTo.Sink(sink)
            .CreateLogger();

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}
