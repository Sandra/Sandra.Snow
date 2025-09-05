using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.ViewModels;

namespace Snow.Engine.Templates;

/// <summary>
/// Simple template renderer that processes basic Razor-like syntax.
/// This is a transitional implementation for Stage 3 that will be enhanced or replaced with RazorLight/MVC in later iterations.
/// </summary>
public class SimpleTemplateRenderer : ITemplateRenderer
{
    private readonly IFileProvider _fileProvider;
    private readonly ISiteSettings _settings;
    private readonly ILogger<SimpleTemplateRenderer> _logger;
    private readonly ConcurrentDictionary<string, (string Content, DateTime LastWriteUtc)> _templateCache = new();
    private readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new();

    // Basic template processing patterns
    private static readonly Regex ModelPropertyRegex = new(@"@Model\.(\w+)", RegexOptions.Compiled);
    private static readonly Regex RawContentRegex = new(@"@Html\.Raw\(([^)]*)\)", RegexOptions.Compiled);
    private static readonly Regex LayoutRegex = new(@"@\{\s*Layout\s*=\s*""([^""]+)""\s*;\s*\}", RegexOptions.Compiled);

    public SimpleTemplateRenderer(IFileProvider fileProvider, ISiteSettings settings, ILogger<SimpleTemplateRenderer> logger)
    {
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<string> RenderAsync<TModel>(
        string templatePath, 
        TModel model, 
        IDictionary<string, object>? viewData = null,
        CancellationToken cancellationToken = default)
    {
        // Check for cancellation before starting work
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.FromResult(Render(templatePath, model, viewData));
    }

    /// <inheritdoc />
    public string Render<TModel>(string templatePath, TModel model, IDictionary<string, object>? viewData = null)
    {
        if (string.IsNullOrWhiteSpace(templatePath))
            throw new ArgumentException("Template path cannot be null or whitespace", nameof(templatePath));

        _logger.LogDebug("Rendering template: {TemplatePath} with model type: {ModelType}", 
            templatePath, typeof(TModel).Name);

        try
        {
            var templateContent = LoadTemplate(templatePath);
            var processedContent = ProcessTemplate(templateContent, model, viewData);

            _logger.LogDebug("Successfully rendered template: {TemplatePath} ({CharacterCount} characters)", 
                templatePath, processedContent.Length);

            return processedContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template: {TemplatePath}", templatePath);
            
            // Return a safe error template
            return $@"<html><body><h1>Template Error</h1><p>Failed to render template: {templatePath}</p><p>Error: {ex.Message}</p></body></html>";
        }
    }

    /// <inheritdoc />
    public bool TemplateExists(string templatePath)
    {
        if (string.IsNullOrWhiteSpace(templatePath))
            return false;

        var resolvedPath = ResolveTemplatePath(templatePath);
        return resolvedPath != null && _fileProvider.FileExists(resolvedPath);
    }

    /// <summary>
    /// Loads template content from file system with caching and invalidation.
    /// </summary>
    /// <param name="templatePath">The template path.</param>
    /// <returns>The template content.</returns>
    private string LoadTemplate(string templatePath)
    {
        var normalizedPath = NormalizeTemplatePath(templatePath);

        var resolvedPath = ResolveTemplatePath(templatePath);
        if (resolvedPath == null || !_fileProvider.FileExists(resolvedPath))
        {
            throw new FileNotFoundException($"Template not found: {templatePath}");
        }

        // Get file info for cache invalidation
        var fileInfo = _fileProvider.GetFileInfo(resolvedPath);
        if (fileInfo == null)
        {
            throw new FileNotFoundException($"Template file info not found: {resolvedPath}");
        }

        // Check cache with invalidation using UTC time
        if (_templateCache.TryGetValue(normalizedPath, out var cached) && 
            cached.LastWriteUtc >= fileInfo.LastWriteTime.ToUniversalTime())
        {
            _logger.LogDebug("Using cached template: {TemplatePath}", normalizedPath);
            return cached.Content;
        }

        // Load fresh content using file provider
        var content = _fileProvider.ReadAllTextAsync(resolvedPath).GetAwaiter().GetResult();
        _templateCache[normalizedPath] = (content, fileInfo.LastWriteTime.ToUniversalTime());

        _logger.LogDebug("Loaded and cached template: {TemplatePath} from {ResolvedPath}", 
            normalizedPath, resolvedPath);

        return content;
    }

    /// <summary>
    /// Processes template content with basic model binding.
    /// </summary>
    private string ProcessTemplate<TModel>(string templateContent, TModel model, IDictionary<string, object>? viewData)
    {
        // This is a simplified template processor for Stage 3
        // In a production implementation, this would use proper Razor compilation
        
        var processed = templateContent;

        // Handle layout directive
        var layoutMatch = LayoutRegex.Match(processed);
        if (layoutMatch.Success)
        {
            var layoutName = layoutMatch.Groups[1].Value;
            processed = LayoutRegex.Replace(processed, ""); // Remove layout directive
            
            // Load and process layout
            var layoutContent = LoadTemplate($"_layouts/{layoutName}");
            processed = layoutContent.Replace("@RenderBody()", processed);
        }

        // Replace @Model.Property references with cached reflection
        processed = ModelPropertyRegex.Replace(processed, match =>
        {
            var propertyName = match.Groups[1].Value;
            try
            {
                var properties = GetCachedProperties(typeof(TModel));
                if (properties.TryGetValue(propertyName, out var property))
                {
                    var value = property.GetValue(model);
                    return System.Net.WebUtility.HtmlEncode(value?.ToString() ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve model property: {PropertyName}", propertyName);
            }
            
            return $"{{@Model.{propertyName}}}"; // Leave unresolved properties as-is
        });

        // Handle @Html.Raw() calls with cached reflection
        processed = RawContentRegex.Replace(processed, match =>
        {
            var content = match.Groups[1].Value.Trim('"');
            
            // Try to resolve from model properties
            if (content.StartsWith("Model."))
            {
                var propertyName = content.Substring(6);
                try
                {
                    var properties = GetCachedProperties(typeof(TModel));
                    if (properties.TryGetValue(propertyName, out var property))
                    {
                        var value = property.GetValue(model);
                        return value?.ToString() ?? string.Empty; // Raw, not encoded
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to resolve raw content property: {PropertyName}", propertyName);
                }
            }

            return content;
        });

        return processed;
    }

    /// <summary>
    /// Resolves template path with theme fallback.
    /// </summary>
    private string? ResolveTemplatePath(string templatePath)
    {
        var normalizedPath = NormalizeTemplatePath(templatePath);
        
        var searchPaths = new[]
        {
            // Current theme
            Path.Combine(_settings.CurrentDirectory, _settings.ThemesDirectory, _settings.Theme, normalizedPath),
            // Default theme fallback
            Path.Combine(_settings.CurrentDirectory, _settings.ThemesDirectory, "default", normalizedPath),
        };

        foreach (var searchPath in searchPaths)
        {
            var fullPath = Path.GetFullPath(searchPath);
            if (_fileProvider.FileExists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets cached property info for a type to avoid repeated reflection.
    /// </summary>
    private Dictionary<string, PropertyInfo> GetCachedProperties(Type type)
    {
        return _propertyCache.GetOrAdd(type, t =>
        {
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    /// Normalizes template path for consistent processing.
    /// </summary>
    private static string NormalizeTemplatePath(string templatePath)
    {
        var normalized = templatePath.TrimStart('/', '\\').Replace('\\', '/');
        
        if (!normalized.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".cshtml";
        }

        return normalized;
    }
}
