using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.ViewModels;

namespace Snow.Engine.Processing.Processors;

/// <summary>
/// Processes generic static files with no loop (default processor).
/// </summary>
public class StaticFileProcessor : BaseContentProcessor
{
    public StaticFileProcessor(ILogger<StaticFileProcessor> logger) : base(logger)
    {
    }

    /// <inheritdoc />
    public override string ProcessorName => "";

    /// <inheritdoc />
    protected override async Task ProcessImplementationAsync(IProcessingContext context, CancellationToken cancellationToken)
    {
        var generatedUrl = context.Settings.SiteUrl + "/" + DestinationName.Trim('/') + "/";

        // Create view model
        var viewModel = new TemplateViewModel
        {
            Title = context.StaticFile.Title ?? string.Empty,
            Settings = context.Settings,
            Posts = context.Posts,
            Pages = context.Pages,
            Drafts = context.Drafts,
            AllCategories = context.Categories,
            GeneratedUrl = generatedUrl,
            HeaderTitleChain = new[] { context.StaticFile.Title ?? string.Empty, context.Settings.BlogTitle }
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

        Logger.LogDebug("Generated static file: {OutputPath}", outputPath);
    }
}
