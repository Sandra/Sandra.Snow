using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;

namespace Snow.Engine.Processing;

/// <summary>
/// Abstract base class for content processors.
/// </summary>
public abstract class BaseContentProcessor : IContentProcessor
{
    protected readonly ILogger Logger;

    protected BaseContentProcessor(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public abstract string ProcessorName { get; }

    /// <inheritdoc />
    public virtual bool CanProcess(string processorName)
    {
        return string.Equals(processorName, ProcessorName, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task ProcessAsync(IProcessingContext context, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Processing {ProcessorName} for file {FileName}", ProcessorName, context.StaticFile.File);
        
        ParseDirectories(context);
        await ProcessImplementationAsync(context, cancellationToken);
        
        Logger.LogDebug("Completed processing {ProcessorName} for file {FileName}", ProcessorName, context.StaticFile.File);
    }

    /// <inheritdoc />
    public void Process(IProcessingContext context)
    {
        Logger.LogDebug("Processing {ProcessorName} for file {FileName}", ProcessorName, context.StaticFile.File);
        
        ParseDirectories(context);
        ProcessImplementation(context);
        
        Logger.LogDebug("Completed processing {ProcessorName} for file {FileName}", ProcessorName, context.StaticFile.File);
    }

    /// <summary>
    /// Async implementation of the processing logic.
    /// </summary>
    /// <param name="context">The processing context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected abstract Task ProcessImplementationAsync(IProcessingContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Synchronous implementation of the processing logic.
    /// </summary>
    /// <param name="context">The processing context.</param>
    protected virtual void ProcessImplementation(IProcessingContext context)
    {
        ProcessImplementationAsync(context, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the source file name.
    /// </summary>
    protected string SourceFile { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the destination directory path.
    /// </summary>
    protected string Destination { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the destination file name.
    /// </summary>
    protected string DestinationName { get; private set; } = string.Empty;

    /// <summary>
    /// Parses the directory information from the static file configuration.
    /// </summary>
    /// <param name="context">The processing context.</param>
    protected virtual void ParseDirectories(IProcessingContext context)
    {
        var source = context.StaticFile.File;

        var sourceFile = source;
        var destinationDirectory = Path.Combine(context.OutputDirectory, GetFileNameWithoutExtension(source));
        var destinationName = GetFileNameWithoutExtension(source);

        // Handle file renaming with " => " syntax
        if (source.Contains(" => "))
        {
            var directorySplit = source.Split(new[] { " => " }, StringSplitOptions.RemoveEmptyEntries);
            if (directorySplit.Length == 2)
            {
                sourceFile = directorySplit[0].Trim();
                destinationDirectory = Path.Combine(context.OutputDirectory, directorySplit[1].Trim());
                destinationName = directorySplit[1].Trim();
            }
        }

        SourceFile = sourceFile;
        Destination = destinationDirectory;
        DestinationName = destinationName;
    }

    /// <summary>
    /// Gets the file name without extension, handling edge cases.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The file name without extension.</returns>
    private static string GetFileNameWithoutExtension(string fileName)
    {
        var dotIndex = fileName.IndexOf('.');
        return dotIndex > 0 ? fileName.Substring(0, dotIndex) : fileName;
    }
}
