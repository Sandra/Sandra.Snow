using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Snow.Abstractions;

namespace Snow.Engine.ViewModels;

/// <summary>
/// Represents an archive menu item for month/year navigation (v1 naming: MonthYear).
/// </summary>
public class ArchiveMenuItem
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// V1 compatibility: Count property alias for PostCount.
    /// </summary>
    public int Count => PostCount;
    
    /// <summary>
    /// V1 compatibility: Title property (formatted as "Month Year (Count)").
    /// </summary>
    public string Title => $"{MonthName} {Year} ({PostCount})";
}

/// <summary>
/// Base view model with properties common to all v1 templates.
/// </summary>
public class BasePostViewModel
{
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
    /// Gets or sets month/year menu items for archive navigation (v1 naming).
    /// </summary>
    public IReadOnlyList<ArchiveMenuItem> MonthYearList { get; set; } = Array.Empty<ArchiveMenuItem>();

    /// <summary>
    /// Gets or sets the header title chain for breadcrumbs.
    /// </summary>
    public IReadOnlyList<string> HeaderTitleChain { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the generated date for the site.
    /// </summary>
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the generated URL for the current page.
    /// </summary>
    public string GeneratedUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the keywords for SEO.
    /// </summary>
    public IReadOnlyList<string> Keywords { get; set; } = Array.Empty<string>();
}

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

    // Pagination properties for posts listings
    
    /// <summary>
    /// Gets or sets the paged posts for the current page.
    /// </summary>
    public IReadOnlyList<IPost> PostsPaged { get; set; } = Array.Empty<IPost>();

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    public int TotalPages { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Gets or sets whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Gets or sets the previous page number (v1 compatibility).
    /// </summary>
    public int PreviousPage { get; set; }

    /// <summary>
    /// Gets or sets the next page number (v1 compatibility).
    /// </summary>
    public int NextPage { get; set; }

    /// <summary>
    /// Gets or sets the generated URL for the current page.
    /// </summary>
    public string GeneratedUrl { get; set; } = string.Empty;

    // Category-specific properties

    /// <summary>
    /// Gets or sets the current category being rendered.
    /// </summary>
    public ICategory? Category { get; set; }

    /// <summary>
    /// Gets or sets posts in the current category.
    /// </summary>
    public IReadOnlyList<IPost> PostsInCategory { get; set; } = Array.Empty<IPost>();

    /// <summary>
    /// Gets or sets the header title chain for breadcrumbs.
    /// </summary>
    public IReadOnlyList<string> HeaderTitleChain { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the keywords for SEO (v1 compatibility).
    /// </summary>
    public IReadOnlyList<string> Keywords { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the generated date for the site (v1 compatibility).
    /// </summary>
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets month/year menu items for archive navigation (v1 naming).
    /// </summary>
    public IReadOnlyList<ArchiveMenuItem> MonthYearList { get; set; } = Array.Empty<ArchiveMenuItem>();

    // Archive properties

    /// <summary>
    /// Gets or sets archive data grouped by year then month.
    /// </summary>
    public IReadOnlyDictionary<int, IReadOnlyDictionary<int, IReadOnlyList<IPost>>>? Archives { get; set; }

    /// <summary>
    /// Gets or sets month/year menu items for archive navigation.
    /// </summary>
    public IReadOnlyList<ArchiveMenuItem>? MonthYearMenu { get; set; }

    /// <summary>
    /// Gets posts grouped by year then month for archive pages (v1 compatibility).
    /// </summary>
    public IReadOnlyDictionary<int, IReadOnlyDictionary<int, IReadOnlyList<IPost>>> PostsGroupedByYearThenMonth
    {
        get
        {
            return Posts
                .GroupBy(p => p.Date.Year)
                .OrderByDescending(g => g.Key)
                .ToDictionary(
                    yearGroup => yearGroup.Key,
                    yearGroup => (IReadOnlyDictionary<int, IReadOnlyList<IPost>>)yearGroup
                        .GroupBy(p => p.Date.Month)
                        .OrderByDescending(g => g.Key)
                        .ToDictionary(
                            monthGroup => monthGroup.Key,
                            monthGroup => (IReadOnlyList<IPost>)monthGroup.OrderByDescending(p => p.Date).ToList()
                        )
                );
        }
    }
    
    /// <summary>
    /// Gets the month name from a month number (v1 compatibility).
    /// </summary>
    public string GetMonth(int month)
    {
        return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
    }

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
