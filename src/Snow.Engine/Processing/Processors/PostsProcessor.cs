using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.ViewModels;

namespace Snow.Engine.Processing.Processors;

/// <summary>
/// Processes the main posts listing page with pagination support.
/// </summary>
public class PostsProcessor : BaseContentProcessor
{
    public PostsProcessor(ILogger<PostsProcessor> logger) : base(logger)
    {
    }

    /// <inheritdoc />
    public override string ProcessorName => "posts";

    /// <inheritdoc />
    protected override async Task ProcessImplementationAsync(IProcessingContext context, CancellationToken cancellationToken)
    {
        // Filter posts that should be shown (published only, v1 compatible - no date filter)
        var filteredPosts = context.Posts
            .Where(p => p.Published == PublishedStatus.Published)
            .OrderByDescending(p => p.Date)
            .ToList();

        var pageSize = context.Settings.PageSize;
        var skip = 0;
        var pageNumber = 1;
        var totalPages = (int)Math.Ceiling((double)filteredPosts.Count / pageSize);

        var currentPagePosts = filteredPosts.Skip(skip).Take(pageSize).ToList();

        while (currentPagePosts.Any())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var folder = pageNumber == 1 ? "" : "page" + pageNumber;
            var generatedUrl = (context.Settings.SiteUrl + "/" + folder).TrimEnd('/') + "/";

            // Create view model with pagination data
            var viewModel = new TemplateViewModel
            {
                Settings = context.Settings,
                Posts = context.Posts,
                Pages = context.Pages,
                Drafts = context.Drafts,
                AllCategories = context.Categories,
                PostsPaged = currentPagePosts,
                PageNumber = pageNumber,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1 && totalPages > 1,
                GeneratedUrl = generatedUrl
            };

            // Render the template
            var html = await context.TemplateRenderer.RenderAsync(
                SourceFile,
                viewModel,
                cancellationToken: cancellationToken);

            // Create output folder
            var outputFolder = string.IsNullOrEmpty(folder)
                ? context.OutputDirectory
                : Path.Combine(context.OutputDirectory, folder);

            if (!context.FileProvider.DirectoryExists(outputFolder))
            {
                context.FileProvider.CreateDirectory(outputFolder);
            }

            // Write the output file
            var outputPath = Path.Combine(outputFolder, "index.html");
            await context.FileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);

            Logger.LogDebug("Generated page {PageNumber}/{TotalPages}: {OutputPath}", 
                pageNumber, totalPages, outputPath);

            // Move to next page
            skip += pageSize;
            pageNumber++;
            currentPagePosts = filteredPosts.Skip(skip).Take(pageSize).ToList();
        }

        Logger.LogInformation("Generated {TotalPages} index page(s) for posts", totalPages == 0 ? 1 : totalPages);
    }
}
