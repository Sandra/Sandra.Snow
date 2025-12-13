using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;

namespace Snow.Engine.Pipeline;

/// <summary>
/// Writes generated pages to the file system.
/// </summary>
public class PageWriter : IPageWriter
{
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<PageWriter> _logger;

    public PageWriter(IFileProvider fileProvider, ILogger<PageWriter> logger)
    {
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task WriteAsync(PipelinePage page, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(page.OutputPath))
        {
            _logger.LogWarning("No output path specified for {SourcePath}", page.SourcePath);
            return;
        }

        if (string.IsNullOrEmpty(page.RenderedContent))
        {
            _logger.LogWarning("No rendered content for {SourcePath}", page.SourcePath);
            return;
        }

        try
        {
            _logger.LogDebug("Writing {PageType} to {OutputPath}", page.PageType, page.OutputPath);

            // Ensure output directory exists
            var outputDirectory = Path.GetDirectoryName(page.OutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !_fileProvider.DirectoryExists(outputDirectory))
            {
                _fileProvider.CreateDirectory(outputDirectory);
                _logger.LogDebug("Created output directory: {Directory}", outputDirectory);
            }

            // Write the rendered content
            await _fileProvider.WriteAllTextAsync(page.OutputPath, page.RenderedContent, cancellationToken);

            _logger.LogDebug("Successfully wrote {PageType} to {OutputPath} ({ContentLength} characters)", 
                page.PageType, page.OutputPath, page.RenderedContent.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write page to {OutputPath}", page.OutputPath);
            throw;
        }
    }
}
