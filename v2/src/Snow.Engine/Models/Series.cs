using System.Collections.Generic;
using System.Linq;
using Snow.Abstractions;

namespace Snow.Engine.Models;

/// <summary>
/// Represents a series of related posts.
/// </summary>
public class Series : ISeries
{
    private readonly SortedDictionary<int, SeriesPart> _parts;

    public Series()
    {
        Name = string.Empty;
        _parts = new SortedDictionary<int, SeriesPart>();
    }

    public Series(string name) : this()
    {
        Name = name;
    }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public int Current { get; set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, ISeriesPart> Parts => _parts.ToDictionary(kvp => kvp.Key, kvp => (ISeriesPart)kvp.Value);

    /// <summary>
    /// Gets the mutable parts collection for building the series.
    /// </summary>
    public SortedDictionary<int, SeriesPart> MutableParts => _parts;
}

/// <summary>
/// Represents a single part within a series.
/// </summary>
public class SeriesPart : ISeriesPart
{
    public SeriesPart()
    {
        Name = string.Empty;
    }

    public SeriesPart(string name) : this()
    {
        Name = name;
    }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string? Url { get; set; }

    /// <inheritdoc />
    public IPost? Post { get; set; }

    /// <summary>
    /// Checks if this part has a URL.
    /// </summary>
    /// <returns>True if URL is not null or empty, false otherwise.</returns>
    public bool HasUrl() => !string.IsNullOrWhiteSpace(Url);
}
