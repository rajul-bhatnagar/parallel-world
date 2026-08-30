using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ParallelWorld.Api.Observability;

namespace ParallelWorld.IntegrationTests;

public sealed class ApiFoundationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Liveness_ReturnsMinimalSafeResponseAndCorrelationId()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/live");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body.GetProperty("status").GetString());
        Assert.Single(body.EnumerateObject());
        Assert.True(response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var values));
        Assert.NotEmpty(Assert.Single(values));
    }

    [Fact]
    public async Task CorrelationId_WhenValid_IsReturnedAndUsedByProblemDetails()
    {
        const string correlationId = "test-correlation-123";
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/_testing/error");
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(correlationId, Assert.Single(response.Headers.GetValues(CorrelationIdMiddleware.HeaderName)));
        Assert.Equal(correlationId, body.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task CorrelationId_WhenInvalid_IsReplaced()
    {
        const string invalidCorrelationId = "unsafe value with spaces";
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, invalidCorrelationId);

        using var response = await client.SendAsync(request);
        var effectiveCorrelationId = Assert.Single(
            response.Headers.GetValues(CorrelationIdMiddleware.HeaderName));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(invalidCorrelationId, effectiveCorrelationId);
        Assert.Matches("^[a-f0-9]{32}$", effectiveCorrelationId);
    }

    [Fact]
    public async Task UnhandledException_ReturnsSanitizedProblemDetails()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/_testing/error");
        var contentType = response.Content.Headers.ContentType?.MediaType;
        var bodyText = await response.Content.ReadAsStringAsync();
        using var body = JsonDocument.Parse(bodyText);
        var root = body.RootElement;

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", contentType);
        Assert.Equal("unexpected_error", root.GetProperty("code").GetString());
        Assert.Equal("An unexpected error occurred.", root.GetProperty("detail").GetString());
        Assert.False(bodyText.Contains("private-test-password", StringComparison.Ordinal));
        Assert.False(bodyText.Contains("private-test-refresh-token", StringComparison.Ordinal));
        Assert.False(bodyText.Contains("InvalidOperationException", StringComparison.Ordinal));
        Assert.False(bodyText.Contains("stack", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Readiness_WhenPostgreSqlIsUnavailable_ReturnsOnlyStatus()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/ready");
        var bodyText = await response.Content.ReadAsStringAsync();
        using var body = JsonDocument.Parse(bodyText);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", body.RootElement.GetProperty("status").GetString());
        Assert.Single(body.RootElement.EnumerateObject());
        Assert.False(bodyText.Contains("postgres", StringComparison.OrdinalIgnoreCase));
        Assert.False(bodyText.Contains("connection", StringComparison.OrdinalIgnoreCase));
    }
}
