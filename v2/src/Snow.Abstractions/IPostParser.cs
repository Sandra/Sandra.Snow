using System.Threading;
using System.Threading.Tasks;

namespace Snow.Abstractions;

/// <summary>
/// Interface for parsing blog posts from files.
/// </summary>
public interface IPostParser
{
    /// <summary>
    /// Parses a post from a file.
    /// </summary>
    /// <param name="fileInfo">Information about the file to parse.</param>
    /// <param name="settings">Site settings for default values.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The parsed post or null if the file cannot be parsed.</returns>
    Task<IPost?> ParsePostAsync(IFileInfo fileInfo, ISiteSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses a post from a file synchronously.
    /// </summary>
    /// <param name="fileInfo">Information about the file to parse.</param>
    /// <param name="settings">Site settings for default values.</param>
    /// <returns>The parsed post or null if the file cannot be parsed.</returns>
    IPost? ParsePost(IFileInfo fileInfo, ISiteSettings settings);

    /// <summary>
    /// Determines if a file can be parsed as a post based on its name.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>True if the file can be parsed as a post, false otherwise.</returns>
    bool CanParsePost(string fileName);
}
