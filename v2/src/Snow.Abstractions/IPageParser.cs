using System.Threading;
using System.Threading.Tasks;

namespace Snow.Abstractions;

/// <summary>
/// Interface for parsing static pages from files.
/// </summary>
public interface IPageParser
{
    /// <summary>
    /// Parses a page from a file.
    /// </summary>
    /// <param name="fileInfo">Information about the file to parse.</param>
    /// <param name="settings">Site settings for default values.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The parsed page or null if the file cannot be parsed.</returns>
    Task<IPage?> ParsePageAsync(IFileInfo fileInfo, ISiteSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses a page from a file synchronously.
    /// </summary>
    /// <param name="fileInfo">Information about the file to parse.</param>
    /// <param name="settings">Site settings for default values.</param>
    /// <returns>The parsed page or null if the file cannot be parsed.</returns>
    IPage? ParsePage(IFileInfo fileInfo, ISiteSettings settings);

    /// <summary>
    /// Determines if a file can be parsed as a page based on its name.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>True if the file can be parsed as a page, false otherwise.</returns>
    bool CanParsePage(string fileName);
}
