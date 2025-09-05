using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.Configuration;
using Snow.Engine.Extensions;
using Snow.Engine.IO;
using Snow.Engine.Models;

namespace Snow.Engine.Parsing;

/// <summary>
/// Parses blog posts from markdown files with YYYY-MM-DD-slug naming convention.
/// </summary>
public class PostParser : IPostParser
{
    private static readonly Regex FileNameRegex = new(
        @"^(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})-(?<slug>.+)\.(?:md|markdown)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IMarkdownParser _markdownParser;
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<PostParser> _logger;

    public PostParser(IMarkdownParser markdownParser, IFileProvider fileProvider, ILogger<PostParser> logger)
    {
        _markdownParser = markdownParser ?? throw new ArgumentNullException(nameof(markdownParser));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IPost?> ParsePostAsync(IFileInfo fileInfo, ISiteSettings settings, CancellationToken cancellationToken = default)
    {
        if (!CanParsePost(fileInfo.Name))
        {
            _logger.LogDebug("Skipping file {FileName} - does not match post naming convention", fileInfo.Name);
            return null;
        }

        try
        {
            var rawContent = await _fileProvider.ReadAllTextAsync(fileInfo.FullPath, cancellationToken);
            return await ParsePostContentAsync(fileInfo.Name, rawContent, settings, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing post from file {FileName}", fileInfo.Name);
            return new Post.MissingPost();
        }
    }

    /// <inheritdoc />
    public IPost? ParsePost(IFileInfo fileInfo, ISiteSettings settings)
    {
        if (!CanParsePost(fileInfo.Name))
        {
            _logger.LogDebug("Skipping file {FileName} - does not match post naming convention", fileInfo.Name);
            return null;
        }

        try
        {
            var rawContent = System.IO.File.ReadAllText(fileInfo.FullPath);
            return ParsePostContent(fileInfo.Name, rawContent, settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing post from file {FileName}", fileInfo.Name);
            return new Post.MissingPost();
        }
    }

    /// <inheritdoc />
    public bool CanParsePost(string fileName)
    {
        return FileNameRegex.IsMatch(fileName);
    }

    private async Task<IPost> ParsePostContentAsync(string fileName, string rawContent, ISiteSettings settings, CancellationToken cancellationToken)
    {
        var fileNameMatch = FileNameRegex.Match(fileName);
        if (!fileNameMatch.Success)
        {
            return new Post.MissingPost();
        }

        // Parse the markdown file
        var parsedFile = MarkdownFileParser.ParseDataFromFile(rawContent);
        var frontMatterSettings = FrontMatterParser.ParseSettings(parsedFile.Header);

        // Extract date from filename
        var year = int.Parse(fileNameMatch.Groups["year"].Value);
        var month = int.Parse(fileNameMatch.Groups["month"].Value);
        var day = int.Parse(fileNameMatch.Groups["day"].Value);
        var slug = fileNameMatch.Groups["slug"].Value.ToUrlSlug();
        var date = new DateTime(year, month, day);

        // Allow front matter date to override filename date
        if (frontMatterSettings.ContainsKey("date"))
        {
            if (DateTime.TryParse(frontMatterSettings["date"].ToString(), out var frontMatterDate))
            {
                date = frontMatterDate;
                year = date.Year;
                month = date.Month;
                day = date.Day;
            }
        }

        // Convert markdown to HTML
        var contentHtml = await _markdownParser.ParseAsync(parsedFile.Body, cancellationToken);
        var excerptHtml = !string.IsNullOrEmpty(parsedFile.Excerpt) 
            ? await ParseExcerptAsync(parsedFile.Excerpt, cancellationToken)
            : string.Empty;

        // Create the post
        var post = new Post
        {
            FileName = fileName,
            MarkdownHeader = parsedFile.Header,
            Content = contentHtml,
            Date = date,
            Year = year,
            Month = month,
            Day = day,
            Url = slug,
            Settings = frontMatterSettings,
            Published = PublishedStatus.Published // Default, will be overridden by front matter
        };

        // Set excerpt if provided in dedicated section
        if (!string.IsNullOrEmpty(excerptHtml))
        {
            post.ContentExcerpt = excerptHtml;
        }

        // Apply site defaults
        if (settings is SiteSettings siteSettings)
        {
            post.SetDefaults(siteSettings);
        }

        // Apply front matter settings
        post.SetFromFrontMatter(frontMatterSettings);

        return post;
    }

    private IPost ParsePostContent(string fileName, string rawContent, ISiteSettings settings)
    {
        return ParsePostContentAsync(fileName, rawContent, settings, CancellationToken.None)
               .GetAwaiter()
               .GetResult();
    }

    /// <summary>
    /// Parses excerpt content using the excerpt pipeline if available.
    /// </summary>
    private async Task<string> ParseExcerptAsync(string excerpt, CancellationToken cancellationToken)
    {
        if (_markdownParser is Markdown.MarkdigMarkdownParser markdigParser)
        {
            return await markdigParser.ParseExcerptAsync(excerpt, cancellationToken);
        }
        
        return await _markdownParser.ParseAsync(excerpt, cancellationToken);
    }
}
