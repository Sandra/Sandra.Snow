using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.Configuration;
using Snow.Engine.Extensions;
using Snow.Engine.Models;

namespace Snow.Engine.Parsing;

/// <summary>
/// Parses static pages from markdown files.
/// </summary>
public class PageParser : IPageParser
{
    private static readonly Regex FileNameRegex = new(
        @"(?<slug>.+)\.(?:md|markdown)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IMarkdownParser _markdownParser;
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<PageParser> _logger;

    public PageParser(IMarkdownParser markdownParser, IFileProvider fileProvider, ILogger<PageParser> logger)
    {
        _markdownParser = markdownParser ?? throw new ArgumentNullException(nameof(markdownParser));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IPage?> ParsePageAsync(IFileInfo fileInfo, ISiteSettings settings, CancellationToken cancellationToken = default)
    {
        if (!CanParsePage(fileInfo.Name))
        {
            _logger.LogDebug("Skipping file {FileName} - does not match page naming convention", fileInfo.Name);
            return null;
        }

        try
        {
            var rawContent = await _fileProvider.ReadAllTextAsync(fileInfo.FullPath, cancellationToken);
            return await ParsePageContentAsync(fileInfo.Name, rawContent, settings, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing page from file {FileName}", fileInfo.Name);
            return null;
        }
    }

    /// <inheritdoc />
    public IPage? ParsePage(IFileInfo fileInfo, ISiteSettings settings)
    {
        if (!CanParsePage(fileInfo.Name))
        {
            _logger.LogDebug("Skipping file {FileName} - does not match page naming convention", fileInfo.Name);
            return null;
        }

        try
        {
            var rawContent = System.IO.File.ReadAllText(fileInfo.FullPath);
            return ParsePageContent(fileInfo.Name, rawContent, settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing page from file {FileName}", fileInfo.Name);
            return null;
        }
    }

    /// <inheritdoc />
    public bool CanParsePage(string fileName)
    {
        return FileNameRegex.IsMatch(fileName);
    }

    private async Task<IPage> ParsePageContentAsync(string fileName, string rawContent, ISiteSettings settings, CancellationToken cancellationToken)
    {
        var fileNameMatch = FileNameRegex.Match(fileName);
        if (!fileNameMatch.Success)
        {
            throw new ArgumentException($"Invalid page file name format: {fileName}");
        }

        // Parse the markdown file
        var parsedFile = MarkdownFileParser.ParseDataFromFile(rawContent);
        var frontMatterSettings = FrontMatterParser.ParseSettings(parsedFile.Header);

        // Extract slug from filename
        var slug = fileNameMatch.Groups["slug"].Value.ToUrlSlug();

        // Default date to current time if not specified in front matter
        var date = DateTime.Now;

        // Convert markdown to HTML
        var contentHtml = await _markdownParser.ParseAsync(parsedFile.Body, cancellationToken);
        var excerptHtml = !string.IsNullOrEmpty(parsedFile.Excerpt) 
            ? await ParseExcerptAsync(parsedFile.Excerpt, cancellationToken)
            : string.Empty;

        // Create the page
        var page = new Page
        {
            FileName = fileName,
            MarkdownHeader = parsedFile.Header,
            Content = contentHtml,
            Date = date,
            Url = "/" + slug,
            Settings = frontMatterSettings,
            Published = PublishedStatus.Published // Default, will be overridden by front matter
        };

        // Set excerpt if provided in dedicated section
        if (!string.IsNullOrEmpty(excerptHtml))
        {
            page.ContentExcerpt = excerptHtml;
        }

        // Apply site defaults
        if (settings is SiteSettings siteSettings)
        {
            page.SetDefaults(siteSettings);
        }

        // Apply front matter settings
        page.SetFromFrontMatter(frontMatterSettings);

        return page;
    }

    private IPage ParsePageContent(string fileName, string rawContent, ISiteSettings settings)
    {
        return ParsePageContentAsync(fileName, rawContent, settings, CancellationToken.None)
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
