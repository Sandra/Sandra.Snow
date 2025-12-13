using System.Collections.Generic;

namespace Snow.Abstractions;

/// <summary>
/// Represents a category with its associated posts.
/// </summary>
public interface ICategory
{
    /// <summary>
    /// Gets the name of the category.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the URL slug for the category.
    /// </summary>
    string Slug { get; }
    
    /// <summary>
    /// Gets the URL path for the category (v1 compatibility alias for Slug).
    /// </summary>
    string Url { get; }

    /// <summary>
    /// Gets the posts in this category.
    /// </summary>
    IReadOnlyList<IPost> Posts { get; }

    /// <summary>
    /// Gets the count of posts in this category.
    /// </summary>
    int PostCount { get; }
}
