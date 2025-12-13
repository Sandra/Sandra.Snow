using System.Collections.Generic;

namespace Snow.Abstractions;

/// <summary>
/// Represents a series of related posts.
/// </summary>
public interface ISeries
{
    /// <summary>
    /// Gets the name of the series.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the current part number in the series.
    /// </summary>
    int Current { get; }

    /// <summary>
    /// Gets all parts in the series.
    /// </summary>
    IReadOnlyDictionary<int, ISeriesPart> Parts { get; }
}

/// <summary>
/// Represents a single part within a series.
/// </summary>
public interface ISeriesPart
{
    /// <summary>
    /// Gets the name/title of this part.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the URL of this part (populated during processing).
    /// </summary>
    string? Url { get; }

    /// <summary>
    /// Gets the post associated with this part (populated during processing).
    /// </summary>
    IPost? Post { get; }
}
