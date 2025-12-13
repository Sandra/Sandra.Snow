using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Snow.Engine.IO;

/// <summary>
/// Provides safe path operations for cross-platform compatibility.
/// </summary>
public static class SafePathOperations
{
    private static readonly Regex InvalidPathCharsRegex = new(
        $"[{Regex.Escape(string.Concat(Path.GetInvalidPathChars()))}]", 
        RegexOptions.Compiled);

    /// <summary>
    /// Safely combines paths, handling leading slashes and invalid characters.
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <param name="relativePath">The relative path to combine.</param>
    /// <returns>The safely combined path.</returns>
    public static string SafeCombine(string basePath, string relativePath)
    {
        if (string.IsNullOrEmpty(basePath))
            throw new ArgumentException("Base path cannot be null or empty", nameof(basePath));

        if (string.IsNullOrEmpty(relativePath))
            return basePath;

        // Remove leading slashes from relative path to prevent absolute path issues
        var cleanRelativePath = relativePath.TrimStart('/', '\\');
        
        // Validate path doesn't contain invalid characters
        if (InvalidPathCharsRegex.IsMatch(cleanRelativePath))
        {
            throw new ArgumentException($"Relative path contains invalid characters: {relativePath}");
        }

        // Use Path.Combine for proper cross-platform path handling
        return Path.Combine(basePath, cleanRelativePath);
    }

    /// <summary>
    /// Safely creates an output file path for content with URL-based structure.
    /// </summary>
    /// <param name="outputDirectory">The base output directory.</param>
    /// <param name="contentUrl">The content URL (may have leading slash).</param>
    /// <param name="fileName">The file name (default: index.html).</param>
    /// <returns>The safe output file path.</returns>
    public static string CreateOutputPath(string outputDirectory, string contentUrl, string fileName = "index.html")
    {
        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory cannot be null or empty", nameof(outputDirectory));

        if (string.IsNullOrEmpty(contentUrl))
            throw new ArgumentException("Content URL cannot be null or empty", nameof(contentUrl));

        // Clean the URL path
        var cleanUrl = contentUrl.TrimStart('/', '\\').TrimEnd('/', '\\');
        
        // Handle empty URL (root content)
        if (string.IsNullOrEmpty(cleanUrl))
        {
            return Path.Combine(outputDirectory, fileName);
        }

        // Create directory structure based on URL
        var urlDirectory = SafeCombine(outputDirectory, cleanUrl);
        
        // Handle file URLs vs directory URLs
        if (IsFileUrl(contentUrl))
        {
            return urlDirectory;
        }
        else
        {
            return Path.Combine(urlDirectory, fileName);
        }
    }

    /// <summary>
    /// Determines if a URL represents a file (has extension) or directory.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>True if the URL represents a file, false for directory.</returns>
    public static bool IsFileUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        var lastSegment = url.Contains('/') || url.Contains('\\') 
            ? Path.GetFileName(url) 
            : url;

        return lastSegment.Contains('.');
    }

    /// <summary>
    /// Validates that a path is safe for use as an output directory.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="currentDirectory">The current working directory.</param>
    /// <returns>True if the path is safe to use.</returns>
    public static bool IsOutputPathSafe(string path, string currentDirectory)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(currentDirectory))
            return false;

        try
        {
            var fullPath = Path.GetFullPath(path);
            var fullCurrentPath = Path.GetFullPath(currentDirectory);

            // Must be within or equal to current directory
            return fullPath.StartsWith(fullCurrentPath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Normalizes a path for consistent cross-platform usage.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // Replace all slashes with the current platform's directory separator
        return path.Replace('/', Path.DirectorySeparatorChar)
                  .Replace('\\', Path.DirectorySeparatorChar);
    }
}
