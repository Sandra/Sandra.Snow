using System.Threading;
using System.Threading.Tasks;
using Snow.Abstractions;

namespace Snow.Engine.Parsing;

/// <summary>
/// Placeholder markdown parser that returns content as-is.
/// This will be replaced with a proper Markdig implementation in Stage 2.
/// </summary>
public class PlaceholderMarkdownParser : IMarkdownParser
{
    /// <inheritdoc />
    public Task<string> ParseAsync(string markdown, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Parse(markdown));
    }

    /// <inheritdoc />
    public string Parse(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;

        // Very basic markdown-to-HTML conversion for testing
        // This is a placeholder - we'll replace with Markdig in Stage 2
        return markdown
            .Replace("\n\n", "</p><p>") // Paragraphs
            .Replace("\n", "<br />")    // Line breaks
            .Insert(0, "<p>")           // Opening paragraph
            + "</p>";                   // Closing paragraph
    }
}
