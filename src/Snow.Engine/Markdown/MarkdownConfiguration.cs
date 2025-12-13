using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Extensions.AutoIdentifiers;

namespace Snow.Engine.Markdown;

/// <summary>
/// Configuration for markdown processing with Markdig.
/// </summary>
public class MarkdownConfiguration
{
    private readonly List<string> _extensions = new()
    {
        "emoji",
        "smartypants", 
        "tables",
        "tasklists",
        "autolinks"
    };

    /// <summary>
    /// Gets the enabled extensions.
    /// </summary>
    public IReadOnlyList<string> Extensions => _extensions.AsReadOnly();

    /// <summary>
    /// Gets or sets whether to treat soft line breaks as hard line breaks.
    /// </summary>
    public bool HardLineBreaks { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to sanitize HTML (disable raw HTML).
    /// Recommended: true for security. Set to false only for trusted content.
    /// </summary>
    public bool SanitizeHtml { get; set; } = true;

    /// <summary>
    /// Gets or sets the heading ID generation strategy.
    /// </summary>
    public HeadingIdStyle HeadingIds { get; set; } = HeadingIdStyle.GitHub;

    /// <summary>
    /// Gets or sets the syntax highlighting configuration.
    /// </summary>
    public SyntaxHighlightingConfiguration SyntaxHighlighting { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to enable advanced extensions by default.
    /// </summary>
    public bool EnableAdvancedExtensions { get; set; } = true;

    /// <summary>
    /// Adds extensions to the configuration.
    /// </summary>
    /// <param name="extensions">Extensions to add.</param>
    /// <returns>This configuration instance for fluent chaining.</returns>
    public MarkdownConfiguration WithExtensions(params string[] extensions)
    {
        _extensions.AddRange(extensions);
        return this;
    }

    /// <summary>
    /// Removes extensions from the configuration.
    /// </summary>
    /// <param name="extensions">Extensions to remove.</param>
    /// <returns>This configuration instance for fluent chaining.</returns>
    public MarkdownConfiguration WithoutExtensions(params string[] extensions)
    {
        foreach (var extension in extensions)
        {
            _extensions.Remove(extension);
        }
        return this;
    }

    /// <summary>
    /// Clears all extensions and sets new ones.
    /// </summary>
    /// <param name="extensions">Extensions to set.</param>
    /// <returns>This configuration instance for fluent chaining.</returns>
    public MarkdownConfiguration SetExtensions(params string[] extensions)
    {
        _extensions.Clear();
        _extensions.AddRange(extensions);
        return this;
    }

    /// <summary>
    /// Creates a default markdown configuration.
    /// </summary>
    /// <returns>A default configuration instance.</returns>
    public static MarkdownConfiguration Default()
    {
        return new MarkdownConfiguration();
    }

    /// <summary>
    /// Validates the configuration and returns any validation errors.
    /// </summary>
    /// <returns>Collection of validation error messages.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        // Validate extensions
        var validExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "emoji", "smartypants", "tables", "tasklists", "autolinks",
            "math", "footnotes", "abbreviations", "definitionlists",
            "figures", "customcontainers", "medialinks", "genericattributes"
        };

        var invalidExtensions = Extensions.Where(ext => !validExtensions.Contains(ext)).ToList();
        if (invalidExtensions.Any())
        {
            errors.Add($"Invalid markdown extensions: {string.Join(", ", invalidExtensions)}");
        }

        return errors;
    }
}

/// <summary>
/// Heading ID generation styles.
/// </summary>
public enum HeadingIdStyle
{
    /// <summary>
    /// No heading IDs generated.
    /// </summary>
    None,

    /// <summary>
    /// Use GitHub-style heading ID generation.
    /// </summary>
    GitHub,

    /// <summary>
    /// Use legacy MarkdownSharp-style heading IDs for compatibility.
    /// </summary>
    Legacy
}

/// <summary>
/// Configuration for syntax highlighting.
/// </summary>
public class SyntaxHighlightingConfiguration
{
    /// <summary>
    /// Gets or sets whether syntax highlighting is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the syntax highlighting theme.
    /// </summary>
    public string Theme { get; set; } = "prism-dark";

    /// <summary>
    /// Gets or sets whether to generate line numbers.
    /// </summary>
    public bool LineNumbers { get; set; } = false;
}
