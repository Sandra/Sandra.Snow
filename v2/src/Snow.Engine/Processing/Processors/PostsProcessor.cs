using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;

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

        // Render the template with the posts data
        var html = await context.TemplateRenderer.RenderAsync(
            SourceFile,
            new PostsViewModel(context.Posts, context.Settings),
            cancellationToken: cancellationToken);

        // Write the output file
        var outputPath = Path.Combine(Destination, "index.html");
        await context.FileProvider.WriteAllTextAsync(outputPath, html, cancellationToken);

        Logger.LogInformation("Generated posts listing: {OutputPath}", outputPath);
    }
}

/// <summary>
/// View model for the posts listing template.
/// </summary>
public class PostsViewModel
{
    public PostsViewModel(IReadOnlyList<IPost> posts, ISiteSettings settings)
    {
        Posts = posts;
        Settings = settings;
    }

    public IReadOnlyList<IPost> Posts { get; }
    public ISiteSettings Settings { get; }
}
