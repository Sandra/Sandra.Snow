using System;

namespace Snow.Abstractions;

/// <summary>
/// Progress information for static site generation.
/// </summary>
public record GeneratorProgress(
    int TotalFiles,
    int ProcessedFiles,
    int ErrorCount,
    string? CurrentFile = null,
    string? CurrentStage = null,
    TimeSpan? Elapsed = null,
    double? PercentComplete = null)
{
    /// <summary>
    /// Gets the percentage complete (0-100).
    /// </summary>
    public double PercentageComplete => TotalFiles > 0 
        ? Math.Round((double)ProcessedFiles / TotalFiles * 100, 1)
        : 0.0;

    /// <summary>
    /// Gets whether the generation is complete.
    /// </summary>
    public bool IsComplete => ProcessedFiles >= TotalFiles;

    /// <summary>
    /// Gets the processing rate (files per second).
    /// </summary>
    public double FilesPerSecond => Elapsed?.TotalSeconds > 0 
        ? ProcessedFiles / Elapsed.Value.TotalSeconds
        : 0.0;

    /// <summary>
    /// Creates an initial progress report.
    /// </summary>
    public static GeneratorProgress Start(int totalFiles) => 
        new(totalFiles, 0, 0);

    /// <summary>
    /// Creates a progress report for a single file processing.
    /// </summary>
    public GeneratorProgress WithFileProgress(string currentFile, string stage) => 
        this with 
        { 
            ProcessedFiles = ProcessedFiles + 1, 
            CurrentFile = currentFile, 
            CurrentStage = stage 
        };

    /// <summary>
    /// Creates a progress report with an error.
    /// </summary>
    public GeneratorProgress WithError() => 
        this with { ErrorCount = ErrorCount + 1 };

    /// <summary>
    /// Updates the elapsed time.
    /// </summary>
    public GeneratorProgress WithElapsed(TimeSpan elapsed) => 
        this with { Elapsed = elapsed };
}

/// <summary>
/// Interface for components that can report progress.
/// </summary>
public interface IProgressReporter
{
    /// <summary>
    /// Reports progress for the current operation.
    /// </summary>
    /// <param name="progress">The progress information.</param>
    void ReportProgress(GeneratorProgress progress);
}
