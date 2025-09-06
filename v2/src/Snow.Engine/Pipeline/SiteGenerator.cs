using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.IO;
using Snow.Engine.ViewModels;

namespace Snow.Engine.Pipeline;

/// <summary>
/// Main static site generator that orchestrates the entire generation process.
/// </summary>
public class SiteGenerator
{
    private readonly IPostParser _postParser;
    private readonly IPageParser _pageParser;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IFileProvider _fileProvider;
    private readonly ISiteSettings _settings;
    private readonly ILogger<SiteGenerator> _logger;
    private readonly IProgress<GeneratorProgress>? _progress;

    public SiteGenerator(
        IPostParser postParser,
        IPageParser pageParser,
        ITemplateRenderer templateRenderer,
        IFileProvider fileProvider,
        ISiteSettings settings,
        ILogger<SiteGenerator> logger,
        IProgress<GeneratorProgress>? progress = null)
    {
        _postParser = postParser ?? throw new ArgumentNullException(nameof(postParser));
        _pageParser = pageParser ?? throw new ArgumentNullException(nameof(pageParser));
        _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _progress = progress;
    }

    /// <summary>
    /// Generates the complete static site.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task GenerateAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting Sandra.Snow v2 static site generation");
            _logger.LogInformation("Blog: {BlogTitle}", _settings.BlogTitle);
            _logger.LogInformation("Theme: {Theme}", _settings.Theme);
            _logger.LogInformation("Output: {OutputDir}", _settings.PostsOutput);

            // Clean output directory
            await CleanOutputDirectoryAsync(cancellationToken);

            // Step 1: Parse all content
            _logger.LogInformation("Step 1: Parsing content...");
            var (posts, pages) = await ParseAllContentAsync(cancellationToken);

            // Step 2: Generate individual post and page files
            _logger.LogInformation("Step 2: Rendering posts and pages...");
            await RenderPostsAndPagesAsync(posts, pages, cancellationToken);

            // Step 3: Process static files (listings, feeds, etc.)
            _logger.LogInformation("Step 3: Processing static files...");
            await ProcessStaticFilesAsync(posts, pages, cancellationToken);

            // Step 4: Copy theme assets
            _logger.LogInformation("Step 4: Copying assets...");
            await CopyAssetsAsync(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation("Static site generation completed successfully in {Elapsed:F2}s", 
                stopwatch.Elapsed.TotalSeconds);
            
            _progress?.Report(new GeneratorProgress(
                TotalFiles: posts.Count + pages.Count,
                ProcessedFiles: posts.Count + pages.Count,
                ErrorCount: 0,
                Elapsed: stopwatch.Elapsed
            ));
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Static site generation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during static site generation");
            throw;
        }
    }

    /// <summary>
    /// Cleans the output directory before generation.
    /// </summary>
    private async Task CleanOutputDirectoryAsync(CancellationToken cancellationToken)
    {
        await SafeDirectoryOperations.CleanDirectoryAsync(
            _fileProvider,
            _settings.PostsOutput,
            _settings.CurrentDirectory,
            _logger,
            cancellationToken);
    }

    /// <summary>
    /// Parses all posts and pages from the content directories.
    /// </summary>
    private async Task<(List<IPost> posts, List<IPage> pages)> ParseAllContentAsync(CancellationToken cancellationToken)
    {
        var posts = new List<IPost>();
        var pages = new List<IPage>();

        // Parse posts
        if (_fileProvider.DirectoryExists(_settings.PostsDirectory))
        {
            var postFiles = _fileProvider.EnumerateFiles(_settings.PostsDirectory, "*", SearchOption.AllDirectories)
                                        .Where(f => f.Extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
                                                   f.Extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase));

            foreach (var file in postFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var post = await _postParser.ParsePostAsync(file, _settings, cancellationToken);
                    if (post != null && post.Published != PublishedStatus.Private)
                    {
                        posts.Add(post);
                        _logger.LogDebug("Parsed post: {Title} from {FileName}", post.Title, file.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse post: {FileName}", file.Name);
                }
            }
        }

        // Parse pages
        if (!string.IsNullOrEmpty(_settings.PagesDirectory) && _fileProvider.DirectoryExists(_settings.PagesDirectory))
        {
            var pageFiles = _fileProvider.EnumerateFiles(_settings.PagesDirectory, "*", SearchOption.AllDirectories)
                                        .Where(f => f.Extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
                                                   f.Extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase));

            foreach (var file in pageFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var page = await _pageParser.ParsePageAsync(file, _settings, cancellationToken);
                    if (page != null && page.Published != PublishedStatus.Private)
                    {
                        pages.Add(page);
                        _logger.LogDebug("Parsed page: {Title} from {FileName}", page.Title, file.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse page: {FileName}", file.Name);
                }
            }
        }

        _logger.LogInformation("Parsed {PostCount} posts and {PageCount} pages", posts.Count, pages.Count);
        return (posts, pages);
    }

    /// <summary>
    /// Renders all posts and pages through their templates.
    /// </summary>
    private async Task RenderPostsAndPagesAsync(List<IPost> posts, List<IPage> pages, CancellationToken cancellationToken)
    {
        var publishedPosts = posts.Where(p => p.Published == PublishedStatus.Published).ToList();
        var draftPosts = posts.Where(p => p.Published == PublishedStatus.Draft).ToList();
        var publishedPages = pages.Where(p => p.Published == PublishedStatus.Published).ToList();

        // Build categories
        var categories = BuildCategories(publishedPosts);

        // Render posts
        foreach (var post in publishedPosts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await RenderPost(post, publishedPosts, publishedPages, draftPosts, categories, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render post: {Title}", post.Title);
            }
        }

        // Render pages
        foreach (var page in publishedPages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await RenderPage(page, publishedPosts, publishedPages, draftPosts, categories, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render page: {Title}", page.Title);
            }
        }
    }

    /// <summary>
    /// Renders a single post.
    /// </summary>
    private async Task RenderPost(
        IPost post, 
        IReadOnlyList<IPost> allPosts, 
        IReadOnlyList<IPage> allPages, 
        IReadOnlyList<IPost> allDrafts, 
        IReadOnlyList<ICategory> allCategories,
        CancellationToken cancellationToken)
    {
        var viewModel = TemplateViewModel.FromPost(post, allPosts, allPages, allDrafts, allCategories, _settings);
        var html = await _templateRenderer.RenderAsync($"_layouts/{post.Layout}", viewModel, null, cancellationToken);

        var outputPath = SafePathOperations.CreateOutputPath(_settings.PostsOutput, post.Url, "index.html");
        var outputDir = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(outputDir) && !_fileProvider.DirectoryExists(outputDir))
        {
            _fileProvider.CreateDirectory(outputDir);
        }

        await _fileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);
        _logger.LogDebug("Rendered post: {Title} -> {OutputPath}", post.Title, outputPath);
    }

    /// <summary>
    /// Renders a single page.
    /// </summary>
    private async Task RenderPage(
        IPage page, 
        IReadOnlyList<IPost> allPosts, 
        IReadOnlyList<IPage> allPages, 
        IReadOnlyList<IPost> allDrafts, 
        IReadOnlyList<ICategory> allCategories,
        CancellationToken cancellationToken)
    {
        var viewModel = TemplateViewModel.FromPage(page, allPosts, allPages, allDrafts, allCategories, _settings);
        var html = await _templateRenderer.RenderAsync($"_layouts/{page.Layout}", viewModel, null, cancellationToken);

        var outputPath = SafePathOperations.CreateOutputPath(_settings.PagesOutput, page.Url, "index.html");
        var outputDir = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(outputDir) && !_fileProvider.DirectoryExists(outputDir))
        {
            _fileProvider.CreateDirectory(outputDir);
        }

        await _fileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);
        _logger.LogDebug("Rendered page: {Title} -> {OutputPath}", page.Title, outputPath);
    }

    /// <summary>
    /// Processes static files like index.cshtml, feed.xml, etc.
    /// </summary>
    private async Task ProcessStaticFilesAsync(List<IPost> posts, List<IPage> pages, CancellationToken cancellationToken)
    {
        var publishedPosts = posts.Where(p => p.Published == PublishedStatus.Published).ToList();
        var categories = BuildCategories(publishedPosts);
        var publishedPages = pages.Where(p => p.Published == PublishedStatus.Published).ToList();
        var draftPosts = posts.Where(p => p.Published == PublishedStatus.Draft).ToList();

        foreach (var staticFile in _settings.ProcessFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ProcessStaticFile(staticFile, publishedPosts, publishedPages, draftPosts, categories, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process static file: {StaticFile}", staticFile.File);
            }
        }
    }

    /// <summary>
    /// Processes a single static file.
    /// </summary>
    private async Task ProcessStaticFile(
        IStaticFile staticFile,
        IReadOnlyList<IPost> posts,
        IReadOnlyList<IPage> pages,
        IReadOnlyList<IPost> drafts,
        IReadOnlyList<ICategory> categories,
        CancellationToken cancellationToken)
    {
        var viewModel = new TemplateViewModel
        {
            Settings = _settings,
            Posts = posts,
            Pages = pages,
            Drafts = drafts,
            AllCategories = categories
        };

        var templateName = Path.GetFileNameWithoutExtension(staticFile.File);
        var html = await _templateRenderer.RenderAsync(templateName, viewModel, null, cancellationToken);

        var outputFileName = staticFile.File;
        if (staticFile.File.EndsWith(".cshtml"))
        {
            outputFileName = staticFile.File.Substring(0, staticFile.File.Length - 7) + ".html";
        }

        var outputPath = Path.Combine(_settings.PostsOutput, outputFileName);
        var outputDir = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(outputDir) && !_fileProvider.DirectoryExists(outputDir))
        {
            _fileProvider.CreateDirectory(outputDir);
        }

        await _fileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);
        _logger.LogDebug("Processed static file: {StaticFile} -> {OutputPath}", staticFile.File, outputPath);
    }

    /// <summary>
    /// Copies theme assets to the output directory.
    /// </summary>
    private async Task CopyAssetsAsync(CancellationToken cancellationToken)
    {
        // Copy directories
        foreach (var directory in _settings.CopyDirectories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await CopyDirectory(directory, cancellationToken);
        }

        // Copy individual files
        foreach (var file in _settings.CopyFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await CopyFile(file, cancellationToken);
        }
    }

    /// <summary>
    /// Copies a directory with optional mapping.
    /// </summary>
    private async Task CopyDirectory(string directorySpec, CancellationToken cancellationToken)
    {
        var sourceDir = directorySpec;
        var destDir = directorySpec;

        // Handle directory mapping (source => destination)
        if (directorySpec.Contains(" => "))
        {
            var parts = directorySpec.Split(new[] { " => " }, StringSplitOptions.RemoveEmptyEntries);
            sourceDir = parts[0].Trim();
            destDir = parts[1].Trim();
        }

        var sourcePath = ResolveAssetPath(sourceDir);
        if (sourcePath == null || !_fileProvider.DirectoryExists(sourcePath))
        {
            _logger.LogWarning("Asset directory not found: {Directory}", sourceDir);
            return;
        }

        var destinationPath = destDir == "." 
            ? _settings.PostsOutput 
            : Path.Combine(_settings.PostsOutput, destDir);

        await CopyDirectoryRecursive(sourcePath, destinationPath, cancellationToken);
        _logger.LogDebug("Copied directory: {Source} -> {Destination}", sourcePath, destinationPath);
    }

    /// <summary>
    /// Copies a single file with optional mapping.
    /// </summary>
    private async Task CopyFile(string fileSpec, CancellationToken cancellationToken)
    {
        var sourcePath = ResolveAssetPath(fileSpec);
        if (sourcePath == null || !_fileProvider.FileExists(sourcePath))
        {
            _logger.LogWarning("Asset file not found: {File}", fileSpec);
            return;
        }

        var destinationPath = Path.Combine(_settings.PostsOutput, fileSpec);
        var destinationDir = Path.GetDirectoryName(destinationPath);

        if (!string.IsNullOrEmpty(destinationDir) && !_fileProvider.DirectoryExists(destinationDir))
        {
            _fileProvider.CreateDirectory(destinationDir);
        }

        _fileProvider.CopyFile(sourcePath, destinationPath, overwrite: true);
        _logger.LogDebug("Copied file: {Source} -> {Destination}", sourcePath, destinationPath);
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Resolves asset path with theme fallback.
    /// </summary>
    private string? ResolveAssetPath(string assetPath)
    {
        var searchPaths = new[]
        {
            Path.Combine(_settings.CurrentDirectory, _settings.ThemesDirectory, _settings.Theme, assetPath),
            Path.Combine(_settings.CurrentDirectory, _settings.ThemesDirectory, "default", assetPath),
            Path.Combine(_settings.CurrentDirectory, assetPath)
        };

        return searchPaths.FirstOrDefault(path => 
            _fileProvider.FileExists(path) || _fileProvider.DirectoryExists(path));
    }

    /// <summary>
    /// Copies a directory recursively.
    /// </summary>
    private async Task CopyDirectoryRecursive(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        if (!_fileProvider.DirectoryExists(destinationPath))
        {
            _fileProvider.CreateDirectory(destinationPath);
        }

        var files = _fileProvider.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(sourcePath, file.FullPath);
            var destFilePath = Path.Combine(destinationPath, relativePath);
            var destFileDir = Path.GetDirectoryName(destFilePath);

            if (!string.IsNullOrEmpty(destFileDir) && !_fileProvider.DirectoryExists(destFileDir))
            {
                _fileProvider.CreateDirectory(destFileDir);
            }

            _fileProvider.CopyFile(file.FullPath, destFilePath, overwrite: true);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Builds category collections from posts.
    /// </summary>
    private List<ICategory> BuildCategories(IEnumerable<IPost> posts)
    {
        return posts
            .SelectMany(p => p.Categories.Select(cat => new { Category = cat, Post = p }))
            .GroupBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var category = new Models.Category(g.Key);
                category.AddPosts(g.Select(x => x.Post));
                category.SortPosts();
                return (ICategory)category;
            })
            .ToList();
    }
}
