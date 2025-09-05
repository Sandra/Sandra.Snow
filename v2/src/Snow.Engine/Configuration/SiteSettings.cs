using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Snow.Abstractions;

namespace Snow.Engine.Configuration;

/// <summary>
/// Represents the site configuration settings.
/// </summary>
public class SiteSettings : ISiteSettings
{
    private readonly string _currentDirectory;
    private string? _postsDirectory;
    private string? _pagesDirectory;
    private string? _layoutsDirectory;
    private string? _postsOutput;
    private string? _pagesOutput;

    public SiteSettings()
    {
        _currentDirectory = string.Empty;
        Author = string.Empty;
        Email = string.Empty;
        BlogTitle = string.Empty;
        SiteUrl = string.Empty;
        Theme = "default";
        ThemesDirectory = "themes/";
        DefaultPostLayout = "post";
        DefaultPageLayout = "page";
        PostUrlFormat = "yyyy/MM/dd/slug";
        PageUrlFormat = "slug";
        CopyDirectories = Array.Empty<string>();
        CopyFiles = Array.Empty<string>();
        ProcessFiles = Array.Empty<StaticFile>();
        PageSize = 10;
        FeedSize = 10;
    }

    public SiteSettings(string currentDirectory) : this()
    {
        _currentDirectory = currentDirectory.TrimEnd('/', '\\');
    }

    /// <inheritdoc />
    public string BlogTitle { get; set; }

    /// <inheritdoc />
    public string SiteUrl { get; set; }

    /// <inheritdoc />
    public string Author { get; set; }

    /// <inheritdoc />
    public string Email { get; set; }

    /// <inheritdoc />
    public string CurrentDirectory => _currentDirectory;

    /// <summary>
    /// Gets or sets the raw posts directory path before combination with current directory.
    /// </summary>
    [JsonPropertyName("posts")]
    public string PostsDirectoryRaw { get; set; } = "_posts";

    /// <inheritdoc />
    public string PostsDirectory
    {
        get => _postsDirectory ??= Path.Combine(_currentDirectory, PostsDirectoryRaw);
        set
        {
            PostsDirectoryRaw = value;
            _postsDirectory = Path.Combine(_currentDirectory, value);
        }
    }

    /// <summary>
    /// Gets or sets the raw pages directory path before combination with current directory.
    /// </summary>
    [JsonPropertyName("pages")]
    public string PagesDirectoryRaw { get; set; } = "_pages";

    /// <inheritdoc />
    public string PagesDirectory
    {
        get => _pagesDirectory ??= Path.Combine(_currentDirectory, PagesDirectoryRaw);
        set
        {
            PagesDirectoryRaw = value;
            _pagesDirectory = Path.Combine(_currentDirectory, value);
        }
    }

    /// <summary>
    /// Gets or sets the raw layouts directory path before combination with current directory.
    /// </summary>
    [JsonPropertyName("layouts")]
    public string LayoutsDirectoryRaw { get; set; } = "_layouts";

    /// <inheritdoc />
    public string LayoutsDirectory
    {
        get => _layoutsDirectory ??= Path.Combine(_currentDirectory, LayoutsDirectoryRaw);
        set
        {
            LayoutsDirectoryRaw = value;
            _layoutsDirectory = Path.Combine(_currentDirectory, value);
        }
    }

    /// <inheritdoc />
    public string ThemesDirectory { get; set; }

    /// <inheritdoc />
    public string Theme { get; set; }

    /// <summary>
    /// Gets or sets the raw posts output directory path before combination with current directory.
    /// </summary>
    [JsonPropertyName("postsOutput")]
    public string PostsOutputRaw { get; set; } = "Website";

    /// <inheritdoc />
    public string PostsOutput
    {
        get => _postsOutput ??= Path.Combine(_currentDirectory, PostsOutputRaw);
        set
        {
            PostsOutputRaw = value;
            _postsOutput = Path.Combine(_currentDirectory, value);
        }
    }

    /// <summary>
    /// Gets or sets the raw pages output directory path before combination with current directory.
    /// </summary>
    [JsonPropertyName("pagesOutput")]
    public string PagesOutputRaw { get; set; } = "Website";

    /// <inheritdoc />
    public string PagesOutput
    {
        get => _pagesOutput ??= Path.Combine(_currentDirectory, PagesOutputRaw);
        set
        {
            PagesOutputRaw = value;
            _pagesOutput = Path.Combine(_currentDirectory, value);
        }
    }

    /// <inheritdoc />
    [JsonPropertyName("urlFormat")]
    public string PostUrlFormat { get; set; }

    /// <inheritdoc />
    public string PageUrlFormat { get; set; }

    /// <inheritdoc />
    public string DefaultPostLayout { get; set; }

    /// <inheritdoc />
    public string DefaultPageLayout { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<string> CopyDirectories { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<string> CopyFiles { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<IStaticFile> ProcessFiles { get; set; }

    /// <inheritdoc />
    public int PageSize { get; set; }

    /// <inheritdoc />
    public int FeedSize { get; set; }

    /// <summary>
    /// Creates a default site settings instance for the specified directory.
    /// </summary>
    /// <param name="directory">The current directory.</param>
    /// <returns>A new SiteSettings instance with default values.</returns>
    public static SiteSettings CreateDefault(string directory)
    {
        return new SiteSettings(directory)
        {
            PostsDirectoryRaw = "_posts",
            PagesDirectoryRaw = "_pages", 
            LayoutsDirectoryRaw = "_layouts",
            Theme = "default",
            ThemesDirectory = "themes/",
            PostsOutputRaw = "Website",
            PagesOutputRaw = "Website",
            PostUrlFormat = "yyyy/MM/dd/slug",
            PageUrlFormat = "slug",
            CopyDirectories = Array.Empty<string>(),
            CopyFiles = Array.Empty<string>(),
            ProcessFiles = Array.Empty<StaticFile>(),
            PageSize = 10,
            FeedSize = 10,
            Author = string.Empty,
            Email = string.Empty,
            DefaultPostLayout = "post",
            DefaultPageLayout = "page"
        };
    }

    /// <summary>
    /// Refreshes all computed paths after the current directory or raw paths have changed.
    /// </summary>
    public void RefreshPaths()
    {
        _postsDirectory = null;
        _pagesDirectory = null;
        _layoutsDirectory = null;
        _postsOutput = null;
        _pagesOutput = null;
    }
}
