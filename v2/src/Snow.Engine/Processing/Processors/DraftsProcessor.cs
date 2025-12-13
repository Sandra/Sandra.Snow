using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.ViewModels;

namespace Snow.Engine.Processing.Processors;

/// <summary>
/// Processes the drafts listing page.
/// </summary>
public class DraftsProcessor : BaseContentProcessor
{
    public DraftsProcessor(ILogger<DraftsProcessor> logger) : base(logger)
    {
    }

    /// <inheritdoc />
    public override string ProcessorName => "drafts";

    /// <inheritdoc />
    protected override async Task ProcessImplementationAsync(IProcessingContext context, CancellationToken cancellationToken)
    {
        var generatedUrl = context.Settings.SiteUrl + "/" + DestinationName.Trim('/') + "/";

        // Create view model
        var viewModel = new TemplateViewModel
        {
            Title = "Drafts",
            Settings = context.Settings,
            Posts = context.Posts,
            Pages = context.Pages,
            Drafts = context.Drafts,
            AllCategories = context.Categories,
            PostsPaged = context.Drafts,
            GeneratedUrl = generatedUrl,
            HeaderTitleChain = new[] { "Drafts", context.Settings.BlogTitle }
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList()
        };

        // Render the template
        var html = await context.TemplateRenderer.RenderAsync(
            SourceFile,
            viewModel,
            cancellationToken: cancellationToken);

        // Create output folder
        if (!context.FileProvider.DirectoryExists(Destination))
        {
            context.FileProvider.CreateDirectory(Destination);
        }

        // Write the output file
        var outputPath = Path.Combine(Destination, "index.html");
        await context.FileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);

        Logger.LogInformation("Generated drafts page with {DraftCount} drafts: {OutputPath}", 
            context.Drafts.Count, outputPath);
    }
}
