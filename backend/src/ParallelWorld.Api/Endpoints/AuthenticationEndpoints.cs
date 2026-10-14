using System.Globalization;
using System.Security.Claims;
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
        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
        group.MapPost("/upgrade", UpgradeAsync).RequireAuthorization();
        group.MapPost("/recovery", RecoverAsync);
        group.MapPost("/password/change", ChangePasswordAsync).RequireAuthorization();
        group.MapPost("/recovery-code/regenerate", RegenerateRecoveryCodeAsync).RequireAuthorization();
        group.MapGet("/sessions", ListSessionsAsync).RequireAuthorization();
        group.MapDelete("/sessions/{sessionId:guid}", RevokeSessionAsync).RequireAuthorization();
        group.MapPost("/logout-all", LogoutAllAsync).RequireAuthorization();
        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, HttpContext context, IAuthenticationService service, CancellationToken ct)
    {
        var errors = ValidateCredentials(request.Email, request.Password); if (errors.Count > 0) return EndpointResults.Validation(context, errors);
        var result = await service.RegisterAsync(new(request.Email, request.Password, GetIpAddress(context)), ct);
        return result.IsSuccess ? Results.Json(AccountResponse(result.Value!), statusCode: 201) : EndpointResults.Failure(context, result.Failure!);
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, HttpContext context, IAuthenticationService service, CancellationToken ct)
    {
        var errors = ValidateCredentials(request.Email, request.Password);
        if (!Guid.TryParse(request.InstallationId, out var iid) || iid == Guid.Empty) errors["installationId"] = ["A valid installation ID is required."];
        if (!string.Equals(request.Platform?.Trim(), "android", StringComparison.OrdinalIgnoreCase)) errors["platform"] = ["The initial supported platform is android."];
        if (string.IsNullOrWhiteSpace(request.AppVersion) || request.AppVersion.Trim().Length > 32) errors["appVersion"] = ["App version is required and must not exceed 32 characters."];
        if (errors.Count > 0) return EndpointResults.Validation(context, errors);
        var result = await service.LoginAsync(new(request.Email, request.Password, iid.ToString(), request.Platform!.Trim().ToLowerInvariant(), request.AppVersion!.Trim(), GetIpAddress(context)), ct);
        if (!result.IsSuccess) return EndpointResults.Failure(context, result.Failure!);
        var x = result.Value!; return Results.Ok(new { accessToken=x.Tokens.AccessToken, accessTokenExpiresAtUtc=x.Tokens.AccessTokenExpiresAt, refreshToken=x.Tokens.RefreshToken, refreshTokenExpiresAtUtc=x.Tokens.RefreshTokenExpiresAt, user=new{id=x.User.Id,accountType=x.User.AccountType}, world=EndpointResults.ToResponse(x.World)});
    }

    private static async Task<IResult> UpgradeAsync(RegisterRequest request, HttpContext context, IAuthenticationService service, CancellationToken ct)
    {
        var uid=EndpointResults.GetUserId(context.User); if(uid is null) return Unauthorized(context);
        var errors=ValidateCredentials(request.Email,request.Password); if(errors.Count>0)return EndpointResults.Validation(context,errors);
        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key) || string.IsNullOrWhiteSpace(key) || key.ToString().Length > 100)
            return EndpointResults.Validation(context, new Dictionary<string,string[]> { ["Idempotency-Key"] = ["A valid Idempotency-Key is required."] });
        var result=await service.UpgradeAsync(new(uid.Value,request.Email,request.Password,key.ToString()),ct);
        return result.IsSuccess ? Results.Ok(AccountResponse(result.Value!)) : EndpointResults.Failure(context,result.Failure!);
    }

    private static async Task<IResult> RecoverAsync(RecoveryRequest request,HttpContext context,IAuthenticationService service,CancellationToken ct)
    {
        var errors=ValidateCredentials(request.Email,request.NewPassword); if(string.IsNullOrWhiteSpace(request.RecoveryCode)||request.RecoveryCode.Length>512)errors["recoveryCode"]=["A recovery code is required."];
        if(errors.Count>0)return EndpointResults.Validation(context,errors);
        var result=await service.RecoverAsync(new(request.Email,request.RecoveryCode,request.NewPassword,GetIpAddress(context)),ct);
        return result.IsSuccess?Results.Ok(new{recoveryCode=result.Value}):EndpointResults.Failure(context,result.Failure!);
    }

    private static async Task<IResult> ChangePasswordAsync(ChangePasswordRequest request,HttpContext context,IAuthenticationService service,CancellationToken ct)
    {
        var uid=EndpointResults.GetUserId(context.User); if(uid is null)return Unauthorized(context);
        var errors=ValidatePassword(request.NewPassword); if(string.IsNullOrEmpty(request.CurrentPassword))errors["currentPassword"]=["Current password is required."];
        if(errors.Count>0)return EndpointResults.Validation(context,errors);
        var result=await service.ChangePasswordAsync(new(uid.Value,request.CurrentPassword,request.NewPassword),ct);
        return result.IsSuccess?Results.NoContent():EndpointResults.Failure(context,result.Failure!);
    }

    private static async Task<IResult> RegenerateRecoveryCodeAsync(HttpContext context,IAuthenticationService service,CancellationToken ct)
    { var uid=EndpointResults.GetUserId(context.User); if(uid is null)return Unauthorized(context); var result=await service.RegenerateRecoveryCodeAsync(uid.Value,ct); return result.IsSuccess?Results.Ok(new{recoveryCode=result.Value}):EndpointResults.Failure(context,result.Failure!); }
    private static async Task<IResult> ListSessionsAsync(HttpContext context,IAuthenticationService service,CancellationToken ct)
    { var uid=EndpointResults.GetUserId(context.User); if(uid is null)return Unauthorized(context); Guid? sid=Guid.TryParse(context.User.FindFirstValue("sid"),out var x)?x:null; var result=await service.ListSessionsAsync(uid.Value,sid,ct); return Results.Ok(new{items=result.Value}); }
    private static async Task<IResult> RevokeSessionAsync(Guid sessionId,HttpContext context,IAuthenticationService service,CancellationToken ct)
    { var uid=EndpointResults.GetUserId(context.User); if(uid is null)return Unauthorized(context); var result=await service.RevokeSessionAsync(uid.Value,sessionId,ct); return result.IsSuccess?Results.NoContent():EndpointResults.Failure(context,result.Failure!); }
    private static async Task<IResult> LogoutAllAsync(HttpContext context,IAuthenticationService service,CancellationToken ct)
    { var uid=EndpointResults.GetUserId(context.User); if(uid is null)return Unauthorized(context); await ((ISessionAdministrationService)service).RevokeAllFamiliesAsync(uid.Value,ct); return Results.NoContent(); }

    private static object AccountResponse(RegisteredAccount x)=>new{user=new{id=x.User.Id,accountType=x.User.AccountType},world=EndpointResults.ToResponse(x.World),recoveryCode=x.RecoveryCode};
    private static IResult Unauthorized(HttpContext context)=>EndpointResults.Failure(context,new("invalid_access_token",401,"Authentication is required."));
    private static Dictionary<string,string[]> ValidateCredentials(string email,string password){var e=ValidatePassword(password);var normalized=email?.Trim();if(string.IsNullOrEmpty(normalized)||normalized.Length>320||!normalized.Contains('@'))e["email"]=["A valid email address is required."];return e;}
    private static Dictionary<string,string[]> ValidatePassword(string password){var e=new Dictionary<string,string[]>(StringComparer.Ordinal);var length=string.IsNullOrEmpty(password)?0:new StringInfo(password).LengthInTextElements;if(length<10||length>128)e["password"]=["Password must contain between 10 and 128 Unicode text elements."];return e;}

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

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] public sealed record RegisterRequest(string Email,string Password);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] public sealed record LoginRequest(string Email,string Password,string InstallationId,string Platform,string AppVersion);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] public sealed record RecoveryRequest(string Email,string RecoveryCode,string NewPassword);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)] public sealed record ChangePasswordRequest(string CurrentPassword,string NewPassword);
