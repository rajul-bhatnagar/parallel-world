using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using ParallelWorld.Application.Authentication;

namespace ParallelWorld.Infrastructure.Authentication;

internal sealed class OpaqueSecretService : IOpaqueSecretService
{
    public string Generate() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    public string Hash(string value) =>
        WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
