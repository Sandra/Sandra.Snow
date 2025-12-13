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
/// Processes Atom feed generation.
/// </summary>
public class AtomProcessor : BaseContentProcessor
{
    public AtomProcessor(ILogger<AtomProcessor> logger) : base(logger)
    {
    }

    /// <inheritdoc />
    public override string ProcessorName => "atom";

    /// <inheritdoc />
    protected override async Task ProcessImplementationAsync(IProcessingContext context, CancellationToken cancellationToken)
    {
        // Get posts for the feed (published and in the past, limited by FeedSize)
        var postsForFeed = context.Posts
            .Where(p => p.Published == PublishedStatus.Published)
            .OrderByDescending(p => p.Date)
            .Take(context.Settings.FeedSize)
            .ToList();

        var atomContent = GenerateAtom(postsForFeed, context.Settings);

        // Create output directory if needed
        if (!context.FileProvider.DirectoryExists(context.OutputDirectory))
        {
            context.FileProvider.CreateDirectory(context.OutputDirectory);
        }

        // Write the Atom file
        var outputPath = Path.Combine(context.OutputDirectory, SourceFile);
        await context.FileProvider.WriteAllTextAsync(outputPath, atomContent, cancellationToken);

        Logger.LogInformation("Generated Atom feed with {PostCount} posts: {OutputPath}", 
            postsForFeed.Count, outputPath);
    }

    private static string GenerateAtom(System.Collections.Generic.IReadOnlyList<IPost> posts, ISiteSettings settings)
    {
        var sb = new StringBuilder();
        var updated = posts.FirstOrDefault()?.Date ?? DateTime.UtcNow;

        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<feed xmlns=\"http://www.w3.org/2005/Atom\">");
        sb.AppendLine($"  <title>{EscapeXml(settings.BlogTitle)}</title>");
        sb.AppendLine($"  <link href=\"{EscapeXml(settings.SiteUrl)}\" />");
        sb.AppendLine($"  <link href=\"{EscapeXml(settings.SiteUrl)}/feed.xml\" rel=\"self\" />");
        sb.AppendLine($"  <updated>{updated:yyyy-MM-ddTHH:mm:ssZ}</updated>");
        sb.AppendLine($"  <id>{EscapeXml(settings.SiteUrl)}/</id>");
        sb.AppendLine("  <author>");
        sb.AppendLine($"    <name>{EscapeXml(settings.Author)}</name>");
        if (!string.IsNullOrEmpty(settings.Email))
        {
            sb.AppendLine($"    <email>{EscapeXml(settings.Email)}</email>");
        }
        sb.AppendLine("  </author>");

        foreach (var post in posts)
        {
            var postUrl = settings.SiteUrl.TrimEnd('/') + post.Url;
            sb.AppendLine("  <entry>");
            sb.AppendLine($"    <title>{EscapeXml(post.Title)}</title>");
            sb.AppendLine($"    <link href=\"{EscapeXml(postUrl)}\" />");
            sb.AppendLine($"    <id>{EscapeXml(postUrl)}</id>");
            sb.AppendLine($"    <updated>{post.Date:yyyy-MM-ddTHH:mm:ssZ}</updated>");
            sb.AppendLine($"    <published>{post.Date:yyyy-MM-ddTHH:mm:ssZ}</published>");
            sb.AppendLine("    <author>");
            sb.AppendLine($"      <name>{EscapeXml(post.Author)}</name>");
            sb.AppendLine("    </author>");
            
            if (!string.IsNullOrEmpty(post.ContentExcerpt))
            {
                sb.AppendLine($"    <summary type=\"html\"><![CDATA[{post.ContentExcerpt}]]></summary>");
            }

            sb.AppendLine($"    <content type=\"html\"><![CDATA[{post.Content}]]></content>");

            foreach (var category in post.Categories)
            {
                sb.AppendLine($"    <category term=\"{EscapeXml(category)}\" />");
            }

            sb.AppendLine("  </entry>");
        }

        sb.AppendLine("</feed>");

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
