using Microsoft.AspNetCore.Http;
using ParallelWorld.Api.Observability;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ParallelWorld.IntegrationTests;

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AddsCorrelationIdToDownstreamLogContextAndDisposesScope()
    {
        const string correlationId = "request-scope-123";
        var sink = new CollectingSink();
        using var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();
        var middleware = new CorrelationIdMiddleware(_ =>
        {
            logger.Information("Downstream request event");
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;

        await middleware.InvokeAsync(context);
        logger.Information("Outside request event");

        Assert.Equal(2, sink.Events.Count);
        var requestValue = Assert.IsType<ScalarValue>(sink.Events[0].Properties["CorrelationId"]);
        Assert.Equal(correlationId, requestValue.Value);
        Assert.DoesNotContain("CorrelationId", sink.Events[1].Properties.Keys);
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}
