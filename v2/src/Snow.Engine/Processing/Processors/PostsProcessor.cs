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
/// Processes the main posts listing page.
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
        // Create the output directory if it doesn't exist
        if (!context.FileProvider.DirectoryExists(Destination))
        {
            context.FileProvider.CreateDirectory(Destination);
        }

        // Create view model with posts data
        var viewModel = new TemplateViewModel
        {
            Settings = context.Settings,
            Posts = context.Posts,
            Pages = context.Pages,
            Drafts = context.Drafts,
            AllCategories = context.Categories
        };

        // Render the template with the posts data
        var html = await context.TemplateRenderer.RenderAsync(
            SourceFile,
            viewModel,
            cancellationToken: cancellationToken);

        // Write the output file
        var outputPath = Path.Combine(Destination, "index.html");
        await context.FileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);

        Logger.LogInformation("Generated posts listing: {OutputPath}", outputPath);
    }
}
