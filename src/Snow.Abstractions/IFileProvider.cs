using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Snow.Abstractions;

/// <summary>
/// Abstraction for file system operations to enable testing.
/// </summary>
public interface IFileProvider
{
    /// <summary>
    /// Gets information about a file at the specified path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>File information or null if file doesn't exist.</returns>
    IFileInfo? GetFileInfo(string path);

    /// <summary>
    /// Gets information about a directory at the specified path.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>Directory information or null if directory doesn't exist.</returns>
    IDirectoryInfo? GetDirectoryInfo(string path);

    /// <summary>
    /// Enumerates files in the specified directory with optional search pattern and options.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <param name="searchPattern">Search pattern for file names.</param>
    /// <param name="searchOption">Search options (current directory or all directories).</param>
    /// <returns>Collection of file information.</returns>
    IEnumerable<IFileInfo> EnumerateFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

    /// <summary>
    /// Reads all text from the specified file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file content as text.</returns>
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes all text to the specified file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="content">The content to write.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    bool FileExists(string path);

    /// <summary>
    /// Checks if a directory exists at the specified path.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>True if the directory exists, false otherwise.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Creates a directory at the specified path.
    /// </summary>
    /// <param name="path">The directory path.</param>
    void CreateDirectory(string path);

    /// <summary>
    /// Copies a file from source to destination.
    /// </summary>
    /// <param name="sourcePath">Source file path.</param>
    /// <param name="destinationPath">Destination file path.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    void CopyFile(string sourcePath, string destinationPath, bool overwrite = false);
}
