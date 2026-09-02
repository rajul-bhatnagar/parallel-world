using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using ParallelWorld.Application.Authentication;

namespace ParallelWorld.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth");
        group.MapPost("/guest", BootstrapGuestAsync);
        group.MapPost("/refresh", RefreshAsync);
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();
        return endpoints;
    }

    private static async Task<IResult> BootstrapGuestAsync(
        GuestBootstrapRequest request,
        HttpContext context,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return EndpointResults.Validation(context, errors);
        }

        var command = new GuestBootstrapCommand(
            Guid.Parse(request.InstallationId).ToString(),
            request.Platform.Trim().ToLowerInvariant(),
            request.AppVersion.Trim(),
            request.GuestBootstrapProof,
            request.WorldName.Trim(),
            GetIpAddress(context));
        var result = await authenticationService.BootstrapGuestAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return EndpointResults.Failure(context, result.Failure!);
        }

        var session = result.Value!;
        var response = new
        {
            accessToken = session.Tokens.AccessToken,
            accessTokenExpiresAtUtc = session.Tokens.AccessTokenExpiresAt,
            refreshToken = session.Tokens.RefreshToken,
            refreshTokenExpiresAtUtc = session.Tokens.RefreshTokenExpiresAt,
            user = new { id = session.User.Id, accountType = session.User.AccountType },
            world = EndpointResults.ToResponse(session.World),
        };
        return session.IsRecovery
            ? Results.Ok(response)
            : Results.Json(response, statusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshSessionRequest request,
        HttpContext context,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        if (context.Request.Headers.ContainsKey("Idempotency-Key"))
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                ["Idempotency-Key"] = ["Idempotency-Key is not accepted for refresh."],
            });
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken) || request.RefreshToken.Length > 512)
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                ["refreshToken"] = ["A valid refresh token is required."],
            });
        }

        var result = await authenticationService.RefreshAsync(
            new RefreshSessionCommand(request.RefreshToken, GetIpAddress(context)),
            cancellationToken);
        if (!result.IsSuccess)
        {
            return EndpointResults.Failure(context, result.Failure!);
        }

        var tokens = result.Value!;
        return Results.Ok(new
        {
            accessToken = tokens.AccessToken,
            accessTokenExpiresAtUtc = tokens.AccessTokenExpiresAt,
            refreshToken = tokens.RefreshToken,
            refreshTokenExpiresAtUtc = tokens.RefreshTokenExpiresAt,
        });
    }

    private static async Task<IResult> LogoutAsync(
        LogoutSessionRequest request,
        HttpContext context,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var userId = EndpointResults.GetUserId(context.User);
        if (userId is null)
        {
            return EndpointResults.Failure(context, new(
                "invalid_access_token",
                StatusCodes.Status401Unauthorized,
                "Authentication is required."));
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken) || request.RefreshToken.Length > 512)
        {
            return EndpointResults.Validation(context, new Dictionary<string, string[]>
            {
                ["refreshToken"] = ["A valid refresh token is required."],
            });
        }

        var result = await authenticationService.LogoutAsync(
            new LogoutSessionCommand(userId.Value, request.RefreshToken, GetIpAddress(context)),
            cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : EndpointResults.Failure(context, result.Failure!);
    }

    private static Dictionary<string, string[]> Validate(GuestBootstrapRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (!Guid.TryParse(request.InstallationId, out var installationId) || installationId == Guid.Empty)
        {
            errors["installationId"] = ["A valid installation ID is required."];
        }

        if (!string.Equals(request.Platform?.Trim(), "android", StringComparison.OrdinalIgnoreCase))
        {
            errors["platform"] = ["The initial supported platform is android."];
        }

        if (string.IsNullOrWhiteSpace(request.AppVersion) || request.AppVersion.Trim().Length > 32)
        {
            errors["appVersion"] = ["App version is required and must not exceed 32 characters."];
        }

        if (!HasAtLeast256Bits(request.GuestBootstrapProof))
        {
            errors["guestBootstrapProof"] =
                ["A cryptographically random bootstrap proof of at least 256 bits is required."];
        }

        if (string.IsNullOrWhiteSpace(request.WorldName) || request.WorldName.Trim().Length > 80)
        {
            errors["worldName"] = ["World name is required and must not exceed 80 characters."];
        }

        return errors;
    }

    private static bool HasAtLeast256Bits(string? proof)
    {
        if (string.IsNullOrWhiteSpace(proof) || proof.Length > 512)
        {
            return false;
        }

        try
        {
            return WebEncoders.Base64UrlDecode(proof).Length >= 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string GetIpAddress(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record GuestBootstrapRequest(
    string InstallationId,
    string Platform,
    string AppVersion,
    string GuestBootstrapProof,
    string WorldName);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RefreshSessionRequest(string RefreshToken);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record LogoutSessionRequest(string RefreshToken);
