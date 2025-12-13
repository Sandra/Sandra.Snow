using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Snow.Abstractions;

/// <summary>
/// Interface for rendering templates with model data.
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// Renders a template with the provided model data.
    /// </summary>
    /// <param name="templatePath">Path to the template file.</param>
    /// <param name="model">The model data to render.</param>
    /// <param name="viewData">Additional view data for rendering.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The rendered HTML content.</returns>
    Task<string> RenderAsync<TModel>(
        string templatePath, 
        TModel model, 
        IDictionary<string, object>? viewData = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a template with the provided model data synchronously.
    /// </summary>
    /// <param name="templatePath">Path to the template file.</param>
    /// <param name="model">The model data to render.</param>
    /// <param name="viewData">Additional view data for rendering.</param>
    /// <returns>The rendered HTML content.</returns>
    string Render<TModel>(
        string templatePath, 
        TModel model, 
        IDictionary<string, object>? viewData = null);

    /// <summary>
    /// Checks if a template exists at the specified path.
    /// </summary>
    /// <param name="templatePath">Path to check for template.</param>
    /// <returns>True if the template exists, false otherwise.</returns>
    bool TemplateExists(string templatePath);
}
