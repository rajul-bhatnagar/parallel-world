using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace ParallelWorld.IntegrationTests;

internal static class M03TestClient
{
    public static string NewSecret() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    public static Task<HttpResponseMessage> BootstrapAsync(
        this HttpClient client,
        string proof,
        string? installationId = null,
        string worldName = "Test World") => client.PostAsJsonAsync("/api/v1/auth/guest", new
        {
            installationId = installationId ?? Guid.NewGuid().ToString(),
            platform = "android",
            appVersion = "1.0.0-test",
            guestBootstrapProof = proof,
            worldName,
        });

    public static Task<HttpResponseMessage> RefreshAsync(
        this HttpClient client,
        string refreshToken) => client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken });

    public static void Authenticate(this HttpClient client, string accessToken) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
}

internal sealed record M03GuestResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    M03UserResponse User,
    M03WorldResponse World);

internal sealed record M03TokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);

internal sealed record M03UserResponse(Guid Id, string AccountType);

internal sealed record M03WorldResponse(
    Guid Id,
    string Name,
    string Status,
    DateTimeOffset CurrentGameTimeUtc,
    M03PlayerResponse Player,
    DateTimeOffset CreatedAtUtc);

internal sealed record M03PlayerResponse(Guid ActorId, string DisplayName);
