namespace ParallelWorld.Api.Configuration;

public sealed class DeploymentOptions
{
    public const string SectionName = "Deployment";
    public const int DefaultMaxRequestBodyBytes = 1_048_576;

    public string[] AllowedHosts { get; set; } = [];
    public string[] CorsAllowedOrigins { get; set; } = [];
    public bool ForwardedHeadersEnabled { get; set; }
    public int MaxRequestBodyBytes { get; set; } = DefaultMaxRequestBodyBytes;

    public static bool IsValid(DeploymentOptions options, bool hardenedEnvironment)
    {
        if (options.MaxRequestBodyBytes is < 4_096 or > 10_485_760)
        {
            return false;
        }

        if (!hardenedEnvironment)
        {
            return true;
        }

        return options.ForwardedHeadersEnabled
            && options.AllowedHosts.Length is > 0 and <= 16
            && options.AllowedHosts.All(IsValidHost)
            && options.CorsAllowedOrigins.Length <= 16
            && options.CorsAllowedOrigins.All(IsValidOrigin);
    }

    private static bool IsValidHost(string value) =>
        !string.IsNullOrWhiteSpace(value)
        && value != "*"
        && Uri.CheckHostName(value) != UriHostNameType.Unknown;

    private static bool IsValidOrigin(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && string.IsNullOrEmpty(uri.UserInfo)
        && uri.AbsolutePath == "/"
        && string.IsNullOrEmpty(uri.Query)
        && string.IsNullOrEmpty(uri.Fragment);
}
