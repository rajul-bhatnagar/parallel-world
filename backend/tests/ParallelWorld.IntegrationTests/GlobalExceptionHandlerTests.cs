using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParallelWorld.Api.Errors;

namespace ParallelWorld.IntegrationTests;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_DoesNotWriteSensitiveExceptionTextToApplicationLog()
    {
        var problemDetailsService = new AcceptingProblemDetailsService();
        var logger = new CapturingLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(problemDetailsService, logger);
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "exception-test-123",
        };
        var exception = new InvalidOperationException(
            "Password=private-test-password;RefreshToken=private-test-refresh-token");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Null(logger.Exception);
        Assert.DoesNotContain("private-test-password", logger.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("private-test-refresh-token", logger.Message, StringComparison.Ordinal);
    }

    private sealed class AcceptingProblemDetailsService : IProblemDetailsService
    {
        public ValueTask WriteAsync(ProblemDetailsContext context) => ValueTask.CompletedTask;

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context) =>
            ValueTask.FromResult(true);
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public Exception? Exception { get; private set; }

        public string Message { get; private set; } = string.Empty;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Exception = exception;
            Message = formatter(state, exception);
        }
    }
}
