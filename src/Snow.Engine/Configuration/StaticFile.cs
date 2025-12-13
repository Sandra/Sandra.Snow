using System.Text.Json.Serialization;
using Snow.Abstractions;

namespace Snow.Engine.Configuration;

/// <summary>
/// Represents a static file that should be processed during site generation.
/// </summary>
public class StaticFile : IStaticFile
{
    /// <inheritdoc />
    public string File { get; set; } = string.Empty;

    /// <inheritdoc />
    public string? Loop { get; set; }

    /// <inheritdoc />
    public string? Title { get; set; }
}
