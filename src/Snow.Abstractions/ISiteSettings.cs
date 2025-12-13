using System.Collections.Generic;

namespace Snow.Abstractions;

/// <summary>
/// Represents the site configuration settings.
/// </summary>
public interface ISiteSettings
{
    /// <summary>
    /// Gets the title of the blog/site.
    /// </summary>
    string BlogTitle { get; }

    /// <summary>
    /// Gets the base URL of the site.
    /// </summary>
    string SiteUrl { get; }

    /// <summary>
    /// Gets the default author name.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Gets the default author email.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets the current working directory.
    /// </summary>
    string CurrentDirectory { get; }

    /// <summary>
    /// Gets the posts source directory.
    /// </summary>
    string PostsDirectory { get; }

    /// <summary>
    /// Gets the pages source directory.
    /// </summary>
    string PagesDirectory { get; }

    /// <summary>
    /// Gets the layouts directory.
    /// </summary>
    string LayoutsDirectory { get; }

    /// <summary>
    /// Gets the themes directory.
    /// </summary>
    string ThemesDirectory { get; }

    /// <summary>
    /// Gets the current theme name.
    /// </summary>
    string Theme { get; }

    /// <summary>
    /// Gets the output directory for posts.
    /// </summary>
    string PostsOutput { get; }

    /// <summary>
    /// Gets the output directory for pages.
    /// </summary>
    string PagesOutput { get; }

    /// <summary>
    /// Gets the URL format for posts.
    /// </summary>
    string PostUrlFormat { get; }

    /// <summary>
    /// Gets the URL format for pages.
    /// </summary>
    string PageUrlFormat { get; }

    /// <summary>
    /// Gets the default layout for posts.
    /// </summary>
    string DefaultPostLayout { get; }

    /// <summary>
    /// Gets the default layout for pages.
    /// </summary>
    string DefaultPageLayout { get; }

    /// <summary>
    /// Gets the directories to copy during build.
    /// </summary>
    IReadOnlyList<string> CopyDirectories { get; }

    /// <summary>
    /// Gets the individual files to copy during build.
    /// </summary>
    IReadOnlyList<string> CopyFiles { get; }

    /// <summary>
    /// Gets the static files to process.
    /// </summary>
    IReadOnlyList<IStaticFile> ProcessFiles { get; }

    /// <summary>
    /// Gets the number of posts per page for pagination.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets the number of items in RSS/Atom feeds.
    /// </summary>
    int FeedSize { get; }
}
