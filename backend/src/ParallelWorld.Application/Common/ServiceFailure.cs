namespace ParallelWorld.Application.Common;

public sealed record ServiceFailure(
    string Code,
    int StatusCode,
    string Title,
    int? RetryAfterSeconds = null);
