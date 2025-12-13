using System.Threading;
using System.Threading.Tasks;

namespace Snow.Abstractions;

/// <summary>
/// Interface for parsing Markdown content to HTML.
/// </summary>
public interface IMarkdownParser
{
    /// <summary>
    /// Converts Markdown text to HTML.
    /// </summary>
    /// <param name="markdown">The Markdown content to convert.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The converted HTML content.</returns>
    Task<string> ParseAsync(string markdown, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Converts Markdown text to HTML synchronously.
    /// </summary>
    /// <param name="markdown">The Markdown content to convert.</param>
    /// <returns>The converted HTML content.</returns>
    string Parse(string markdown);
}
