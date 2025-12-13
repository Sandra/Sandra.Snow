using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Snow.Abstractions;
using Snow.Engine.Templates;

namespace Snow.Engine.Adapters;

/// <summary>
/// Template renderer adapter that delegates to the simple template renderer.
/// This maintains the Nancy compatibility interface while using the new template system.
/// </summary>
public class ModernTemplateRenderer : ITemplateRenderer
{
    private readonly SimpleTemplateRenderer _simpleRenderer;

    public ModernTemplateRenderer(IFileProvider fileProvider, ISiteSettings settings, Microsoft.Extensions.Logging.ILogger<SimpleTemplateRenderer> logger)
    {
        _simpleRenderer = new SimpleTemplateRenderer(fileProvider, settings, logger);
    }

    /// <inheritdoc />
    public Task<string> RenderAsync<TModel>(string templatePath, TModel model, IDictionary<string, object>? viewData = null, CancellationToken cancellationToken = default)
    {
        return _simpleRenderer.RenderAsync(templatePath, model, viewData, cancellationToken);
    }

    /// <inheritdoc />
    public string Render<TModel>(string templatePath, TModel model, IDictionary<string, object>? viewData = null)
    {
        return _simpleRenderer.Render(templatePath, model, viewData);
    }

    /// <inheritdoc />
    public bool TemplateExists(string templatePath)
    {
        return _simpleRenderer.TemplateExists(templatePath);
    }
}
