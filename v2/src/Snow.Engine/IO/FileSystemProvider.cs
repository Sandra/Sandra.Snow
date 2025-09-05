using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Snow.Abstractions;

namespace Snow.Engine.IO;

/// <summary>
/// File system implementation of IFileProvider.
/// </summary>
public class FileSystemProvider : IFileProvider
{
    /// <inheritdoc />
    public IFileInfo? GetFileInfo(string path)
    {
        var fileInfo = new FileInfo(path);
        return fileInfo.Exists ? new FileInfoWrapper(fileInfo) : null;
    }

    /// <inheritdoc />
    public IDirectoryInfo? GetDirectoryInfo(string path)
    {
        var dirInfo = new DirectoryInfo(path);
        return dirInfo.Exists ? new DirectoryInfoWrapper(dirInfo) : null;
    }

    /// <inheritdoc />
    public IEnumerable<IFileInfo> EnumerateFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!Directory.Exists(path))
            return Enumerable.Empty<IFileInfo>();

        try
        {
            return Directory.EnumerateFiles(path, searchPattern, searchOption)
                           .Select(f => new FileInfoWrapper(new FileInfo(f)));
        }
        catch (DirectoryNotFoundException)
        {
            return Enumerable.Empty<IFileInfo>();
        }
        catch (UnauthorizedAccessException)
        {
            return Enumerable.Empty<IFileInfo>();
        }
    }

    /// <inheritdoc />
    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    /// <inheritdoc />
    public async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, content, cancellationToken);
    }

    /// <inheritdoc />
    public bool FileExists(string path) => File.Exists(path);

    /// <inheritdoc />
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <inheritdoc />
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    /// <inheritdoc />
    public void CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        // Ensure destination directory exists
        var destinationDir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        File.Copy(sourcePath, destinationPath, overwrite);
    }
}

/// <summary>
/// Wrapper for System.IO.FileInfo to implement IFileInfo.
/// </summary>
public class FileInfoWrapper : IFileInfo
{
    private readonly FileInfo _fileInfo;

    public FileInfoWrapper(FileInfo fileInfo)
    {
        _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
    }

    /// <inheritdoc />
    public string Name => _fileInfo.Name;

    /// <inheritdoc />
    public string FullPath => _fileInfo.FullName;

    /// <inheritdoc />
    public string Extension => _fileInfo.Extension;

    /// <inheritdoc />
    public long Length => _fileInfo.Length;

    /// <inheritdoc />
    public DateTime LastWriteTime => _fileInfo.LastWriteTime;

    /// <inheritdoc />
    public DateTime CreationTime => _fileInfo.CreationTime;

    /// <inheritdoc />
    public bool Exists => _fileInfo.Exists;
}

/// <summary>
/// Wrapper for System.IO.DirectoryInfo to implement IDirectoryInfo.
/// </summary>
public class DirectoryInfoWrapper : IDirectoryInfo
{
    private readonly DirectoryInfo _directoryInfo;

    public DirectoryInfoWrapper(DirectoryInfo directoryInfo)
    {
        _directoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
    }

    /// <inheritdoc />
    public string Name => _directoryInfo.Name;

    /// <inheritdoc />
    public string FullPath => _directoryInfo.FullName;

    /// <inheritdoc />
    public DateTime LastWriteTime => _directoryInfo.LastWriteTime;

    /// <inheritdoc />
    public DateTime CreationTime => _directoryInfo.CreationTime;

    /// <inheritdoc />
    public bool Exists => _directoryInfo.Exists;
}
