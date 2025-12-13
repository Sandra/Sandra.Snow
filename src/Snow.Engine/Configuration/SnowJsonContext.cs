using System.Collections.Generic;
using System.Text.Json.Serialization;
using Snow.Engine.Incremental;
using Snow.Engine.Markdown;

namespace Snow.Engine.Configuration;

/// <summary>
/// JSON serialization context for AOT/trimming compatibility.
/// Uses source generators instead of reflection-based serialization.
/// </summary>
[JsonSerializable(typeof(SiteSettings))]
[JsonSerializable(typeof(StaticFile))]
[JsonSerializable(typeof(StaticFile[]))]
[JsonSerializable(typeof(MarkdownConfiguration))]
[JsonSerializable(typeof(SyntaxHighlightingConfiguration))]
[JsonSerializable(typeof(DependencyCache))]
[JsonSerializable(typeof(BuildNode))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
public partial class SnowJsonContext : JsonSerializerContext
{
}
