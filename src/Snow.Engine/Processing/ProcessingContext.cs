using System.Collections.Generic;
using System.IO;
using Snow.Abstractions;

namespace Snow.Engine.Processing;

/// <summary>
/// Implementation of processing context containing all data needed for content processing.
/// </summary>
public class ProcessingContext : IProcessingContext
{
    public ProcessingContext(
        IStaticFile staticFile,
        IReadOnlyList<IPost> posts,
        IReadOnlyList<IPage> pages,
        IReadOnlyList<IPost> drafts,
        IReadOnlyList<ICategory> categories,
        IReadOnlyDictionary<int, IReadOnlyDictionary<int, IReadOnlyList<IPost>>> archives,
        ISiteSettings settings,
        ITemplateRenderer templateRenderer,
        IFileProvider fileProvider,
        string outputDirectory)
    {
        StaticFile = staticFile;
        Posts = posts;
        Pages = pages;
        Drafts = drafts;
        Categories = categories;
        Archives = archives;
        Settings = settings;
        TemplateRenderer = templateRenderer;
        FileProvider = fileProvider;
        OutputDirectory = outputDirectory;
    }

    /// <inheritdoc />
    public IStaticFile StaticFile { get; }

    /// <inheritdoc />
    public IReadOnlyList<IPost> Posts { get; }

    /// <inheritdoc />
    public IReadOnlyList<IPage> Pages { get; }

    /// <inheritdoc />
    public IReadOnlyList<IPost> Drafts { get; }

    /// <inheritdoc />
    public IReadOnlyList<ICategory> Categories { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, IReadOnlyDictionary<int, IReadOnlyList<IPost>>> Archives { get; }

    /// <inheritdoc />
    public ISiteSettings Settings { get; }

    /// <inheritdoc />
    public ITemplateRenderer TemplateRenderer { get; }

    /// <inheritdoc />
    public IFileProvider FileProvider { get; }

    /// <inheritdoc />
    public string OutputDirectory { get; }
}
