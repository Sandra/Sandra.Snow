using System;
using System.Collections.Generic;

namespace Snow.Abstractions;

/// <summary>
/// Represents a blog post.
/// </summary>
public interface IPost
{
    /// <summary>
    /// Gets the title of the post.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the HTML content of the post.
    /// </summary>
    string Content { get; }

    /// <summary>
    /// Gets the excerpt of the post for listings.
    /// </summary>
    string ContentExcerpt { get; }

    /// <summary>
    /// Gets the URL slug for the post.
    /// </summary>
    string Url { get; }

    /// <summary>
    /// Gets the author of the post.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Gets the author's email.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets the publication date of the post.
    /// </summary>
    DateTime Date { get; }

    /// <summary>
    /// Gets the year component of the publication date.
    /// </summary>
    int Year { get; }

    /// <summary>
    /// Gets the month component of the publication date.
    /// </summary>
    int Month { get; }

    /// <summary>
    /// Gets the day component of the publication date.
    /// </summary>
    int Day { get; }

    /// <summary>
    /// Gets the layout template to use for rendering.
    /// </summary>
    string Layout { get; }

    /// <summary>
    /// Gets the filename of the source file.
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Gets the categories the post belongs to.
    /// </summary>
    IReadOnlyList<string> Categories { get; }

    /// <summary>
    /// Gets the keywords/tags for the post.
    /// </summary>
    string Keywords { get; }

    /// <summary>
    /// Gets the meta description for the post.
    /// </summary>
    string MetaDescription { get; }

    /// <summary>
    /// Gets the publication status of the post.
    /// </summary>
    PublishedStatus Published { get; }

    /// <summary>
    /// Gets the series information if the post is part of a series.
    /// </summary>
    ISeries? Series { get; }

    /// <summary>
    /// Gets additional custom settings from the front matter.
    /// </summary>
    IReadOnlyDictionary<string, object> Settings { get; }

    /// <summary>
    /// Gets the raw markdown header from the source file.
    /// </summary>
    string MarkdownHeader { get; }
}

/// <summary>
/// Represents the publication status of a post.
/// </summary>
public enum PublishedStatus
{
    /// <summary>
    /// Post is published and visible.
    /// </summary>
    Published = 0,

    /// <summary>
    /// Post is private (generated but excluded from output).
    /// </summary>
    Private = 1,

    /// <summary>
    /// Post is a draft (generated but hidden from main listings).
    /// </summary>
    Draft = 2
}
