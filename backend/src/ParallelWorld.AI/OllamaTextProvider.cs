using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ParallelWorld.AI;

public sealed class OllamaTextProvider(HttpClient httpClient) : IAiTextProvider
{
    public const int MaximumResponseBytes = 65_536;

    public async Task<AiProviderResult> GenerateAsync(
        AiProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new OllamaGenerateRequest(
            request.Model,
            request.Prompt.SystemInstruction,
            request.Prompt.UserPrompt,
            false,
            new(request.Prompt.MaxOutputLength));

        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
            {
                Content = JsonContent.Create(body),
            };
            using var response = await httpClient.SendAsync(
                message,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return AiProviderResult.Failure(
                    FailureCode(response.StatusCode),
                    IsTransient(response.StatusCode));
            }

            if (response.Content.Headers.ContentLength is > MaximumResponseBytes)
            {
                return AiProviderResult.Failure("response_too_large", transient: false);
            }

            OllamaGenerateResponse? payload;
            try
            {
                await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var bounded = new MemoryStream(MaximumResponseBytes);
                var buffer = new byte[4096];
                while (true)
                {
                    var read = await content.ReadAsync(buffer, cancellationToken);
                    if (read == 0)
                    {
                        break;
                    }

                    if (bounded.Length + read > MaximumResponseBytes)
                    {
                        return AiProviderResult.Failure("response_too_large", transient: false);
                    }

                    bounded.Write(buffer, 0, read);
                }

                payload = JsonSerializer.Deserialize<OllamaGenerateResponse>(bounded.GetBuffer().AsSpan(0, (int)bounded.Length));
            }
            catch (Exception exception) when (exception is JsonException or NotSupportedException or IOException)
            {
                return AiProviderResult.Failure("malformed_response", transient: false);
            }

            if (payload is null || string.IsNullOrWhiteSpace(payload.Response))
            {
                return AiProviderResult.Failure("empty_response", transient: false);
            }

            return AiProviderResult.Success(
                payload.Response,
                payload.PromptEvalCount,
                payload.EvalCount);
        }
        catch (HttpRequestException)
        {
            return AiProviderResult.Failure("provider_unavailable", transient: true);
        }
    }

    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout
        || (int)statusCode >= 500;

    private static string FailureCode(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.NotFound => "model_unavailable",
        HttpStatusCode.RequestTimeout => "provider_timeout",
        HttpStatusCode.TooManyRequests => "provider_resource_limited",
        _ when (int)statusCode >= 500 => "provider_unavailable",
        _ => "provider_rejected",
    };

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("system")] string System,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("options")] OllamaGenerateOptions Options);

    private sealed record OllamaGenerateOptions(
        [property: JsonPropertyName("num_predict")] int NumPredict);

    private sealed record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string? Response,
        [property: JsonPropertyName("prompt_eval_count")] int? PromptEvalCount,
        [property: JsonPropertyName("eval_count")] int? EvalCount);
}

public static class OllamaHttpHandlerFactory
{
    public static HttpClientHandler Create() => new()
    {
        AllowAutoRedirect = false,
    };
}
