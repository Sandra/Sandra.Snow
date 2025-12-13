using System;

namespace Snow.Abstractions;

/// <summary>
/// Represents information about a file.
/// </summary>
public interface IFileInfo
{
    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the full path of the file.
    /// </summary>
    string FullPath { get; }

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    string Extension { get; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    long Length { get; }

    /// <summary>
    /// Gets the last write time of the file.
    /// </summary>
    DateTime LastWriteTime { get; }

    /// <summary>
    /// Gets the creation time of the file.
    /// </summary>
    DateTime CreationTime { get; }

    /// <summary>
    /// Gets a value indicating whether the file exists.
    /// </summary>
    bool Exists { get; }
}

/// <summary>
/// Represents information about a directory.
/// </summary>
public interface IDirectoryInfo
{
    /// <summary>
    /// Gets the name of the directory.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the full path of the directory.
    /// </summary>
    string FullPath { get; }

    /// <summary>
    /// Gets the last write time of the directory.
    /// </summary>
    DateTime LastWriteTime { get; }

    /// <summary>
    /// Gets the creation time of the directory.
    /// </summary>
    DateTime CreationTime { get; }

    /// <summary>
    /// Gets a value indicating whether the directory exists.
    /// </summary>
    bool Exists { get; }
}
