using System;
using System.Threading.Tasks;
using RazorEngineCore;

namespace Snow.Engine.Templates.Razor;

/// <summary>
/// Base template class for Snow Razor templates.
/// Provides v1 Nancy-compatible @Html helpers and layout support.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public abstract class SnowTemplateBase<TModel> : RazorEngineTemplateBase<TModel>
{
    private SnowHtmlHelpers<TModel>? _html;

    /// <summary>
    /// Gets the Html helper instance with Nancy-compatible methods.
    /// </summary>
    public SnowHtmlHelpers<TModel> Html => _html ??= new SnowHtmlHelpers<TModel>(this);

    /// <summary>
    /// Gets or sets the layout template name.
    /// Set by template code: @{ Layout = "default.cshtml"; }
    /// </summary>
    public string? Layout { get; set; }

    /// <summary>
    /// Gets or sets the callback for including partial templates.
    /// Set by the renderer before template execution.
    /// </summary>
    public Func<string, Task<string>>? IncludeCallback { get; set; }

    /// <summary>
    /// Gets or sets the rendered body content for use in layouts.
    /// Set by the renderer when rendering layout templates.
    /// </summary>
    public string? BodyContent { get; set; }

    /// <summary>
    /// Renders the body content in a layout template.
    /// </summary>
    public string RenderBody()
    {
        return BodyContent ?? string.Empty;
    }

    /// <summary>
    /// Includes a partial template.
    /// </summary>
    public async Task<string> Include(string templateName)
    {
        if (IncludeCallback == null)
            return $"<!-- Include callback not set for: {templateName} -->";
        
        return await IncludeCallback(templateName);
    }

    /// <summary>
    /// Includes a partial template (synchronous version).
    /// </summary>
    public string IncludeSync(string templateName)
    {
        return Include(templateName).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Non-generic base template for dynamic models.
/// </summary>
public abstract class SnowTemplateBase : SnowTemplateBase<dynamic>
{
}
