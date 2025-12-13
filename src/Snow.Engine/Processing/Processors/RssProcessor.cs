using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;

namespace Snow.Engine.Processing.Processors;

/// <summary>
/// Processes RSS feed generation.
/// </summary>
public class RssProcessor : BaseContentProcessor
{
    public RssProcessor(ILogger<RssProcessor> logger) : base(logger)
    {
    }

    /// <inheritdoc />
    public override string ProcessorName => "rss";

    /// <inheritdoc />
    protected override async Task ProcessImplementationAsync(IProcessingContext context, CancellationToken cancellationToken)
    {
        // Get posts for the feed (published and in the past, limited by FeedSize)
        var postsForFeed = context.Posts
            .Where(p => p.Published == PublishedStatus.Published)
            .OrderByDescending(p => p.Date)
            .Take(context.Settings.FeedSize)
            .ToList();

        var rssContent = GenerateRss(postsForFeed, context.Settings);

        // Create output directory if needed
        if (!context.FileProvider.DirectoryExists(context.OutputDirectory))
        {
            context.FileProvider.CreateDirectory(context.OutputDirectory);
        }

        // Write the RSS file
        var outputPath = Path.Combine(context.OutputDirectory, SourceFile);
        await context.FileProvider.WriteAllTextAsync(outputPath, rssContent, cancellationToken);

        Logger.LogInformation("Generated RSS feed with {PostCount} posts: {OutputPath}", 
            postsForFeed.Count, outputPath);
    }

    private static string GenerateRss(System.Collections.Generic.IReadOnlyList<IPost> posts, ISiteSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\">");
        sb.AppendLine("  <channel>");
        sb.AppendLine($"    <title>{EscapeXml(settings.BlogTitle)}</title>");
        sb.AppendLine($"    <link>{EscapeXml(settings.SiteUrl)}</link>");
        sb.AppendLine($"    <description>{EscapeXml(settings.BlogTitle)}</description>");
        sb.AppendLine($"    <atom:link href=\"{EscapeXml(settings.SiteUrl)}/feed.xml\" rel=\"self\" type=\"application/rss+xml\" />");
        sb.AppendLine($"    <lastBuildDate>{DateTime.UtcNow:R}</lastBuildDate>");

        foreach (var post in posts)
        {
            var postUrl = settings.SiteUrl.TrimEnd('/') + post.Url;
            sb.AppendLine("    <item>");
            sb.AppendLine($"      <title>{EscapeXml(post.Title)}</title>");
            sb.AppendLine($"      <link>{EscapeXml(postUrl)}</link>");
            sb.AppendLine($"      <guid isPermaLink=\"true\">{EscapeXml(postUrl)}</guid>");
            sb.AppendLine($"      <pubDate>{post.Date:R}</pubDate>");
            sb.AppendLine($"      <author>{EscapeXml(post.Email)} ({EscapeXml(post.Author)})</author>");
            
            if (!string.IsNullOrEmpty(post.ContentExcerpt))
            {
                sb.AppendLine($"      <description><![CDATA[{post.ContentExcerpt}]]></description>");
            }
            else
            {
                sb.AppendLine($"      <description><![CDATA[{post.Content}]]></description>");
            }

            foreach (var category in post.Categories)
            {
                sb.AppendLine($"      <category>{EscapeXml(category)}</category>");
            }

            sb.AppendLine("    </item>");
        }

        sb.AppendLine("  </channel>");
        sb.AppendLine("</rss>");

        return sb.ToString();
    }

    private static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
