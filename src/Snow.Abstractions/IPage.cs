using System;
using System.Collections.Generic;

namespace Snow.Abstractions;

/// <summary>
/// Represents a static page.
/// </summary>
public interface IPage
{
    /// <summary>
    /// Gets the title of the page.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the HTML content of the page.
    /// </summary>
    string Content { get; }

    /// <summary>
    /// Gets the excerpt of the page.
    /// </summary>
    string ContentExcerpt { get; }

    /// <summary>
    /// Gets the URL slug for the page.
    /// </summary>
    string Url { get; }

    /// <summary>
    /// Gets the author of the page.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Gets the author's email.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets the date of the page (if specified).
    /// </summary>
    DateTime Date { get; }

    /// <summary>
    /// Gets the layout template to use for rendering.
    /// </summary>
    string Layout { get; }

    /// <summary>
    /// Gets the filename of the source file.
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Gets the categories the page belongs to.
    /// </summary>
    IReadOnlyList<string> Categories { get; }

    /// <summary>
    /// Gets the keywords/tags for the page.
    /// </summary>
    string Keywords { get; }

    /// <summary>
    /// Gets the meta description for the page.
    /// </summary>
    string MetaDescription { get; }

    /// <summary>
    /// Gets the publication status of the page.
    /// </summary>
    PublishedStatus Published { get; }

    /// <summary>
    /// Gets additional custom settings from the front matter.
    /// </summary>
    IReadOnlyDictionary<string, object> Settings { get; }

    /// <summary>
    /// Gets the raw markdown header from the source file.
    /// </summary>
    string MarkdownHeader { get; }
}
