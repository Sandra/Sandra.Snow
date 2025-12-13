using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Snow.Engine.Incremental;

/// <summary>
/// Cross-platform file watcher with debouncing for efficient change detection.
/// </summary>
public class FileWatcher : IDisposable
{
    private readonly string _watchDirectory;
    private readonly ILogger<FileWatcher> _logger;
    private readonly FileSystemWatcher _watcher;
    private readonly Channel<FileChangeEvent> _changeChannel;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _debouncingTask;

    /// <summary>
    /// Event raised when files have changed after debouncing.
    /// </summary>
    public event Func<IReadOnlyList<string>, Task>? FilesChanged;

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds.
    /// </summary>
    public int DebounceDelayMs { get; set; } = 100;

    /// <summary>
    /// Gets or sets the patterns to ignore during file watching.
    /// </summary>
    public IReadOnlyList<string> IgnorePatterns { get; set; } = new[]
    {
        "*.tmp", "*.swp", "*~", ".git", ".vs", ".vscode", "node_modules", 
        "bin", "obj", ".sandra", "*.log"
    };

    public FileWatcher(string watchDirectory, ILogger<FileWatcher> logger)
    {
        _watchDirectory = Path.GetFullPath(watchDirectory);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!Directory.Exists(_watchDirectory))
        {
            throw new DirectoryNotFoundException($"Watch directory not found: {_watchDirectory}");
        }

        // Create unbounded channel for file change events
        _changeChannel = Channel.CreateUnbounded<FileChangeEvent>();

        // Setup file system watcher
        _watcher = new FileSystemWatcher(_watchDirectory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
        };

        _watcher.Created += OnFileSystemEvent;
        _watcher.Changed += OnFileSystemEvent;
        _watcher.Deleted += OnFileSystemEvent;
        _watcher.Renamed += OnFileRenamed;

        // Start debouncing task
        _debouncingTask = Task.Run(DebouncingLoop, _cancellationTokenSource.Token);

        _logger.LogInformation("File watcher initialized for: {WatchDirectory}", _watchDirectory);
    }

    /// <summary>
    /// Starts watching for file changes.
    /// </summary>
    public void Start()
    {
        _watcher.EnableRaisingEvents = true;
        _logger.LogInformation("File watching started");
    }

    /// <summary>
    /// Stops watching for file changes.
    /// </summary>
    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
        _logger.LogInformation("File watching stopped");
    }

    /// <summary>
    /// Handles file system events.
    /// </summary>
    private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
    {
        if (ShouldIgnoreFile(e.FullPath))
        {
            return;
        }

        var changeEvent = new FileChangeEvent(e.FullPath, MapChangeType(e.ChangeType), DateTime.UtcNow);
        
        if (!_changeChannel.Writer.TryWrite(changeEvent))
        {
            _logger.LogWarning("Failed to queue file change event: {FilePath}", e.FullPath);
        }
    }

    /// <summary>
    /// Handles file rename events.
    /// </summary>
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        if (ShouldIgnoreFile(e.FullPath))
        {
            return;
        }

        // Treat rename as delete old + create new
        var deleteEvent = new FileChangeEvent(e.OldFullPath, FileChangeType.Deleted, DateTime.UtcNow);
        var createEvent = new FileChangeEvent(e.FullPath, FileChangeType.Added, DateTime.UtcNow);

        _changeChannel.Writer.TryWrite(deleteEvent);
        _changeChannel.Writer.TryWrite(createEvent);
    }

    /// <summary>
    /// Main debouncing loop that batches and processes file changes.
    /// </summary>
    private async Task DebouncingLoop()
    {
        var changeTracker = new ConcurrentDictionary<string, FileChangeEvent>();

        try
        {
            while (await _changeChannel.Reader.WaitToReadAsync(_cancellationTokenSource.Token))
            {
                // Collect all available changes
                while (_changeChannel.Reader.TryRead(out var changeEvent))
                {
                    changeTracker.AddOrUpdate(
                        changeEvent.FilePath,
                        changeEvent,
                        (_, existing) => changeEvent.Timestamp > existing.Timestamp ? changeEvent : existing);
                }

                // Wait for debounce period
                await Task.Delay(DebounceDelayMs, _cancellationTokenSource.Token);

                // Check if more changes arrived during debounce
                if (_changeChannel.Reader.TryRead(out _))
                {
                    continue; // More changes arrived, reset debounce timer
                }

                // Process batched changes
                if (changeTracker.Any())
                {
                    var changedFiles = changeTracker.Values
                        .Select(e => e.FilePath)
                        .ToList(); // Include all changes including deletions

                    if (changedFiles.Any())
                    {
                        _logger.LogDebug("Processing {ChangeCount} file changes after debounce", changedFiles.Count);
                        
                        try
                        {
                            await (FilesChanged?.Invoke(changedFiles) ?? Task.CompletedTask);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing file changes");
                        }
                    }

                    changeTracker.Clear();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("File watcher debouncing loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in file watcher debouncing loop");
        }
    }

    /// <summary>
    /// Checks if a file should be ignored based on patterns.
    /// </summary>
    private bool ShouldIgnoreFile(string filePath)
    {
        try
        {
            var relativePath = Path.GetRelativePath(_watchDirectory, filePath);
            var fileName = Path.GetFileName(filePath);

            foreach (var pattern in IgnorePatterns)
            {
                if (MatchesPattern(fileName, pattern) || MatchesPattern(relativePath, pattern))
                {
                    _logger.LogTrace("Ignoring file change: {FilePath} (matches pattern: {Pattern})", filePath, pattern);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking ignore patterns for file: {FilePath}", filePath);
            return true; // Ignore files we can't process
        }
    }

    /// <summary>
    /// Simple pattern matching (supports * wildcard).
    /// </summary>
    private static bool MatchesPattern(string text, string pattern)
    {
        if (pattern == "*")
            return true;

        if (!pattern.Contains('*'))
            return string.Equals(text, pattern, StringComparison.OrdinalIgnoreCase);

        // Simple wildcard matching
        var regexPattern = "^" + pattern.Replace("*", ".*") + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(text, regexPattern, 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Maps FileSystemWatcher change types to our internal types.
    /// </summary>
    private static FileChangeType MapChangeType(WatcherChangeTypes changeType)
    {
        return changeType switch
        {
            WatcherChangeTypes.Created => FileChangeType.Added,
            WatcherChangeTypes.Changed => FileChangeType.Modified,
            WatcherChangeTypes.Deleted => FileChangeType.Deleted,
            WatcherChangeTypes.Renamed => FileChangeType.Modified,
            _ => FileChangeType.Modified
        };
    }

    /// <summary>
    /// Disposes the file watcher and releases resources.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        
        try
        {
            _debouncingTask?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
            // Ignore cancellation exceptions
        }

        _changeChannel.Writer.TryComplete();
        _watcher?.Dispose();
        _cancellationTokenSource?.Dispose();
        
        _logger.LogDebug("File watcher disposed");
    }
}

/// <summary>
/// Represents a file change event.
/// </summary>
public record FileChangeEvent(
    string FilePath,
    FileChangeType ChangeType,
    DateTime Timestamp);

/// <summary>
/// Types of file changes.
/// </summary>
public enum FileChangeType
{
    Added,
    Modified,
    Deleted
}
