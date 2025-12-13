using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Snow.Abstractions;

/// <summary>
/// Interface for template compilation and rendering engines.
/// </summary>
public interface ITemplateEngine
{
    /// <summary>
    /// Compiles a template for later rendering.
    /// </summary>
    /// <param name="templateKey">The key identifying the template.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task CompileAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a template with the provided model.
    /// </summary>
    /// <param name="templateKey">The key identifying the template.</param>
    /// <param name="model">The model data for rendering.</param>
    /// <param name="viewData">Additional view data for the template.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The rendered HTML content.</returns>
    Task<string> RenderAsync<TModel>(
        string templateKey, 
        TModel model, 
        IDictionary<string, object>? viewData = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a template with the provided model synchronously.
    /// </summary>
    /// <param name="templateKey">The key identifying the template.</param>
    /// <param name="model">The model data for rendering.</param>
    /// <param name="viewData">Additional view data for the template.</param>
    /// <returns>The rendered HTML content.</returns>
    string Render<TModel>(
        string templateKey, 
        TModel model, 
        IDictionary<string, object>? viewData = null);

    /// <summary>
    /// Checks if a template exists with the specified key.
    /// </summary>
    /// <param name="templateKey">The template key to check.</param>
    /// <returns>True if the template exists, false otherwise.</returns>
    bool TemplateExists(string templateKey);

    /// <summary>
    /// Gets all available template keys.
    /// </summary>
    /// <returns>Collection of available template keys.</returns>
    Task<IReadOnlyList<string>> GetAvailableTemplatesAsync();

    /// <summary>
    /// Pre-compiles all available templates for improved runtime performance.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task PrecompileAllTemplatesAsync(CancellationToken cancellationToken = default);
}
