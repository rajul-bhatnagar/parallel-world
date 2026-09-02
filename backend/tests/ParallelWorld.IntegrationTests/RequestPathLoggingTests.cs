using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class RequestPathLoggingTests
{
    [Fact]
    public async Task RealAuthenticationRequests_LogUsefulContextWithoutCredentials()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        var sink = new TestLogSink();
        await using var factory = await M03ApiFactory.CreateAsync(connectionString, logSink: sink);
        using var client = factory.CreateClient();
        var proof = M03TestClient.NewSecret();
        var cookieSecret = $"private-cookie-{Guid.NewGuid():N}";
        using var bootstrapRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/guest")
        {
            Content = JsonContent.Create(new
            {
                installationId = Guid.NewGuid().ToString(),
                platform = "android",
                appVersion = "1.0.0-test",
                guestBootstrapProof = proof,
                worldName = "Logged World",
            }),
        };
        bootstrapRequest.Headers.Add("Cookie", $"session={cookieSecret}");
        using var bootstrapResponse = await client.SendAsync(bootstrapRequest);
        var session = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.Equal(HttpStatusCode.Created, bootstrapResponse.StatusCode);
        Assert.NotNull(session);

        using var refreshResponse = await client.RefreshAsync(session.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        using var currentRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/worlds/current");
        currentRequest.Headers.Authorization = new("Bearer", session.AccessToken);
        using var currentResponse = await client.SendAsync(currentRequest);

        var formatted = sink.FormatAll();
        Assert.Equal(HttpStatusCode.OK, currentResponse.StatusCode);
        Assert.DoesNotContain(proof, formatted, StringComparison.Ordinal);
        Assert.DoesNotContain(cookieSecret, formatted, StringComparison.Ordinal);
        Assert.DoesNotContain(session.AccessToken, formatted, StringComparison.Ordinal);
        Assert.DoesNotContain(session.RefreshToken, formatted, StringComparison.Ordinal);
        Assert.Contains("/api/v1/auth/guest", formatted, StringComparison.Ordinal);
        Assert.Contains("/api/v1/auth/refresh", formatted, StringComparison.Ordinal);
        Assert.Contains("/api/v1/worlds/current", formatted, StringComparison.Ordinal);
        Assert.Contains("CorrelationId", formatted, StringComparison.Ordinal);
        Assert.Contains("RequestMethod", formatted, StringComparison.Ordinal);
    }
}

internal sealed class TestLogSink : ILogEventSink
{
    private readonly ConcurrentQueue<LogEvent> _events = new();

    public void Emit(LogEvent logEvent) => _events.Enqueue(logEvent);

    public string FormatAll()
    {
        var formatter = new JsonFormatter();
        using var writer = new StringWriter();
        foreach (var logEvent in _events)
        {
            formatter.Format(logEvent, writer);
        }

        return writer.ToString();
    }
}
