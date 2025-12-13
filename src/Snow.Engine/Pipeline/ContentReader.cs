using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;

namespace Snow.Engine.Pipeline;

/// <summary>
/// Reads content files from the file system for pipeline processing.
/// </summary>
public class ContentReader : IContentReader
{
    private readonly IFileProvider _fileProvider;
    private readonly ISiteSettings _settings;
    private readonly ILogger<ContentReader> _logger;

    public ContentReader(IFileProvider fileProvider, ISiteSettings settings, ILogger<ContentReader> logger)
    {
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<PipelinePage> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting content discovery from {PostsDir} and {PagesDir}", 
            _settings.PostsDirectory, _settings.PagesDirectory);

        var totalFiles = 0;

        // Read posts
        await foreach (var post in ReadPostsAsync(cancellationToken))
        {
            totalFiles++;
            yield return post;
        }

        // Read pages
        await foreach (var page in ReadPagesAsync(cancellationToken))
        {
            totalFiles++;
            yield return page;
        }

        // Read static files for processing
        await foreach (var staticFile in ReadStaticFilesAsync(cancellationToken))
        {
            totalFiles++;
            yield return staticFile;
        }

        _logger.LogInformation("Content discovery completed. Found {TotalFiles} files", totalFiles);
    }

    /// <summary>
    /// Reads all posts from the posts directory.
    /// </summary>
    private async IAsyncEnumerable<PipelinePage> ReadPostsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_fileProvider.DirectoryExists(_settings.PostsDirectory))
        {
            _logger.LogWarning("Posts directory not found: {PostsDirectory}", _settings.PostsDirectory);
            yield break;
        }

        var extensions = new[] { ".md", ".markdown" };
        var postFiles = _fileProvider.EnumerateFiles(_settings.PostsDirectory, "*", SearchOption.AllDirectories)
                                   .Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

        foreach (var file in postFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogDebug("Discovered post file: {FilePath}", file.FullPath);

            yield return new PipelinePage(
                SourcePath: file.FullPath,
                PageType: PipelinePageType.Post
            );

            // Add small delay for cooperative cancellation in tight loops
            if (cancellationToken.IsCancellationRequested)
                yield break;

            await Task.Yield(); // Yield control for cooperative multitasking
        }
    }

    /// <summary>
    /// Reads all pages from the pages directory.
    /// </summary>
    private async IAsyncEnumerable<PipelinePage> ReadPagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings.PagesDirectory) || !_fileProvider.DirectoryExists(_settings.PagesDirectory))
        {
            _logger.LogDebug("Pages directory not found or not configured: {PagesDirectory}", _settings.PagesDirectory);
            yield break;
        }

        var extensions = new[] { ".md", ".markdown" };
        var pageFiles = _fileProvider.EnumerateFiles(_settings.PagesDirectory, "*", SearchOption.AllDirectories)
                                   .Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

        foreach (var file in pageFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogDebug("Discovered page file: {FilePath}", file.FullPath);

            yield return new PipelinePage(
                SourcePath: file.FullPath,
                PageType: PipelinePageType.Page
            );

            if (cancellationToken.IsCancellationRequested)
                yield break;

            await Task.Yield();
        }
    }

    /// <summary>
    /// Reads static files that need processing (like index.cshtml, feed.xml, etc.).
    /// </summary>
    private async IAsyncEnumerable<PipelinePage> ReadStaticFilesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var staticFile in _settings.ProcessFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var templatePath = ResolveTemplatePath(staticFile.File);
            if (templatePath != null && _fileProvider.FileExists(templatePath))
            {
                _logger.LogDebug("Discovered static file template: {FilePath} -> {TemplatePath}", 
                    staticFile.File, templatePath);

                yield return new PipelinePage(
                    SourcePath: templatePath,
                    PageType: PipelinePageType.StaticFile
                );
            }
            else
            {
                _logger.LogWarning("Static file template not found: {StaticFile}", staticFile.File);
                
                yield return new PipelinePage(
                    SourcePath: staticFile.File,
                    PageType: PipelinePageType.StaticFile
                ).WithError("Discovery", new FileNotFoundException($"Static file template not found: {staticFile.File}"));
            }

            await Task.Yield();
        }
    }

    /// <summary>
    /// Resolves the full path for a template file.
    /// </summary>
    private string? ResolveTemplatePath(string templateFile)
    {
        var searchPaths = new[]
        {
            Path.Combine(_settings.CurrentDirectory, _settings.ThemesDirectory, _settings.Theme, templateFile),
            Path.Combine(_settings.CurrentDirectory, _settings.ThemesDirectory, "default", templateFile),
            Path.Combine(_settings.CurrentDirectory, templateFile)
        };

        return searchPaths.FirstOrDefault(path => _fileProvider.FileExists(path));
    }
}
