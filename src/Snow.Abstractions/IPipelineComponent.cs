using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Snow.Abstractions;

/// <summary>
/// Represents a page in the generation pipeline.
/// </summary>
public record PipelinePage(
    string SourcePath,
    string? Content = null,
    IPost? Post = null,
    IPage? Page = null,
    string? RenderedContent = null,
    string? OutputPath = null,
    PipelinePageType PageType = PipelinePageType.Unknown,
    IReadOnlyList<PipelineError>? Errors = null)
{
    public IReadOnlyList<PipelineError> Errors { get; init; } = Errors ?? [];

    /// <summary>
    /// Adds an error to this page.
    /// </summary>
    public PipelinePage WithError(PipelineError error) =>
        this with { Errors = [..Errors, error] };

    /// <summary>
    /// Adds an error to this page from an exception.
    /// </summary>
    public PipelinePage WithError(string stage, Exception exception) =>
        WithError(new PipelineError(stage, exception.Message, exception));

    /// <summary>
    /// Gets whether this page has any errors.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;

    /// <summary>
    /// Gets whether this page has any fatal errors that should stop processing.
    /// </summary>
    public bool HasFatalErrors => Errors.Any(e => e.IsFatal);
}

/// <summary>
/// Types of pages in the pipeline.
/// </summary>
public enum PipelinePageType
{
    Unknown,
    Post,
    Page,
    StaticFile,
    Asset
}

/// <summary>
/// Represents an error that occurred during pipeline processing.
/// </summary>
public record PipelineError(
    string Stage,
    string Message,
    Exception? Exception = null,
    bool IsFatal = false,
    string? SourceLocation = null);

/// <summary>
/// Delegate for pipeline stage processing.
/// </summary>
/// <param name="source">The input page stream.</param>
/// <param name="cancellationToken">Cancellation token for the operation.</param>
/// <returns>The processed page stream.</returns>
public delegate IAsyncEnumerable<PipelinePage> PagePipe(
    IAsyncEnumerable<PipelinePage> source, 
    CancellationToken cancellationToken = default);

/// <summary>
/// Interface for reading content from the file system.
/// </summary>
public interface IContentReader
{
    /// <summary>
    /// Reads all content files from the configured directories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Stream of discovered content files.</returns>
    IAsyncEnumerable<PipelinePage> ReadAllAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for writing generated content to the file system.
/// </summary>
public interface IPageWriter
{
    /// <summary>
    /// Writes a page to the output directory.
    /// </summary>
    /// <param name="page">The page to write.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task WriteAsync(PipelinePage page, CancellationToken cancellationToken = default);
}
