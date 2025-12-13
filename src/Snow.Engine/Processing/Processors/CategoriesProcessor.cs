using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.ViewModels;

namespace Snow.Engine.Processing.Processors;

/// <summary>
/// Processes category listing pages - creates a page for each category.
/// </summary>
public class CategoriesProcessor : BaseContentProcessor
{
    public CategoriesProcessor(ILogger<CategoriesProcessor> logger) : base(logger)
    {
    }

    /// <inheritdoc />
    public override string ProcessorName => "categories";

    /// <inheritdoc />
    protected override async Task ProcessImplementationAsync(IProcessingContext context, CancellationToken cancellationToken)
    {
        foreach (var category in context.Categories.Where(c => c.PostCount > 0))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get posts in this category that are published (v1 compatible - no date filter)
            var postsInCategory = context.Posts
                .Where(p => p.Published == PublishedStatus.Published && 
                           p.Categories.Contains(category.Name, StringComparer.OrdinalIgnoreCase))
                .OrderByDescending(p => p.Date)
                .ToList();

            var generatedUrl = context.Settings.SiteUrl + "/category/" + category.Slug + "/";

            // Create view model
            var viewModel = new TemplateViewModel
            {
                Title = category.Name,
                Settings = context.Settings,
                Posts = context.Posts,
                Pages = context.Pages,
                Drafts = context.Drafts,
                AllCategories = context.Categories,
                Category = category,
                PostsInCategory = postsInCategory,
                GeneratedUrl = generatedUrl,
                HeaderTitleChain = new[] { category.Name, "Categories", context.Settings.BlogTitle }
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList()
            };

            // Render the template
            var html = await context.TemplateRenderer.RenderAsync(
                SourceFile,
                viewModel,
                cancellationToken: cancellationToken);

            // Create output folder
            var outputFolder = Path.Combine(context.OutputDirectory, "category", category.Slug);
            if (!context.FileProvider.DirectoryExists(outputFolder))
            {
                context.FileProvider.CreateDirectory(outputFolder);
            }

            // Write the output file
            var outputPath = Path.Combine(outputFolder, "index.html");
            await context.FileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);

            Logger.LogDebug("Generated category page: {Category} -> {OutputPath}", category.Name, outputPath);
        }

        Logger.LogInformation("Generated {CategoryCount} category pages", 
            context.Categories.Count(c => c.PostCount > 0));
    }
}
