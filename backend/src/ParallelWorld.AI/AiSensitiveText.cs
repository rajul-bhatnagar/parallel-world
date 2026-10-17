using System.Text.RegularExpressions;

namespace ParallelWorld.AI;

internal static partial class AiSensitiveText
{
    public const string RedactedValue = "[REDACTED]";

    public static AiSanitizedText Sanitize(string? value)
    {
        if (value is null)
        {
            return new(null, false);
        }

        var normalized = value.Trim();
        return ContainsSensitiveMaterial(normalized)
            ? new(RedactedValue, true)
            : new(normalized, false);
    }

    public static bool ContainsSensitiveMaterial(string value) =>
        AuthorizationPattern().IsMatch(value)
        || BearerTokenPattern().IsMatch(value)
        || JwtPattern().IsMatch(value)
        || CookieOrSessionPattern().IsMatch(value)
        || ConnectionComponentPattern().IsMatch(value)
        || CredentialAssignmentPattern().IsMatch(value)
        || PrivateKeyMarkerPattern().IsMatch(value);

    [GeneratedRegex(
        "(?i)\\bAuthorization\\s*:\\s*(Bearer|Basic)\\s+[^\\s,;]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex AuthorizationPattern();

    [GeneratedRegex(
        "(?i)\\bBearer\\s+[A-Za-z0-9._~+/-]+=*",
        RegexOptions.CultureInvariant)]
    private static partial Regex BearerTokenPattern();

    [GeneratedRegex(
        "\\beyJ[A-Za-z0-9_-]*\\.[A-Za-z0-9_-]+\\.[A-Za-z0-9_-]+\\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex JwtPattern();

    [GeneratedRegex(
        "(?i)(\\b(Set-Cookie|Cookie)\\s*:|\\b(session(id|token)?|auth[_-]?token)\\s*=)",
        RegexOptions.CultureInvariant)]
    private static partial Regex CookieOrSessionPattern();

    [GeneratedRegex(
        "(?i)\\b(Host|Server|Data Source|Database|Initial Catalog|User ID|Username|Password|Pwd)\\s*=",
        RegexOptions.CultureInvariant)]
    private static partial Regex ConnectionComponentPattern();

    [GeneratedRegex(
        "(?i)\\b(password|pwd|secret|client[_-]?secret|api[_-]?key|access[_-]?token|refresh[_-]?token|guest[_-]?bootstrap[_-]?proof)\\s*[:=]",
        RegexOptions.CultureInvariant)]
    private static partial Regex CredentialAssignmentPattern();

    [GeneratedRegex(
        "(?i)-----BEGIN [A-Z ]*PRIVATE KEY-----",
        RegexOptions.CultureInvariant)]
    private static partial Regex PrivateKeyMarkerPattern();
}

internal sealed record AiSanitizedText(string? Value, bool SensitiveDetected);
