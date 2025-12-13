using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazorEngineCore;
using Snow.Abstractions;

namespace Snow.Engine.Templates.Razor;

/// <summary>
/// Template renderer using RazorEngineCore for full Razor syntax support.
/// Provides backwards compatibility with v1 Nancy Razor templates.
/// Supports single-file publish deployment.
/// </summary>
public class RazorTemplateRenderer : ITemplateRenderer
{
    private readonly ISiteSettings _settings;
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<RazorTemplateRenderer> _logger;
    private readonly IRazorEngine _engine;
    private readonly ConcurrentDictionary<string, object> _templateCache = new();

    public RazorTemplateRenderer(
        ISiteSettings settings, 
        IFileProvider fileProvider, 
        ILogger<RazorTemplateRenderer> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _engine = new RazorEngine();
    }

    /// <inheritdoc />
    public async Task<string> RenderAsync<TModel>(
        string templatePath, 
        TModel model, 
        IDictionary<string, object>? viewData = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            _logger.LogDebug("Rendering template: {TemplatePath} with model type: {ModelType}", 
                templatePath, typeof(TModel).Name);

            var normalizedPath = NormalizeTemplatePath(templatePath);
            var resolvedPath = ResolveFullPath(normalizedPath);
            
            if (resolvedPath == null)
            {
                _logger.LogError("Template not found: {TemplatePath}", templatePath);
                return GenerateErrorHtml(templatePath, "Template not found");
            }

            var templateContent = await _fileProvider.ReadAllTextAsync(resolvedPath);
            var result = await RenderWithLayoutAsync(templateContent, resolvedPath, model, cancellationToken);
            
            _logger.LogDebug("Successfully rendered template: {TemplatePath} ({CharacterCount} characters)", 
                templatePath, result.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template: {TemplatePath}", templatePath);
            return GenerateErrorHtml(templatePath, ex.Message, ex.ToString());
        }
    }

    /// <summary>
    /// Renders a template with layout support.
    /// </summary>
    private async Task<string> RenderWithLayoutAsync<TModel>(
        string templateContent, 
        string templatePath,
        TModel model, 
        CancellationToken cancellationToken)
    {
        // Compile and run the template
        var (content, layout) = await CompileAndRunAsync(templateContent, templatePath, model, cancellationToken);
        
        // If there's a layout, render it with the content as the body
        if (!string.IsNullOrEmpty(layout))
        {
            var layoutPath = ResolveLayoutPath(layout, templatePath);
            if (layoutPath != null)
            {
                var layoutContent = await _fileProvider.ReadAllTextAsync(layoutPath);
                return await RenderLayoutAsync(layoutContent, layoutPath, model, content, cancellationToken);
            }
            else
            {
                _logger.LogWarning("Layout not found: {Layout} for template: {TemplatePath}", layout, templatePath);
            }
        }
        
        return content;
    }

    /// <summary>
    /// Renders a layout template with the body content.
    /// Note: Nested layouts are not currently supported.
    /// </summary>
    private async Task<string> RenderLayoutAsync<TModel>(
        string layoutContent,
        string layoutPath,
        TModel model,
        string bodyContent,
        CancellationToken cancellationToken)
    {
        var compiled = await GetOrCompileTemplateAsync<TModel>(layoutContent, layoutPath, cancellationToken);
        
        var result = await compiled.RunAsync(instance =>
        {
            instance.Model = model;
            instance.BodyContent = bodyContent;
            SetupTemplateCallbacks(instance, layoutPath, cancellationToken);
        });
        
        return result;
    }

    /// <summary>
    /// Compiles and runs a template, returning the content and the layout name if specified.
    /// Captures the Layout property from the template instance after execution.
    /// </summary>
    private async Task<(string content, string? layout)> CompileAndRunAsync<TModel>(
        string templateContent,
        string templatePath,
        TModel model,
        CancellationToken cancellationToken)
    {
        var compiled = await GetOrCompileTemplateAsync<TModel>(templateContent, templatePath, cancellationToken);
        
        // We need to capture the Layout property AFTER template execution
        // The callback in RunAsync runs BEFORE template execution, so we need a different approach
        // Create the instance, run it, then read the Layout property
        
        SnowTemplateBase<TModel>? templateInstance = null;
        
        var result = await compiled.RunAsync(instance =>
        {
            instance.Model = model;
            SetupTemplateCallbacks(instance, templatePath, cancellationToken);
            templateInstance = instance;
        });
        
        // Layout is set during template execution, so read it after RunAsync completes
        var detectedLayout = templateInstance?.Layout;
        
        return (result, detectedLayout);
    }

    /// <summary>
    /// Gets or compiles a template with caching.
    /// Templates are cached by path and model type to avoid recompilation.
    /// </summary>
    private async Task<IRazorEngineCompiledTemplate<SnowTemplateBase<TModel>>> GetOrCompileTemplateAsync<TModel>(
        string templateContent,
        string templatePath,
        CancellationToken cancellationToken)
    {
        // Key by full path + model type to avoid generic type collisions
        var cacheKey = $"{templatePath}::{typeof(TModel).FullName}";

        if (_templateCache.TryGetValue(cacheKey, out var cachedObj))
        {
            _logger.LogDebug("Using cached template: {TemplatePath}", templatePath);
            return (IRazorEngineCompiledTemplate<SnowTemplateBase<TModel>>)cachedObj;
        }

        _logger.LogDebug("Compiling template: {TemplatePath}", templatePath);
        var compiled = await CompileTemplateAsync<TModel>(templateContent, templatePath, cancellationToken);
        _templateCache[cacheKey] = compiled;
        return compiled;
    }

    /// <summary>
    /// Preprocesses a template to remove @inherits directive (handled by our base class).
    /// </summary>
    private static string PreprocessTemplate(string templateContent)
    {
        // Remove @inherits directives - we provide our own base class via Compile<T>
        // Pattern matches: @inherits Nancy.ViewEngines.Razor.NancyRazorViewBase<SomeType>
        var processed = System.Text.RegularExpressions.Regex.Replace(
            templateContent,
            @"@inherits\s+[^\r\n]+[\r\n]*",
            "",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        
        return processed;
    }

    /// <summary>
    /// Compiles a template.
    /// </summary>
    private async Task<IRazorEngineCompiledTemplate<SnowTemplateBase<TModel>>> CompileTemplateAsync<TModel>(
        string templateContent,
        string templatePath,
        CancellationToken cancellationToken)
    {
        try
        {
            // Preprocess to remove @inherits directive
            var processedContent = PreprocessTemplate(templateContent);
            
            var compiled = await _engine.CompileAsync<SnowTemplateBase<TModel>>(
                processedContent,
                builder =>
                {
                    // Add required namespaces
                    builder.AddUsing("System");
                    builder.AddUsing("System.Collections.Generic");
                    builder.AddUsing("System.Linq");
                    builder.AddUsing("System.Threading.Tasks");
                    builder.AddUsing("Snow.Abstractions");
                    builder.AddUsing("Snow.Engine.ViewModels");
                    builder.AddUsing("Snow.Engine.Templates.Razor");
                    builder.AddUsing("Snow.ViewModels");
                    
                    // Add assembly references for the types used in templates
                    builder.AddAssemblyReference(typeof(Snow.Abstractions.IPost).Assembly);
                    builder.AddAssemblyReference(typeof(Snow.Engine.ViewModels.BasePostViewModel).Assembly);
                    builder.AddAssemblyReference(typeof(System.Linq.Enumerable).Assembly);
                    builder.AddAssemblyReference(typeof(System.Collections.Generic.List<>).Assembly);
                    
                    // Add the model type's assembly if it's different
                    var modelAssembly = typeof(TModel).Assembly;
                    builder.AddAssemblyReference(modelAssembly);
                    
                    // Set a safe template filename for compilation (must be a valid assembly name)
                    var safeFileName = Path.GetFileNameWithoutExtension(templatePath);
                    safeFileName = System.Text.RegularExpressions.Regex.Replace(safeFileName, @"[^a-zA-Z0-9_]", "_");
                    builder.Options.TemplateFilename = $"{safeFileName}.cshtml";
                },
                cancellationToken);
            
            return compiled;
        }
        catch (RazorEngineCompilationException ex)
        {
            _logger.LogError(ex, "Template compilation failed: {TemplatePath}\nErrors: {Errors}", 
                templatePath, string.Join("\n", ex.Errors?.Select(e => e.ToString()) ?? Array.Empty<string>()));
            throw;
        }
    }

    /// <summary>
    /// Sets up callbacks on the template instance.
    /// </summary>
    private void SetupTemplateCallbacks<TModel>(SnowTemplateBase<TModel> instance, string currentTemplatePath, CancellationToken cancellationToken = default)
    {
        instance.IncludeCallback = async (templateName) =>
        {
            var includePath = ResolveLayoutPath(templateName, currentTemplatePath);
            if (includePath == null)
            {
                return $"<!-- Include not found: {templateName} -->";
            }
            
            var includeContent = await _fileProvider.ReadAllTextAsync(includePath);
            // Use caching for includes as well
            var compiled = await GetOrCompileTemplateAsync<TModel>(includeContent, includePath, cancellationToken);
            return await compiled.RunAsync(inc =>
            {
                inc.Model = instance.Model;
                SetupTemplateCallbacks(inc, includePath, cancellationToken);
            });
        };
    }

    /// <summary>
    /// Resolves the full path of a layout template.
    /// </summary>
    private string? ResolveLayoutPath(string layoutName, string currentTemplatePath)
    {
        var normalized = NormalizeTemplatePath(layoutName);
        var currentDir = Path.GetDirectoryName(currentTemplatePath) ?? "";
        
        // Search in the same directory as the current template first
        var searchPaths = new[]
        {
            Path.Combine(currentDir, normalized),
            Path.Combine(currentDir, "_layouts", normalized),
            ResolveFullPath(normalized),
            ResolveFullPath(Path.Combine("_layouts", normalized))
        };
        
        foreach (var path in searchPaths.Where(p => p != null))
        {
            if (_fileProvider.FileExists(path!))
            {
                return path;
            }
        }
        
        return null;
    }

    /// <inheritdoc />
    public string Render<TModel>(string templatePath, TModel model, IDictionary<string, object>? viewData = null)
    {
        return RenderAsync(templatePath, model, viewData).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public bool TemplateExists(string templatePath)
    {
        if (string.IsNullOrWhiteSpace(templatePath))
            return false;

        try
        {
            var resolvedPath = ResolveFullPath(NormalizeTemplatePath(templatePath));
            return resolvedPath != null;
        }
        catch
        {
            return false;
        }
    }

    private string? ResolveFullPath(string templatePath)
    {
        var normalizedPath = NormalizeTemplatePath(templatePath);
        var baseDir = Path.GetFullPath(_settings.CurrentDirectory);
        
        var searchPaths = new[]
        {
            Path.Combine(baseDir, _settings.ThemesDirectory, _settings.Theme, 
                Path.GetFileName(_settings.LayoutsDirectory.TrimEnd('/', '\\')), normalizedPath),
            Path.Combine(baseDir, _settings.ThemesDirectory, _settings.Theme, normalizedPath),
            Path.Combine(_settings.LayoutsDirectory, normalizedPath),
            Path.Combine(baseDir, normalizedPath),
        };

        foreach (var searchPath in searchPaths)
        {
            var fullPath = Path.GetFullPath(searchPath);
            
            // Security: Ensure resolved path is within current directory
            if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            if (_fileProvider.FileExists(fullPath))
            {
                return fullPath;
            }
        }
        
        return null;
    }

    private static string NormalizeTemplatePath(string templatePath)
    {
        var normalized = templatePath.TrimStart('/', '\\').Replace('\\', '/');
        
        if (!normalized.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".cshtml";
        }

        return normalized;
    }

    private static string GenerateErrorHtml(string templatePath, string errorMessage, string? details = null)
    {
        var detailsHtml = details != null 
            ? $"<pre>{System.Net.WebUtility.HtmlEncode(details)}</pre>" 
            : "";
        
        return $@"<html><body><h1>Template Error</h1>
<p>Failed to render template: {System.Net.WebUtility.HtmlEncode(templatePath)}</p>
<p>Error: {System.Net.WebUtility.HtmlEncode(errorMessage)}</p>
{detailsHtml}
</body></html>";
    }
}
