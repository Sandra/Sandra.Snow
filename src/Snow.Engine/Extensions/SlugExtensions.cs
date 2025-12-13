using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Snow.Abstractions;

namespace Snow.Engine.Extensions;

/// <summary>
/// Extensions for URL slug generation and formatting.
/// </summary>
public static class SlugExtensions
{
    private static readonly Regex SpaceRegex = new(@"\s", RegexOptions.Compiled);
    private static readonly Regex InvalidCharsRegex = new(@"[^a-z0-9\s\-_\.]", RegexOptions.Compiled);
    private static readonly Regex DuplicateCharsRegex = new(@"([\-_]){2,}", RegexOptions.Compiled);

    private static readonly SortedList<int, Func<string, IPost, string>> UrlFormatParser = new()
    {
        { 0, DayFull },
        { 1, DayAbbreviated },
        { 2, Day },
        { 3, MonthFull },
        { 4, MonthAbbreviated },
        { 5, Month },
        { 6, YearFull },
        { 7, Year },
        { 8, Slug },
        { 9, Category },
        { 10, Author }
    };

    /// <summary>
    /// Converts a string to a URL-friendly slug.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A URL-friendly slug.</returns>
    public static string ToUrlSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // First to lower case
        value = value.ToLowerInvariant();

        // Remove diacritics (accents)
        value = RemoveDiacritics(value);

        // Replace ampersand first
        value = value.Replace("&", "and");

        // Remove invalid chars
        value = InvalidCharsRegex.Replace(value, string.Empty);

        // Replace spaces with hyphens
        value = SpaceRegex.Replace(value, "-");

        // Trim dashes from end
        value = value.Trim('-', '_');

        // Replace double occurrences of - or _
        value = DuplicateCharsRegex.Replace(value, "$1");

        return value;
    }

    /// <summary>
    /// Appends a trailing slash if the URL doesn't point to a file.
    /// </summary>
    /// <param name="url">The URL to process.</param>
    /// <returns>The URL with a trailing slash if necessary.</returns>
    public static string AppendSlashIfNecessary(this string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        return url.IsFileUrl() ? url : url + "/";
    }

    /// <summary>
    /// Determines if a URL points to a file (contains extension).
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>True if the URL appears to point to a file, false otherwise.</returns>
    public static bool IsFileUrl(this string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        // Check if the last segment (after the last slash, or the entire string if no slash) contains a dot
        var lastSegment = url.Contains('/') ? url.Split('/').LastOrDefault() : url;
        return lastSegment?.Contains('.') == true;
    }

    /// <summary>
    /// Sets URLs for posts based on the site settings URL format.
    /// </summary>
    /// <param name="posts">The posts to set URLs for.</param>
    /// <param name="settings">The site settings containing URL format.</param>
    public static void SetPostUrls(this IEnumerable<IPost> posts, ISiteSettings settings)
    {
        foreach (var post in posts)
        {
            SetPostUrl(post, settings);
        }
    }

    /// <summary>
    /// Sets URLs for pages based on the site settings URL format.
    /// </summary>
    /// <param name="pages">The pages to set URLs for.</param>
    /// <param name="settings">The site settings containing URL format.</param>
    public static void SetPageUrls(this IEnumerable<IPage> pages, ISiteSettings settings)
    {
        foreach (var page in pages)
        {
            SetPageUrl(page, settings);
        }
    }

    /// <summary>
    /// Sets the URL for a single post based on the URL format.
    /// </summary>
    /// <param name="post">The post to set URL for.</param>
    /// <param name="settings">The site settings containing URL format.</param>
    public static void SetPostUrl(this IPost post, ISiteSettings settings)
    {
        var urlFormat = "/" + settings.PostUrlFormat.Trim('/').AppendSlashIfNecessary();

        if (post.Published == PublishedStatus.Draft)
        {
            urlFormat = "/drafts" + urlFormat;
        }

        foreach (var formatter in UrlFormatParser.OrderBy(x => x.Key).Select(x => x.Value))
        {
            urlFormat = formatter.Invoke(urlFormat, post);
        }

        if (post is Models.Post mutablePost)
        {
            mutablePost.Url = "/" + urlFormat.TrimStart('/');
        }
    }

    /// <summary>
    /// Sets the URL for a single page based on the URL format.
    /// </summary>
    /// <param name="page">The page to set URL for.</param>
    /// <param name="settings">The site settings containing URL format.</param>
    public static void SetPageUrl(this IPage page, ISiteSettings settings)
    {
        var urlFormat = "/" + settings.PageUrlFormat.Trim('/').AppendSlashIfNecessary();

        // The page.Url contains the slug (possibly with leading slash from parsing)
        var slug = page.Url.TrimStart('/');

        // Apply URL formatting (pages use slug directly)
        urlFormat = urlFormat.Replace("{slug}", slug).Replace("slug", slug);

        if (page is Models.Page mutablePage)
        {
            mutablePage.Url = "/" + urlFormat.TrimStart('/');
        }
    }

    /// <summary>
    /// Removes diacritics (accents) from a string.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <returns>Text without diacritics.</returns>
    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Updates series navigation to ensure all parts have correct URLs.
    /// </summary>
    /// <param name="posts">The posts to update.</param>
    public static void UpdateSeriesNavigation(this IEnumerable<IPost> posts)
    {
        var postsList = posts.ToList();
        var groupBySeriesId = (from x in postsList
                               where x.Series != null
                               where x.Published == PublishedStatus.Published
                               group x by x.Series!.Name
                               into g
                               select g).ToList();

        foreach (var seriesGroup in groupBySeriesId)
        {
            // Get all series as mutable types
            var mutableSeriesInGroup = seriesGroup
                .Select(x => x.Series)
                .OfType<Models.Series>()
                .ToList();

            if (!mutableSeriesInGroup.Any())
                continue;

            var latestSeries = mutableSeriesInGroup
                .OrderByDescending(x => seriesGroup.First(p => p.Series == x).Date)
                .First();

            // Fix up all the Parts to have the latest URLs
            foreach (var partKvp in latestSeries.MutableParts)
            {
                var post = seriesGroup.FirstOrDefault(x => x.Series?.Current == partKvp.Key);
                if (post != null)
                {
                    partKvp.Value.Url = post.Url;
                }
            }

            // Copy the latest parts to all posts in the series
            foreach (var series in mutableSeriesInGroup)
            {
                if (series != latestSeries)
                {
                    series.MutableParts.Clear();
                    foreach (var part in latestSeries.MutableParts)
                    {
                        series.MutableParts[part.Key] = part.Value;
                    }
                }
            }
        }
    }

    // URL format helper methods
    private static string DayFull(string url, IPost post) => url.Replace("dddd", post.Date.ToString("dddd"));
    private static string DayAbbreviated(string url, IPost post) => url.Replace("ddd", post.Date.ToString("ddd"));
    private static string Day(string url, IPost post) => url.Replace("dd", post.Date.ToString("dd"));
    private static string Month(string url, IPost post) => url.Replace("MM", post.Date.ToString("MM"));
    private static string MonthAbbreviated(string url, IPost post) => url.Replace("MMM", post.Date.ToString("MMM"));
    private static string MonthFull(string url, IPost post) => url.Replace("MMMM", post.Date.ToString("MMMM"));
    private static string YearFull(string url, IPost post) => url.Replace("yyyy", post.Date.ToString("yyyy"));
    private static string Year(string url, IPost post) => url.Replace("yy", post.Date.ToString("yy"));
    private static string Slug(string url, IPost post) => url.Replace("{slug}", post.Url).Replace("slug", post.Url);
    private static string Category(string url, IPost post) => url.Replace("{category}", post.Categories.FirstOrDefault() ?? string.Empty);
    private static string Author(string url, IPost post) => url.Replace("{author}", post.Author ?? string.Empty);
}
