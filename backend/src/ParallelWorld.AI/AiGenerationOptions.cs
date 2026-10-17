using System.Text.RegularExpressions;

namespace ParallelWorld.AI;

public sealed partial class AiGenerationOptions
{
    public const string SectionName = "AI:Generation";
    public const int MaximumContentCharacters = 500;

    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "qwen3:4b";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    public int MaxOutputLength { get; set; } = MaximumContentCharacters;
    public string PromptTemplateVersion { get; set; } = "m09-v1";

    public static bool IsValid(AiGenerationOptions options) =>
        Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
        && baseUri.Scheme is "http" or "https"
        && string.IsNullOrEmpty(baseUri.UserInfo)
        && string.IsNullOrEmpty(baseUri.Query)
        && !string.IsNullOrWhiteSpace(options.Model)
        && options.Model.Length <= 100
        && options.Timeout >= TimeSpan.FromSeconds(1)
        && options.Timeout <= TimeSpan.FromMinutes(2)
        && options.MaxOutputLength is >= 40 and <= MaximumContentCharacters
        && PromptTemplateVersionPattern().IsMatch(options.PromptTemplateVersion);

    [GeneratedRegex("^[a-z0-9][a-z0-9.-]{0,39}$", RegexOptions.CultureInvariant)]
    private static partial Regex PromptTemplateVersionPattern();
}
