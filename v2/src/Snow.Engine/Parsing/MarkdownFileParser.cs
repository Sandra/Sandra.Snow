using System;

namespace Snow.Engine.Parsing;

/// <summary>
/// Parses markdown files with YAML-like front matter and optional excerpt sections.
/// </summary>
public static class MarkdownFileParser
{
    /// <summary>
    /// Parses a markdown file with optional front matter and excerpt.
    /// </summary>
    /// <param name="rawContent">The raw file content.</param>
    /// <returns>The parsed file components.</returns>
    public static ParsedFile ParseDataFromFile(string rawContent)
    {
        if (string.IsNullOrEmpty(rawContent))
        {
            return new ParsedFile { Body = string.Empty };
        }

        var parsedFile = new ParsedFile();

        // Get the Header info from a Post Markdown File
        // Find the first index of ---
        var startOfSettingsIndex = rawContent.IndexOf("---", StringComparison.InvariantCultureIgnoreCase);
        if (startOfSettingsIndex >= 0)
        {
            // Find the second index of --- after the first
            var endOfSettingsIndex = rawContent.IndexOf("---", startOfSettingsIndex + 3,
                StringComparison.InvariantCultureIgnoreCase);

            // If we find the 2nd index, parse the settings
            // Otherwise we assume there's no header or settings...
            if (endOfSettingsIndex >= 0)
            {
                parsedFile.Header = rawContent.Substring(startOfSettingsIndex, endOfSettingsIndex + 3);
                parsedFile.Body = rawContent.Substring(endOfSettingsIndex + 3, rawContent.Length - (endOfSettingsIndex + 3));
            }
            else
            {
                // No closing --- found, treat entire content as body
                parsedFile.Body = rawContent;
            }
        }
        else
        {
            // No front matter, entire content is body
            parsedFile.Body = rawContent;
        }

        // Handle excerpt section: ---excerpt ... ---end (v2 format)
        var startOfExcerpt = rawContent.IndexOf("---excerpt", StringComparison.InvariantCultureIgnoreCase);
        if (startOfExcerpt >= 0)
        {
            var endOfExcerpt = rawContent.IndexOf("---end", startOfExcerpt + 10, StringComparison.InvariantCultureIgnoreCase);
            if (endOfExcerpt > startOfExcerpt)
            {
                parsedFile.Excerpt = rawContent.Substring(startOfExcerpt + 10, endOfExcerpt - (startOfExcerpt + 10)).Trim();
                
                // Remove the excerpt section from the body
                var beforeExcerpt = rawContent.Substring(0, startOfExcerpt);
                var afterExcerpt = rawContent.Substring(endOfExcerpt + 6);
                
                // Re-parse the body without the excerpt section
                var contentWithoutExcerpt = beforeExcerpt + afterExcerpt;
                var reparsed = ParseDataFromFile(contentWithoutExcerpt);
                
                parsedFile.Header = reparsed.Header;
                parsedFile.Body = reparsed.Body;
            }
        }
        
        // Handle inline excerpt marker: <!--excerpt--> (v1 format)
        // This marker splits the body into excerpt (before) and rest (after)
        // The marker itself should be removed from the content
        const string excerptMarker = "<!--excerpt-->";
        var excerptMarkerIndex = parsedFile.Body.IndexOf(excerptMarker, StringComparison.InvariantCultureIgnoreCase);
        if (excerptMarkerIndex >= 0 && string.IsNullOrEmpty(parsedFile.Excerpt))
        {
            // Extract content before the marker as excerpt
            parsedFile.Excerpt = parsedFile.Body.Substring(0, excerptMarkerIndex).Trim();
            
            // Remove the marker from body (keep content before and after)
            parsedFile.Body = parsedFile.Body.Substring(0, excerptMarkerIndex) + 
                              parsedFile.Body.Substring(excerptMarkerIndex + excerptMarker.Length);
        }

        // Trim whitespace from all components
        parsedFile.Header = parsedFile.Header?.Trim() ?? string.Empty;
        parsedFile.Body = parsedFile.Body?.Trim() ?? string.Empty;
        parsedFile.Excerpt = parsedFile.Excerpt?.Trim() ?? string.Empty;

        return parsedFile;
    }
}

/// <summary>
/// Represents a parsed markdown file with its components.
/// </summary>
public class ParsedFile
{
    /// <summary>
    /// Gets or sets the YAML-like front matter header.
    /// </summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the excerpt content (if specified with ---excerpt ... ---end).
    /// </summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the main body content.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
