using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ParallelWorld.Application.Authentication;
using ParallelWorld.Infrastructure.Configuration;

namespace ParallelWorld.Infrastructure.Authentication;

internal sealed class AccessTokenIssuer(
    JwtKeyMaterial keyMaterial,
    IOptions<AuthenticationOptions> optionsAccessor,
    TimeProvider timeProvider) : IAccessTokenIssuer
{
    private readonly AuthenticationOptions _options = optionsAccessor.Value;

    public IssuedAccessToken Issue(Guid userId)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var identity = new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ]);
        var descriptor = new SecurityTokenDescriptor
        {
            Audience = _options.Audience,
            Expires = expiresAt.UtcDateTime,
            IssuedAt = now.UtcDateTime,
            Issuer = _options.Issuer,
            NotBefore = now.UtcDateTime,
            SigningCredentials = keyMaterial.SigningCredentials,
            Subject = identity,
        };

        return new IssuedAccessToken(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }
}
