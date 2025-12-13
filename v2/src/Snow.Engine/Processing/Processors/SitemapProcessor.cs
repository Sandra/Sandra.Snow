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
/// Processes sitemap.xml generation.
/// </summary>
public class SitemapProcessor : BaseContentProcessor
{
    public SitemapProcessor(ILogger<SitemapProcessor> logger) : base(logger)
    {
    }

    /// <inheritdoc />
    public override string ProcessorName => "sitemap";

    /// <inheritdoc />
    protected override async Task ProcessImplementationAsync(IProcessingContext context, CancellationToken cancellationToken)
    {
        var sitemapContent = GenerateSitemap(context);

        // Create output directory if needed
        if (!context.FileProvider.DirectoryExists(context.OutputDirectory))
        {
            context.FileProvider.CreateDirectory(context.OutputDirectory);
        }

        // Write the sitemap file
        var outputPath = Path.Combine(context.OutputDirectory, SourceFile);
        await context.FileProvider.WriteAllTextAsync(outputPath, sitemapContent, cancellationToken);

        Logger.LogInformation("Generated sitemap: {OutputPath}", outputPath);
    }

    private static string GenerateSitemap(IProcessingContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Add homepage
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{EscapeXml(context.Settings.SiteUrl)}/</loc>");
        sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
        sb.AppendLine("    <changefreq>daily</changefreq>");
        sb.AppendLine("    <priority>1.0</priority>");
        sb.AppendLine("  </url>");

        // Add all published posts
        foreach (var post in context.Posts.Where(p => p.Published == PublishedStatus.Published))
        {
            var postUrl = context.Settings.SiteUrl.TrimEnd('/') + post.Url;
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{EscapeXml(postUrl)}</loc>");
            sb.AppendLine($"    <lastmod>{post.Date:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("    <changefreq>monthly</changefreq>");
            sb.AppendLine("    <priority>0.8</priority>");
            sb.AppendLine("  </url>");
        }

        // Add all published pages
        foreach (var page in context.Pages.Where(p => p.Published == PublishedStatus.Published))
        {
            var pageUrl = context.Settings.SiteUrl.TrimEnd('/') + page.Url;
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{EscapeXml(pageUrl)}</loc>");
            sb.AppendLine($"    <lastmod>{page.Date:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("    <changefreq>monthly</changefreq>");
            sb.AppendLine("    <priority>0.6</priority>");
            sb.AppendLine("  </url>");
        }

        // Add category pages
        foreach (var category in context.Categories.Where(c => c.PostCount > 0))
        {
            var categoryUrl = context.Settings.SiteUrl.TrimEnd('/') + "/category/" + category.Slug + "/";
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{EscapeXml(categoryUrl)}</loc>");
            sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("    <changefreq>weekly</changefreq>");
            sb.AppendLine("    <priority>0.5</priority>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");

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
