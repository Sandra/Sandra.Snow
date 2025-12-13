using System;
using System.Threading;
using System.Threading.Tasks;
using Markdig;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;

namespace Snow.Engine.Markdown;

/// <summary>
/// High-performance Markdig-based markdown parser with dual pipelines.
/// </summary>
public sealed class MarkdigMarkdownParser : IMarkdownParser, IDisposable
{
    private readonly MarkdownPipeline _fullPipeline;
    private readonly MarkdownPipeline _excerptPipeline;
    private readonly ILogger<MarkdigMarkdownParser> _logger;
    private readonly MarkdownConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the MarkdigMarkdownParser.
    /// </summary>
    /// <param name="configuration">The markdown configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public MarkdigMarkdownParser(MarkdownConfiguration configuration, ILogger<MarkdigMarkdownParser> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate configuration
        var validationErrors = configuration.Validate();
        foreach (var error in validationErrors)
        {
            _logger.LogWarning("Markdown configuration warning: {Error}", error);
        }

        // Create pipelines once for thread-safe reuse
        try
        {
            _fullPipeline = MarkdownPipelineFactory.CreateFullPipeline(configuration, logger);
            _excerptPipeline = MarkdownPipelineFactory.CreateExcerptPipeline(configuration, logger);
            
            _logger.LogInformation("Markdig markdown parser initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Markdig pipelines");
            throw;
        }
    }

    /// <inheritdoc />
    public Task<string> ParseAsync(string markdown, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Parse(markdown));
    }

    /// <inheritdoc />
    public string Parse(string markdown)
    {
        return ParseInternal(markdown, useExcerptPipeline: false);
    }

    /// <summary>
    /// Parses markdown using the excerpt pipeline for lighter processing.
    /// </summary>
    /// <param name="markdown">The markdown content to parse.</param>
    /// <returns>The parsed HTML content.</returns>
    public string ParseExcerpt(string markdown)
    {
        return ParseInternal(markdown, useExcerptPipeline: true);
    }

    /// <summary>
    /// Parses markdown using the excerpt pipeline asynchronously.
    /// </summary>
    /// <param name="markdown">The markdown content to parse.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The parsed HTML content.</returns>
    public Task<string> ParseExcerptAsync(string markdown, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ParseExcerpt(markdown));
    }

    /// <summary>
    /// Gets the current markdown configuration.
    /// </summary>
    public MarkdownConfiguration Configuration => _configuration;

    /// <summary>
    /// Internal parsing method that selects the appropriate pipeline.
    /// </summary>
    /// <param name="markdown">The markdown content to parse.</param>
    /// <param name="useExcerptPipeline">Whether to use the lightweight excerpt pipeline.</param>
    /// <returns>The parsed HTML content.</returns>
    private string ParseInternal(string markdown, bool useExcerptPipeline)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return string.Empty;
        }

        try
        {
            var pipeline = useExcerptPipeline ? _excerptPipeline : _fullPipeline;
            var html = Markdig.Markdown.ToHtml(markdown, pipeline);
            
            // Log performance metrics periodically
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var pipelineType = useExcerptPipeline ? "excerpt" : "full";
                _logger.LogDebug("Parsed {CharCount} characters using {PipelineType} pipeline", 
                    markdown.Length, pipelineType);
            }

            return html;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse markdown content");
            
            // Return escaped content as fallback
            return System.Net.WebUtility.HtmlEncode(markdown);
        }
    }

    /// <summary>
    /// Creates a parser with default configuration.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <returns>A new MarkdigMarkdownParser with default settings.</returns>
    public static MarkdigMarkdownParser CreateDefault(ILogger<MarkdigMarkdownParser> logger)
    {
        return new MarkdigMarkdownParser(MarkdownConfiguration.Default(), logger);
    }

    /// <summary>
    /// Disposes the markdown pipelines.
    /// </summary>
    public void Dispose()
    {
        // MarkdownPipeline doesn't implement IDisposable in current Markdig version
        // This method is kept for future compatibility
    }
}
