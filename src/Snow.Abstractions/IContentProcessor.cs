using System.Threading;
using System.Threading.Tasks;

namespace Snow.Abstractions;

/// <summary>
/// Interface for processing specific types of content during site generation.
/// </summary>
public interface IContentProcessor
{
    /// <summary>
    /// The name that identifies this processor in configuration.
    /// </summary>
    string ProcessorName { get; }

    /// <summary>
    /// Determines if this processor can handle the specified processor name.
    /// </summary>
    /// <param name="processorName">The processor name to check.</param>
    /// <returns>True if this processor can handle the name, false otherwise.</returns>
    bool CanProcess(string processorName);

    /// <summary>
    /// Processes content using the provided data context.
    /// </summary>
    /// <param name="context">The processing context containing site data and configuration.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task ProcessAsync(IProcessingContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes content synchronously using the provided data context.
    /// </summary>
    /// <param name="context">The processing context containing site data and configuration.</param>
    void Process(IProcessingContext context);
}
