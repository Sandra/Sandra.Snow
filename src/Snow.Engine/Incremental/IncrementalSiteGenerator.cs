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
using Snow.Engine.Pipeline;
using Snow.Engine.ViewModels;

namespace Snow.Engine.Incremental;

/// <summary>
/// Site generator with incremental build capabilities.
/// </summary>
public class IncrementalSiteGenerator
{
    private readonly SiteGenerator _fullGenerator;
    private readonly DependencyGraph _dependencyGraph;
    private readonly IPostParser _postParser;
    private readonly IPageParser _pageParser;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IFileProvider _fileProvider;
    private readonly ISiteSettings _settings;
    private readonly ILogger<IncrementalSiteGenerator> _logger;
    private readonly IProgress<GeneratorProgress>? _progress;

    public IncrementalSiteGenerator(
        SiteGenerator fullGenerator,
        DependencyGraph dependencyGraph,
        IPostParser postParser,
        IPageParser pageParser,
        ITemplateRenderer templateRenderer,
        IFileProvider fileProvider,
        ISiteSettings settings,
        ILogger<IncrementalSiteGenerator> logger,
        IProgress<GeneratorProgress>? progress = null)
    {
        _fullGenerator = fullGenerator ?? throw new ArgumentNullException(nameof(fullGenerator));
        _dependencyGraph = dependencyGraph ?? throw new ArgumentNullException(nameof(dependencyGraph));
        _postParser = postParser ?? throw new ArgumentNullException(nameof(postParser));
        _pageParser = pageParser ?? throw new ArgumentNullException(nameof(pageParser));
        _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _progress = progress;
    }

    /// <summary>
    /// Performs a full build and initializes the dependency graph.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task FullBuildAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing full build and building dependency graph...");
        var stopwatch = Stopwatch.StartNew();

        // Clear existing graph
        _dependencyGraph.Clear();

        // Build dependency graph during full generation
        await BuildDependencyGraphAsync(cancellationToken);

        // Perform full generation
        await _fullGenerator.GenerateAsync(cancellationToken);

        // Save cache for future incremental builds
        var cacheFile = GetCacheFilePath();
        await _dependencyGraph.SaveCacheAsync(cacheFile);

        stopwatch.Stop();
        _logger.LogInformation("Full build completed in {Elapsed:F2}s with dependency graph", 
            stopwatch.Elapsed.TotalSeconds);
    }

    /// <summary>
    /// Performs an incremental build for the specified changed files.
    /// </summary>
    /// <param name="changedFiles">Files that have changed.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if any files were rebuilt, false if no changes detected.</returns>
    public async Task<bool> IncrementalBuildAsync(IEnumerable<string> changedFiles, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var changedList = changedFiles.ToList();

        _logger.LogDebug("Starting incremental build for {FileCount} changed files", changedList.Count);

        // Filter to files that actually changed (content hash comparison)
        var actuallyChanged = new List<string>();
        foreach (var file in changedList)
        {
            if (await _dependencyGraph.HasFileChangedAsync(file, _fileProvider))
            {
                actuallyChanged.Add(file);
            }
        }

        if (!actuallyChanged.Any())
        {
            _logger.LogDebug("No actual changes detected, skipping incremental build");
            return false;
        }

        _logger.LogInformation("Incremental build: {ChangedCount} files changed", actuallyChanged.Count);

        // Get nodes that need rebuilding
        var nodesToRebuild = _dependencyGraph.GetNodesToRebuild(actuallyChanged);
        
        if (!nodesToRebuild.Any())
        {
            _logger.LogDebug("No dependent nodes need rebuilding");
            return false;
        }

        _logger.LogInformation("Rebuilding {RebuildCount} affected files", nodesToRebuild.Count);

        // Rebuild affected nodes
        var rebuilt = 0;
        var errors = 0;

        foreach (var node in nodesToRebuild)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await RebuildNode(node, cancellationToken);
                rebuilt++;

                _progress?.Report(new GeneratorProgress(
                    TotalFiles: nodesToRebuild.Count,
                    ProcessedFiles: rebuilt,
                    ErrorCount: errors,
                    CurrentFile: Path.GetFileName(node.Path),
                    CurrentStage: "Incremental",
                    Elapsed: stopwatch.Elapsed
                ));
            }
            catch (Exception ex)
            {
                errors++;
                _logger.LogError(ex, "Failed to rebuild {NodePath}", node.Path);
            }
        }

        stopwatch.Stop();

        _logger.LogInformation("Incremental build completed in {Elapsed:F2}s: {Rebuilt} rebuilt, {Errors} errors",
            stopwatch.Elapsed.TotalSeconds, rebuilt, errors);

        // Save updated cache
        var cacheFile = GetCacheFilePath();
        await _dependencyGraph.SaveCacheAsync(cacheFile);

        return rebuilt > 0;
    }

    /// <summary>
    /// Initializes from existing cache if available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task InitializeFromCacheAsync(CancellationToken cancellationToken = default)
    {
        var cacheFile = GetCacheFilePath();
        await _dependencyGraph.LoadCacheAsync(cacheFile);
    }

    /// <summary>
    /// Builds the dependency graph by analyzing all content and templates.
    /// </summary>
    private async Task BuildDependencyGraphAsync(CancellationToken cancellationToken)
    {
        // Add content files
        await AddContentFilesToGraph(cancellationToken);

        // Add template files
        await AddTemplateFilesToGraph(cancellationToken);

        // Add configuration files
        AddConfigurationToGraph();

        _logger.LogDebug("Dependency graph built: {NodeCount} nodes", _dependencyGraph.GetAllNodes().Count);
    }

    /// <summary>
    /// Adds all content files (posts, pages) to the dependency graph.
    /// </summary>
    private async Task AddContentFilesToGraph(CancellationToken cancellationToken)
    {
        // Add posts
        if (_fileProvider.DirectoryExists(_settings.PostsDirectory))
        {
            var postFiles = _fileProvider.EnumerateFiles(_settings.PostsDirectory, "*.md", SearchOption.AllDirectories);
            foreach (var file in postFiles)
            {
                var content = await _fileProvider.ReadAllTextAsync(file.FullPath, cancellationToken);
                var hash = ComputeHash(content);
                
                var node = new BuildNode(file.FullPath, BuildNodeType.SourceContent, hash, file.LastWriteTime.ToUniversalTime());
                _dependencyGraph.AddNode(node);

                // Content depends on layout templates
                var post = await _postParser.ParsePostAsync(file, _settings, cancellationToken);
                if (post != null)
                {
                    var layoutPath = ResolveTemplatePath($"_layouts/{post.Layout}.cshtml");
                    if (layoutPath != null)
                    {
                        _dependencyGraph.AddDependency(file.FullPath, layoutPath);
                    }
                }
            }
        }

        // Add pages
        if (!string.IsNullOrEmpty(_settings.PagesDirectory) && _fileProvider.DirectoryExists(_settings.PagesDirectory))
        {
            var pageFiles = _fileProvider.EnumerateFiles(_settings.PagesDirectory, "*.md", SearchOption.AllDirectories);
            foreach (var file in pageFiles)
            {
                var content = await _fileProvider.ReadAllTextAsync(file.FullPath, cancellationToken);
                var hash = ComputeHash(content);
                
                var node = new BuildNode(file.FullPath, BuildNodeType.SourceContent, hash, file.LastWriteTime.ToUniversalTime());
                _dependencyGraph.AddNode(node);

                // Content depends on layout templates
                var page = await _pageParser.ParsePageAsync(file, _settings, cancellationToken);
                if (page != null)
                {
                    var layoutPath = ResolveTemplatePath($"_layouts/{page.Layout}.cshtml");
                    if (layoutPath != null)
                    {
                        _dependencyGraph.AddDependency(file.FullPath, layoutPath);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds template files to the dependency graph.
    /// </summary>
    private async Task AddTemplateFilesToGraph(CancellationToken cancellationToken)
    {
        var themePath = Path.Combine(_settings.CurrentDirectory, _settings.ThemesDirectory, _settings.Theme);
        
        if (_fileProvider.DirectoryExists(themePath))
        {
            var templateFiles = _fileProvider.EnumerateFiles(themePath, "*.cshtml", SearchOption.AllDirectories);
            foreach (var file in templateFiles)
            {
                var content = await _fileProvider.ReadAllTextAsync(file.FullPath, cancellationToken);
                var hash = ComputeHash(content);
                
                var node = new BuildNode(file.FullPath, BuildNodeType.Template, hash, file.LastWriteTime.ToUniversalTime());
                _dependencyGraph.AddNode(node);
            }
        }
    }

    /// <summary>
    /// Adds configuration files to the dependency graph.
    /// </summary>
    private void AddConfigurationToGraph()
    {
        var configPaths = new[]
        {
            Path.Combine(_settings.CurrentDirectory, "sandra.json"),
            Path.Combine(_settings.CurrentDirectory, "sandra.config.json"),
            Path.Combine(_settings.CurrentDirectory, "snow.config.json"),
            Path.Combine(_settings.CurrentDirectory, "snow.config")
        };

        foreach (var configPath in configPaths)
        {
            if (_fileProvider.FileExists(configPath))
            {
                var fileInfo = _fileProvider.GetFileInfo(configPath);
                var node = new BuildNode(
                    configPath, 
                    BuildNodeType.Configuration, 
                    "", // Config changes always trigger full rebuild
                    fileInfo?.LastWriteTime.ToUniversalTime() ?? DateTime.UtcNow);
                
                _dependencyGraph.AddNode(node);
                break; // Only track one config file
            }
        }
    }

    /// <summary>
    /// Rebuilds a specific node based on its type.
    /// </summary>
    private async Task RebuildNode(BuildNode node, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Rebuilding node: {Path} ({Type})", node.Path, node.Type);

        switch (node.Type)
        {
            case BuildNodeType.SourceContent:
                await RebuildContentNode(node, cancellationToken);
                break;

            case BuildNodeType.Template:
                // Template changes affect all content that uses them
                _logger.LogInformation("Template changed, triggering full rebuild: {Path}", node.Path);
                await _fullGenerator.GenerateAsync(cancellationToken);
                break;

            case BuildNodeType.Configuration:
                // Configuration changes always trigger full rebuild
                _logger.LogInformation("Configuration changed, triggering full rebuild");
                await _fullGenerator.GenerateAsync(cancellationToken);
                break;

            default:
                _logger.LogWarning("Unknown node type for rebuild: {Type}", node.Type);
                break;
        }
    }

    /// <summary>
    /// Rebuilds a single content node (post or page).
    /// </summary>
    private async Task RebuildContentNode(BuildNode node, CancellationToken cancellationToken)
    {
        var fileInfo = _fileProvider.GetFileInfo(node.Path);
        if (fileInfo == null || !fileInfo.Exists)
        {
            _logger.LogWarning("Content file not found for rebuild: {Path}", node.Path);
            return;
        }

        // Parse content
        if (node.Path.Contains("_posts"))
        {
            var post = await _postParser.ParsePostAsync(fileInfo, _settings, cancellationToken);
            if (post != null && post.Published != PublishedStatus.Private)
            {
                await RenderSinglePost(post, cancellationToken);
            }
        }
        else if (node.Path.Contains("_pages"))
        {
            var page = await _pageParser.ParsePageAsync(fileInfo, _settings, cancellationToken);
            if (page != null && page.Published != PublishedStatus.Private)
            {
                await RenderSinglePage(page, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Renders a single post without affecting other content.
    /// </summary>
    private async Task RenderSinglePost(IPost post, CancellationToken cancellationToken)
    {
        // For incremental builds, we need all posts for site-wide data
        // This is a simplified implementation - in a full implementation,
        // we'd cache the site collections to avoid re-parsing everything
        var allPosts = await GetAllPosts(cancellationToken);
        var allPages = await GetAllPages(cancellationToken);
        var allCategories = BuildCategories(allPosts);

        var viewModel = TemplateViewModel.FromPost(post, allPosts, allPages, new List<IPost>(), allCategories, _settings);
        var html = await _templateRenderer.RenderAsync($"_layouts/{post.Layout}", viewModel, null, cancellationToken);

        var outputPath = SafePathOperations.CreateOutputPath(_settings.PostsOutput, post.Url, "index.html");
        var outputDir = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(outputDir) && !_fileProvider.DirectoryExists(outputDir))
        {
            _fileProvider.CreateDirectory(outputDir);
        }

        await _fileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);
        _logger.LogDebug("Incrementally rebuilt post: {Title} -> {OutputPath}", post.Title, outputPath);
    }

    /// <summary>
    /// Renders a single page without affecting other content.
    /// </summary>
    private async Task RenderSinglePage(IPage page, CancellationToken cancellationToken)
    {
        var allPosts = await GetAllPosts(cancellationToken);
        var allPages = await GetAllPages(cancellationToken);
        var allCategories = BuildCategories(allPosts);

        var viewModel = TemplateViewModel.FromPage(page, allPosts, allPages, new List<IPost>(), allCategories, _settings);
        var html = await _templateRenderer.RenderAsync($"_layouts/{page.Layout}", viewModel, null, cancellationToken);

        var outputPath = SafePathOperations.CreateOutputPath(_settings.PagesOutput, page.Url, "index.html");
        var outputDir = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(outputDir) && !_fileProvider.DirectoryExists(outputDir))
        {
            _fileProvider.CreateDirectory(outputDir);
        }

        await _fileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);
        _logger.LogDebug("Incrementally rebuilt page: {Title} -> {OutputPath}", page.Title, outputPath);
    }

    /// <summary>
    /// Gets all posts (cached implementation would be better for performance).
    /// </summary>
    private async Task<IReadOnlyList<IPost>> GetAllPosts(CancellationToken cancellationToken)
    {
        // Simplified implementation - in production, this would be cached
        var posts = new List<IPost>();

        if (_fileProvider.DirectoryExists(_settings.PostsDirectory))
        {
            var postFiles = _fileProvider.EnumerateFiles(_settings.PostsDirectory, "*.md", SearchOption.AllDirectories);
            foreach (var file in postFiles)
            {
                var post = await _postParser.ParsePostAsync(file, _settings, cancellationToken);
                if (post != null && post.Published == PublishedStatus.Published)
                {
                    posts.Add(post);
                }
            }
        }

        return posts.OrderByDescending(p => p.Date).ToList();
    }

    /// <summary>
    /// Gets all pages (cached implementation would be better for performance).
    /// </summary>
    private async Task<IReadOnlyList<IPage>> GetAllPages(CancellationToken cancellationToken)
    {
        var pages = new List<IPage>();

        if (!string.IsNullOrEmpty(_settings.PagesDirectory) && _fileProvider.DirectoryExists(_settings.PagesDirectory))
        {
            var pageFiles = _fileProvider.EnumerateFiles(_settings.PagesDirectory, "*.md", SearchOption.AllDirectories);
            foreach (var file in pageFiles)
            {
                var page = await _pageParser.ParsePageAsync(file, _settings, cancellationToken);
                if (page != null && page.Published == PublishedStatus.Published)
                {
                    pages.Add(page);
                }
            }
        }

        return pages.OrderByDescending(p => p.Date).ToList();
    }

    /// <summary>
    /// Builds categories from posts.
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

    /// <summary>
    /// Resolves a template path with theme fallback.
    /// </summary>
    private string? ResolveTemplatePath(string templateName)
    {
        var searchPaths = new[]
        {
            Path.Combine(_settings.CurrentDirectory, _settings.ThemesDirectory, _settings.Theme, templateName),
            Path.Combine(_settings.CurrentDirectory, _settings.ThemesDirectory, "default", templateName)
        };

        return searchPaths.FirstOrDefault(path => _fileProvider.FileExists(path));
    }

    /// <summary>
    /// Gets the cache file path for the dependency graph.
    /// </summary>
    private string GetCacheFilePath()
    {
        var cacheDir = Path.Combine(_settings.CurrentDirectory, ".sandra");
        return Path.Combine(cacheDir, "dependency-cache.json");
    }

    /// <summary>
    /// Computes a content hash for change detection.
    /// </summary>
    private static string ComputeHash(string content)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }
}
