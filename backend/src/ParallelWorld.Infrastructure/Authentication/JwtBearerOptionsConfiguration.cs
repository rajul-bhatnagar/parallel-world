using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ParallelWorld.Infrastructure.Configuration;

namespace ParallelWorld.Infrastructure.Authentication;

internal sealed class JwtBearerOptionsConfiguration(
    JwtKeyMaterial keyMaterial,
    IOptions<AuthenticationOptions> optionsAccessor,
    TimeProvider timeProvider) : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthenticationOptions _options = optionsAccessor.Value;

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        Configure(options);
    }

    public void Configure(JwtBearerOptions options)
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.FromSeconds(_options.ClockSkewSeconds),
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            ValidAudience = _options.Audience,
            ValidIssuer = _options.Issuer,
            IssuerSigningKeyResolver = (_, _, keyId, _) =>
                keyMaterial.ValidationKeys.Where(key => key.KeyId == keyId),
            LifetimeValidator = (notBefore, expires, _, parameters) =>
            {
                if (expires is null)
                {
                    return false;
                }

                var now = timeProvider.GetUtcNow().UtcDateTime;
                return (notBefore is null || notBefore.Value <= now.Add(parameters.ClockSkew))
                    && expires.Value >= now.Subtract(parameters.ClockSkew);
            },
            NameClaimType = "sub",
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var subject = context.Principal?.FindFirst("sub")?.Value;
                var tokenId = context.Principal?.FindFirst("jti")?.Value;
                if (!Guid.TryParse(subject, out _) || !Guid.TryParse(tokenId, out _))
                {
                    context.Fail("The access token is missing required identity claims.");
                }

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var problemDetailsService = context.HttpContext.RequestServices
                    .GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Authentication is required.",
                        Type = "https://errors.parallel-world.example/invalid-access-token",
                        Instance = context.Request.Path,
                        Extensions =
                        {
                            ["code"] = "invalid_access_token",
                            ["traceId"] = context.HttpContext.TraceIdentifier,
                        },
                    },
                });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                var problemDetailsService = context.HttpContext.RequestServices
                    .GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Access is denied.",
                        Type = "https://errors.parallel-world.example/access-denied",
                        Instance = context.Request.Path,
                        Extensions =
                        {
                            ["code"] = "access_denied",
                            ["traceId"] = context.HttpContext.TraceIdentifier,
                        },
                    },
                });
            },
        };
    }
}
