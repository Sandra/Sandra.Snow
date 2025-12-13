namespace Snow.Abstractions;

/// <summary>
/// Represents a static file that should be processed during site generation.
/// </summary>
public interface IStaticFile
{
    /// <summary>
    /// Gets the file name to process.
    /// </summary>
    string File { get; }

    /// <summary>
    /// Gets the loop/processor type for this file.
    /// </summary>
    string? Loop { get; }

    /// <summary>
    /// Gets the title for this file (optional).
    /// </summary>
    string? Title { get; }
}
