using System.Collections.Generic;
using System.IO;

namespace Snow.Abstractions;

/// <summary>
/// Context provided to content processors containing all data needed for processing.
/// </summary>
public interface IProcessingContext
{
    /// <summary>
    /// The static file configuration being processed.
    /// </summary>
    IStaticFile StaticFile { get; }

    /// <summary>
    /// All posts in the site.
    /// </summary>
    IReadOnlyList<IPost> Posts { get; }

    /// <summary>
    /// All pages in the site.
    /// </summary>
    IReadOnlyList<IPage> Pages { get; }

    /// <summary>
    /// All draft posts in the site.
    /// </summary>
    IReadOnlyList<IPost> Drafts { get; }

    /// <summary>
    /// All categories with their posts.
    /// </summary>
    IReadOnlyList<ICategory> Categories { get; }

    /// <summary>
    /// Archive information grouped by year and month.
    /// </summary>
    IReadOnlyDictionary<int, IReadOnlyDictionary<int, IReadOnlyList<IPost>>> Archives { get; }

    /// <summary>
    /// Site configuration settings.
    /// </summary>
    ISiteSettings Settings { get; }

    /// <summary>
    /// Template renderer for generating HTML output.
    /// </summary>
    ITemplateRenderer TemplateRenderer { get; }

    /// <summary>
    /// File provider for accessing files in the theme and content directories.
    /// </summary>
    IFileProvider FileProvider { get; }

    /// <summary>
    /// Output directory for generated files.
    /// </summary>
    string OutputDirectory { get; }
}
