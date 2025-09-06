using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;

namespace Snow.Engine.IO;

/// <summary>
/// Provides safe directory operations for the file provider.
/// </summary>
public static class SafeDirectoryOperations
{
    /// <summary>
    /// Safely cleans a directory using the file provider with protection against dangerous paths.
    /// </summary>
    /// <param name="fileProvider">The file provider to use.</param>
    /// <param name="targetDirectory">The directory to clean.</param>
    /// <param name="currentDirectory">The current working directory for safety checks.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public static async Task CleanDirectoryAsync(
        IFileProvider fileProvider,
        string targetDirectory,
        string currentDirectory,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(targetDirectory))
            throw new ArgumentException("Target directory cannot be null or empty", nameof(targetDirectory));

        if (string.IsNullOrEmpty(currentDirectory))
            throw new ArgumentException("Current directory cannot be null or empty", nameof(currentDirectory));

        // Safety checks to prevent dangerous deletions
        var fullTargetPath = Path.GetFullPath(targetDirectory);
        var fullCurrentPath = Path.GetFullPath(currentDirectory);

        // Prevent deleting root directories or system directories
        var dangerousPaths = new[]
        {
            "/", "C:\\", "D:\\", // Root directories
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.Windows)
        }.Where(p => !string.IsNullOrEmpty(p)).Select(Path.GetFullPath);

        if (dangerousPaths.Any(dangerous => 
            string.Equals(fullTargetPath, dangerous, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Refusing to clean dangerous directory: {fullTargetPath}");
        }

        // Ensure target is within current directory or a subdirectory
        if (!fullTargetPath.StartsWith(fullCurrentPath, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Target directory {TargetDir} is outside current directory {CurrentDir}, skipping clean", 
                fullTargetPath, fullCurrentPath);
            return;
        }

        logger.LogDebug("Safely cleaning directory: {TargetDirectory}", fullTargetPath);

        try
        {
            if (!fileProvider.DirectoryExists(fullTargetPath))
            {
                logger.LogDebug("Creating output directory: {TargetDirectory}", fullTargetPath);
                fileProvider.CreateDirectory(fullTargetPath);
                return;
            }

            await DeleteDirectoryContentsAsync(fileProvider, fullTargetPath, logger, cancellationToken);
            logger.LogDebug("Successfully cleaned directory: {TargetDirectory}", fullTargetPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clean directory: {TargetDirectory}", fullTargetPath);
            throw;
        }
    }

    /// <summary>
    /// Recursively deletes directory contents using the file provider.
    /// </summary>
    private static async Task DeleteDirectoryContentsAsync(
        IFileProvider fileProvider,
        string directoryPath,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Delete all files first
        var files = fileProvider.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories).ToList();
        
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                File.Delete(file.FullPath); // Use System.IO for actual deletion
                logger.LogTrace("Deleted file: {FilePath}", file.FullPath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete file: {FilePath}", file.FullPath);
            }
        }

        // Then delete subdirectories (bottom-up)
        try
        {
            var subdirectories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories)
                                         .OrderByDescending(d => d.Length) // Deepest first
                                         .ToList();

            foreach (var subdir in subdirectories)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (Directory.Exists(subdir))
                    {
                        Directory.Delete(subdir, false); // Don't force recursive since we cleaned files already
                        logger.LogTrace("Deleted directory: {DirectoryPath}", subdir);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to delete directory: {DirectoryPath}", subdir);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to enumerate subdirectories in: {DirectoryPath}", directoryPath);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Safely copies a directory recursively using the file provider.
    /// </summary>
    /// <param name="fileProvider">The file provider to use.</param>
    /// <param name="sourcePath">The source directory path.</param>
    /// <param name="destinationPath">The destination directory path.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public static async Task CopyDirectoryAsync(
        IFileProvider fileProvider,
        string sourcePath,
        string destinationPath,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!fileProvider.DirectoryExists(sourcePath))
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");
        }

        logger.LogDebug("Copying directory: {Source} -> {Destination}", sourcePath, destinationPath);

        if (!fileProvider.DirectoryExists(destinationPath))
        {
            fileProvider.CreateDirectory(destinationPath);
        }

        var files = fileProvider.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories);
        var fileCount = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(sourcePath, file.FullPath);
            var destFilePath = Path.Combine(destinationPath, relativePath);
            var destFileDir = Path.GetDirectoryName(destFilePath);

            if (!string.IsNullOrEmpty(destFileDir) && !fileProvider.DirectoryExists(destFileDir))
            {
                fileProvider.CreateDirectory(destFileDir);
            }

            fileProvider.CopyFile(file.FullPath, destFilePath, overwrite: true);
            fileCount++;

            logger.LogTrace("Copied file: {Source} -> {Destination}", file.FullPath, destFilePath);
        }

        logger.LogDebug("Copied {FileCount} files from {Source} to {Destination}", 
            fileCount, sourcePath, destinationPath);

        await Task.CompletedTask;
    }
}
