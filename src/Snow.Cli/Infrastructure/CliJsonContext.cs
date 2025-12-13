using System.Text.Json.Serialization;
using Snow.Engine.Configuration;
using Snow.Engine.Markdown;

namespace Snow.Cli.Infrastructure;

/// <summary>
/// JSON serialization context for CLI operations.
/// Uses source generators for AOT/trimming compatibility.
/// </summary>
[JsonSerializable(typeof(SiteSettings))]
[JsonSerializable(typeof(StaticFile))]
[JsonSerializable(typeof(StaticFile[]))]
[JsonSerializable(typeof(MarkdownConfiguration))]
[JsonSerializable(typeof(SyntaxHighlightingConfiguration))]
[JsonSerializable(typeof(LiveReloadMessage))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
public partial class CliJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Message sent to browser for live reload.
/// </summary>
public class LiveReloadMessage
{
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string[]? Files { get; set; }
}
