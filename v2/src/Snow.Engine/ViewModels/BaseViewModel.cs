using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Snow.Abstractions;

namespace Snow.Engine.ViewModels;

/// <summary>
/// Simple view model for template rendering with all necessary data.
/// </summary>
public class TemplateViewModel
{
    /// <summary>
    /// Gets or sets the title of the content.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTML content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publication date.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the relative URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author's name.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the meta description for SEO.
    /// </summary>
    public string MetaDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the site configuration settings.
    /// </summary>
    public ISiteSettings Settings { get; set; } = null!;

    /// <summary>
    /// Gets or sets all published posts in the site.
    /// </summary>
    public IReadOnlyList<IPost> Posts { get; set; } = Array.Empty<IPost>();

    /// <summary>
    /// Gets or sets all published pages in the site.
    /// </summary>
    public IReadOnlyList<IPage> Pages { get; set; } = Array.Empty<IPage>();

    /// <summary>
    /// Gets or sets all draft posts in the site.
    /// </summary>
    public IReadOnlyList<IPost> Drafts { get; set; } = Array.Empty<IPost>();

    /// <summary>
    /// Gets or sets all categories in the site.
    /// </summary>
    public IReadOnlyList<ICategory> AllCategories { get; set; } = Array.Empty<ICategory>();

    /// <summary>
    /// Gets or sets additional properties from front matter or context.
    /// </summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();

    /// <summary>
    /// Creates a TemplateViewModel from a Post model.
    /// </summary>
    public static TemplateViewModel FromPost(IPost post, IReadOnlyList<IPost> posts, IReadOnlyList<IPage> pages, IReadOnlyList<IPost> drafts, IReadOnlyList<ICategory> categories, ISiteSettings settings)
    {
        return new TemplateViewModel
        {
            Title = post.Title,
            Content = post.Content,
            Date = post.Date,
            Url = post.Url,
            Author = post.Author,
            MetaDescription = post.MetaDescription,
            Settings = settings,
            Posts = posts,
            Pages = pages,
            Drafts = drafts,
            AllCategories = categories
        };
    }

    /// <summary>
    /// Creates a TemplateViewModel from a Page model.
    /// </summary>
    public static TemplateViewModel FromPage(IPage page, IReadOnlyList<IPost> posts, IReadOnlyList<IPage> pages, IReadOnlyList<IPost> drafts, IReadOnlyList<ICategory> categories, ISiteSettings settings)
    {
        return new TemplateViewModel
        {
            Title = page.Title,
            Content = page.Content,
            Date = page.Date,
            Url = page.Url,
            Author = page.Author,
            MetaDescription = page.MetaDescription,
            Settings = settings,
            Posts = posts,
            Pages = pages,
            Drafts = drafts,
            AllCategories = categories
        };
    }
}
