using System;
using System.Linq;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Microsoft.Extensions.Logging;

namespace Snow.Engine.Markdown;

/// <summary>
/// Factory for creating Markdig pipelines based on configuration.
/// </summary>
public static class MarkdownPipelineFactory
{
    /// <summary>
    /// Creates a full-featured pipeline for posts and pages.
    /// </summary>
    /// <param name="config">The markdown configuration.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <returns>A configured Markdig pipeline.</returns>
    public static MarkdownPipeline CreateFullPipeline(MarkdownConfiguration config, ILogger? logger = null)
    {
        logger?.LogDebug("Creating full Markdig pipeline with {ExtensionCount} extensions", config.Extensions.Count);

        var builder = new MarkdownPipelineBuilder();

        // Core extensions based on configuration
        if (config.EnableAdvancedExtensions)
        {
            builder.UseAdvancedExtensions();
            logger?.LogDebug("Enabled advanced extensions");
        }

        // Configure specific extensions
        ConfigureExtensions(builder, config, logger);

        // Configure syntax highlighting
        ConfigureSyntaxHighlighting(builder, config, logger);

        // Configure heading IDs
        ConfigureHeadingIds(builder, config, logger);

        // Configure HTML sanitization
        if (config.SanitizeHtml)
        {
            builder.DisableHtml();
            logger?.LogDebug("Disabled raw HTML for security");
        }

        // Configure line breaks
        if (config.HardLineBreaks)
        {
            builder.UseSoftlineBreakAsHardlineBreak();
            logger?.LogDebug("Enabled hard line breaks");
        }

        var pipeline = builder.Build();
        
        logger?.LogDebug("Full Markdig pipeline created successfully");
        return pipeline;
    }

    /// <summary>
    /// Creates a lightweight pipeline for excerpts and previews.
    /// </summary>
    /// <param name="config">The markdown configuration.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <returns>A lightweight Markdig pipeline.</returns>
    public static MarkdownPipeline CreateExcerptPipeline(MarkdownConfiguration config, ILogger? logger = null)
    {
        logger?.LogDebug("Creating excerpt Markdig pipeline");

        var builder = new MarkdownPipelineBuilder();

        // Essential extensions only for performance
        var essentialExtensions = config.Extensions.Where(ext => IsEssentialForExcerpts(ext)).ToList();
        
        foreach (var extension in essentialExtensions)
        {
            AddExtension(builder, extension, logger);
        }

        // Configure heading IDs (needed for navigation)
        ConfigureHeadingIds(builder, config, logger);

        // Always sanitize HTML in excerpts
        builder.DisableHtml();

        // Configure line breaks
        if (config.HardLineBreaks)
        {
            builder.UseSoftlineBreakAsHardlineBreak();
        }

        var pipeline = builder.Build();
        
        logger?.LogDebug("Excerpt Markdig pipeline created with {ExtensionCount} extensions", essentialExtensions.Count);
        return pipeline;
    }

    /// <summary>
    /// Configures extensions based on the configuration.
    /// </summary>
    private static void ConfigureExtensions(MarkdownPipelineBuilder builder, MarkdownConfiguration config, ILogger? logger)
    {
        foreach (var extension in config.Extensions)
        {
            AddExtension(builder, extension, logger);
        }
    }

    /// <summary>
    /// Adds a specific extension to the pipeline builder.
    /// </summary>
    private static void AddExtension(MarkdownPipelineBuilder builder, string extension, ILogger? logger)
    {
        try
        {
            switch (extension.ToLowerInvariant())
            {
                case "emoji":
                    builder.UseEmojiAndSmiley();
                    break;
                case "smartypants":
                    builder.UseSmartyPants();
                    break;
                case "tables":
                    builder.UsePipeTables();
                    builder.UseGridTables();
                    break;
                case "tasklists":
                    builder.UseTaskLists();
                    break;
                case "autolinks":
                    builder.UseAutoLinks();
                    break;
                case "math":
                    builder.UseMathematics();
                    break;
                case "footnotes":
                    builder.UseFootnotes();
                    break;
                case "abbreviations":
                    builder.UseAbbreviations();
                    break;
                case "definitionlists":
                    builder.UseDefinitionLists();
                    break;
                case "figures":
                    builder.UseFigures();
                    break;
                case "customcontainers":
                    builder.UseCustomContainers();
                    break;
                case "medialinks":
                    builder.UseMediaLinks();
                    break;
                case "genericattributes":
                    builder.UseGenericAttributes();
                    break;
                default:
                    logger?.LogWarning("Unknown markdown extension: {Extension}", extension);
                    break;
            }
            
            logger?.LogDebug("Added markdown extension: {Extension}", extension);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to add markdown extension: {Extension}", extension);
        }
    }

    /// <summary>
    /// Configures syntax highlighting if enabled.
    /// </summary>
    private static void ConfigureSyntaxHighlighting(MarkdownPipelineBuilder builder, MarkdownConfiguration config, ILogger? logger)
    {
        if (config.SyntaxHighlighting.Enabled)
        {
            // For static site generators, we use client-side highlighting (Prism.js, highlight.js)
            // to keep server-side rendering fast. This ensures code blocks get proper CSS classes.
            // The actual highlighting will be done by client-side JavaScript.
            logger?.LogDebug("Syntax highlighting configured for client-side rendering with theme: {Theme}", 
                config.SyntaxHighlighting.Theme);
        }
    }

    /// <summary>
    /// Configures heading ID generation.
    /// </summary>
    private static void ConfigureHeadingIds(MarkdownPipelineBuilder builder, MarkdownConfiguration config, ILogger? logger)
    {
        switch (config.HeadingIds)
        {
            case HeadingIdStyle.GitHub:
                builder.UseAutoIdentifiers(AutoIdentifierOptions.GitHub);
                logger?.LogDebug("Configured GitHub-style heading IDs");
                break;
            case HeadingIdStyle.Legacy:
                builder.UseAutoIdentifiers(AutoIdentifierOptions.Default);
                logger?.LogDebug("Configured legacy-style heading IDs");
                break;
            case HeadingIdStyle.None:
                // Don't add auto identifiers
                logger?.LogDebug("Heading ID generation disabled");
                break;
        }
    }

    /// <summary>
    /// Determines if an extension is essential for excerpt rendering.
    /// </summary>
    private static bool IsEssentialForExcerpts(string extension)
    {
        var essentialExtensions = new[]
        {
            "emoji", "smartypants", "autolinks"
        };

        return essentialExtensions.Contains(extension.ToLowerInvariant());
    }
}
