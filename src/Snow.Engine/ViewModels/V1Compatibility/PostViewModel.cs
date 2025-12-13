// V1 Compatibility - allows existing v1 templates with Snow.ViewModels types to work
// ReSharper disable CheckNamespace
namespace Snow.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using Snow.Abstractions;
using Snow.Engine.ViewModels;

/// <summary>
/// V1-compatible Base view model for layout templates.
/// This is used by layout templates that inherit from Snow.ViewModels.BaseViewModel.
/// </summary>
public class BaseViewModel : BasePostViewModel
{
    // All properties are inherited from BasePostViewModel
    // This class exists for v1 namespace compatibility
}

/// <summary>
/// V1-compatible Post view model for backward compatibility with existing templates.
/// </summary>
public class PostViewModel : BaseViewModel
{
    /// <summary>
    /// Gets or sets the post title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the post date (v1 naming convention).
    /// </summary>
    public DateTime PostDate { get; set; }

    /// <summary>
    /// Gets or sets the HTML content (v1 naming convention).
    /// </summary>
    public string PostContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the categories for this post (as ICategory objects for v1 template compatibility).
    /// </summary>
    public IReadOnlyList<ICategory> Categories { get; set; } = Array.Empty<ICategory>();
    
    /// <summary>
    /// Gets or sets the raw category names.
    /// </summary>
    public IReadOnlyList<string> CategoryNames { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the post URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the series information if this post is part of a series.
    /// </summary>
    public ISeries? Series { get; set; }

    /// <summary>
    /// Gets or sets the excerpt for this post.
    /// </summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>
    /// Creates a PostViewModel from a Post.
    /// </summary>
    public static PostViewModel FromPost(IPost post, BasePostViewModel baseModel)
    {
        // Map category names to ICategory objects from AllCategories
        var categories = baseModel.AllCategories
            .Where(c => post.Categories.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();
        
        return new PostViewModel
        {
            Title = post.Title,
            PostDate = post.Date,
            PostContent = post.Content,
            Categories = categories,
            CategoryNames = post.Categories,
            Author = post.Author,
            Url = post.Url,
            Series = post.Series,
            Excerpt = string.Empty, // Excerpt not available on IPost, use empty
            // Copy base properties
            Settings = baseModel.Settings,
            Posts = baseModel.Posts,
            Pages = baseModel.Pages,
            Drafts = baseModel.Drafts,
            AllCategories = baseModel.AllCategories,
            MonthYearList = baseModel.MonthYearList,
            HeaderTitleChain = baseModel.HeaderTitleChain,
            GeneratedDate = baseModel.GeneratedDate,
            GeneratedUrl = baseModel.GeneratedUrl,
            Keywords = baseModel.Keywords
        };
    }
    
    /// <summary>
    /// Creates a PostViewModel from a Page (v1 used PostViewModel for pages too).
    /// </summary>
    public static PostViewModel FromPage(IPage page, BasePostViewModel baseModel)
    {
        // Map category names to ICategory objects from AllCategories
        var categories = baseModel.AllCategories
            .Where(c => page.Categories.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();
        
        return new PostViewModel
        {
            Title = page.Title,
            PostDate = page.Date,
            PostContent = page.Content,
            Categories = categories,
            CategoryNames = page.Categories,
            Author = page.Author,
            Url = page.Url,
            Series = null,
            Excerpt = page.ContentExcerpt,
            // Copy base properties
            Settings = baseModel.Settings,
            Posts = baseModel.Posts,
            Pages = baseModel.Pages,
            Drafts = baseModel.Drafts,
            AllCategories = baseModel.AllCategories,
            MonthYearList = baseModel.MonthYearList,
            HeaderTitleChain = baseModel.HeaderTitleChain,
            GeneratedDate = baseModel.GeneratedDate,
            GeneratedUrl = baseModel.GeneratedUrl,
            Keywords = baseModel.Keywords
        };
    }
}

/// <summary>
/// V1-compatible Page view model for backward compatibility with existing templates.
/// </summary>
public class PageViewModel : BaseViewModel
{
    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTML content (v1 naming convention).
    /// </summary>
    public string PostContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the categories for this page (as ICategory objects for v1 template compatibility).
    /// </summary>
    public IReadOnlyList<ICategory> Categories { get; set; } = Array.Empty<ICategory>();
    
    /// <summary>
    /// Gets or sets the raw category names.
    /// </summary>
    public IReadOnlyList<string> CategoryNames { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the page URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Creates a PageViewModel from a Page.
    /// </summary>
    public static PageViewModel FromPage(IPage page, BasePostViewModel baseModel)
    {
        // Map category names to ICategory objects from AllCategories
        var categories = baseModel.AllCategories
            .Where(c => page.Categories.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();
        
        return new PageViewModel
        {
            Title = page.Title,
            PostContent = page.Content,
            Categories = categories,
            CategoryNames = page.Categories,
            Url = page.Url,
            // Copy base properties
            Settings = baseModel.Settings,
            Posts = baseModel.Posts,
            Pages = baseModel.Pages,
            Drafts = baseModel.Drafts,
            AllCategories = baseModel.AllCategories,
            MonthYearList = baseModel.MonthYearList,
            HeaderTitleChain = baseModel.HeaderTitleChain,
            GeneratedDate = baseModel.GeneratedDate,
            GeneratedUrl = baseModel.GeneratedUrl,
            Keywords = baseModel.Keywords
        };
    }
}

/// <summary>
/// V1-compatible Content view model for index pages and archive views.
/// </summary>
public class ContentViewModel : BaseViewModel
{
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTML content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

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
    /// Gets or sets the current category being rendered.
    /// </summary>
    public ICategory? Category { get; set; }

    /// <summary>
    /// Gets or sets posts in the current category or paged posts.
    /// </summary>
    public IReadOnlyList<IPost> PostsInCategory { get; set; } = Array.Empty<IPost>();
    
    /// <summary>
    /// Gets or sets the paged posts for the current page (v1 naming).
    /// </summary>
    public IReadOnlyList<IPost> PostsPaged { get; set; } = Array.Empty<IPost>();
    
    /// <summary>
    /// Gets or sets the previous page number (v1 compatibility).
    /// </summary>
    public int PreviousPage { get; set; }
    
    /// <summary>
    /// Gets or sets the next page number (v1 compatibility).
    /// </summary>
    public int NextPage { get; set; }
    
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
}
