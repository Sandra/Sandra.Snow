using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Engine.Configuration;

namespace Snow.Engine.Incremental;

/// <summary>
/// Manages dependencies between content files and tracks changes for incremental builds.
/// </summary>
public class DependencyGraph
{
    private readonly ConcurrentDictionary<string, BuildNode> _nodes = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _dependencies = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _dependents = new();
    private readonly ILogger<DependencyGraph> _logger;

    public DependencyGraph(ILogger<DependencyGraph> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Adds or updates a node in the dependency graph.
    /// </summary>
    /// <param name="node">The build node to add.</param>
    public void AddNode(BuildNode node)
    {
        _nodes.AddOrUpdate(node.Path, node, (_, _) => node);
        _dependencies.TryAdd(node.Path, new HashSet<string>());
        _dependents.TryAdd(node.Path, new HashSet<string>());
        
        _logger.LogTrace("Added node: {NodePath} ({NodeType})", node.Path, node.Type);
    }

    /// <summary>
    /// Adds a dependency relationship (source depends on dependency).
    /// </summary>
    /// <param name="sourcePath">The path that depends on another.</param>
    /// <param name="dependencyPath">The path that is depended upon.</param>
    public void AddDependency(string sourcePath, string dependencyPath)
    {
        // Add forward dependency
        var deps = _dependencies.GetOrAdd(sourcePath, _ => new HashSet<string>());
        deps.Add(dependencyPath);

        // Add reverse dependency for invalidation
        var dependents = _dependents.GetOrAdd(dependencyPath, _ => new HashSet<string>());
        dependents.Add(sourcePath);

        _logger.LogTrace("Added dependency: {Source} -> {Dependency}", sourcePath, dependencyPath);
    }

    /// <summary>
    /// Gets all nodes that need to be rebuilt when the specified paths change.
    /// </summary>
    /// <param name="changedPaths">The paths that have changed.</param>
    /// <returns>Collection of nodes that need rebuilding.</returns>
    public IReadOnlyList<BuildNode> GetNodesToRebuild(IEnumerable<string> changedPaths)
    {
        var toRebuild = new HashSet<string>();
        var visited = new HashSet<string>();

        foreach (var changedPath in changedPaths)
        {
            CollectDependents(changedPath, toRebuild, visited);
        }

        var result = toRebuild
            .Where(path => _nodes.ContainsKey(path))
            .Select(path => _nodes[path])
            .ToList();

        _logger.LogDebug("Incremental build: {ChangedCount} changes -> {RebuildCount} rebuilds", 
            changedPaths.Count(), result.Count);

        return result;
    }

    /// <summary>
    /// Updates the content hash for a node.
    /// </summary>
    /// <param name="path">The node path.</param>
    /// <param name="content">The current content.</param>
    /// <returns>True if the content has changed since last update.</returns>
    public bool UpdateContentHash(string path, string content)
    {
        if (!_nodes.TryGetValue(path, out var node))
        {
            return true; // New node, always needs build
        }

        var newHash = ComputeHash(content);
        var hasChanged = node.ContentHash != newHash;

        if (hasChanged)
        {
            var updatedNode = node with { ContentHash = newHash, LastModified = DateTime.UtcNow };
            _nodes.TryUpdate(path, updatedNode, node);
            
            _logger.LogTrace("Content hash updated for {Path}: {Hash}", path, newHash);
        }

        return hasChanged;
    }

    /// <summary>
    /// Checks if a file has changed by comparing its file system timestamp and content hash.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="fileProvider">File provider for reading content.</param>
    /// <returns>True if the file has changed.</returns>
    public async Task<bool> HasFileChangedAsync(string filePath, IFileProvider fileProvider)
    {
        if (!fileProvider.FileExists(filePath))
        {
            return _nodes.ContainsKey(filePath); // File deleted
        }

        if (!_nodes.TryGetValue(filePath, out var node))
        {
            return true; // New file
        }

        var fileInfo = fileProvider.GetFileInfo(filePath);
        if (fileInfo == null)
        {
            return true;
        }

        // Quick check: file timestamp
        if (fileInfo.LastWriteTime.ToUniversalTime() <= node.LastModified)
        {
            return false; // File hasn't been modified
        }

        // Detailed check: content hash
        try
        {
            var content = await fileProvider.ReadAllTextAsync(filePath);
            return UpdateContentHash(filePath, content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read file for hash comparison: {FilePath}", filePath);
            return true; // Assume changed if we can't read it
        }
    }

    /// <summary>
    /// Gets all nodes in the graph.
    /// </summary>
    public IReadOnlyList<BuildNode> GetAllNodes()
    {
        return _nodes.Values.ToList();
    }

    /// <summary>
    /// Clears the dependency graph.
    /// </summary>
    public void Clear()
    {
        _nodes.Clear();
        _dependencies.Clear();
        _dependents.Clear();
        _logger.LogDebug("Dependency graph cleared");
    }

    /// <summary>
    /// Recursively collects all dependents of a changed path.
    /// </summary>
    private void CollectDependents(string changedPath, HashSet<string> toRebuild, HashSet<string> visited)
    {
        if (visited.Contains(changedPath))
        {
            return; // Avoid cycles
        }

        visited.Add(changedPath);
        toRebuild.Add(changedPath);

        if (_dependents.TryGetValue(changedPath, out var dependents))
        {
            foreach (var dependent in dependents)
            {
                CollectDependents(dependent, toRebuild, visited);
            }
        }
    }

    /// <summary>
    /// Computes a content hash for change detection.
    /// </summary>
    private static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Saves the dependency graph to a cache file for persistence.
    /// </summary>
    /// <param name="cacheFilePath">Path to save the cache file.</param>
    public async Task SaveCacheAsync(string cacheFilePath)
    {
        try
        {
            var cache = new DependencyCache
            {
                Nodes = _nodes.Values.ToList(),
                Dependencies = _dependencies.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToList()),
                LastUpdated = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(cache, SnowJsonContext.Default.DependencyCache);

            var cacheDirectory = Path.GetDirectoryName(cacheFilePath);
            if (!string.IsNullOrEmpty(cacheDirectory) && !Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }

            await File.WriteAllTextAsync(cacheFilePath, json);
            _logger.LogDebug("Dependency graph saved to cache: {CacheFile}", cacheFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save dependency graph cache: {CacheFile}", cacheFilePath);
        }
    }

    /// <summary>
    /// Loads the dependency graph from a cache file.
    /// </summary>
    /// <param name="cacheFilePath">Path to load the cache file from.</param>
    public async Task LoadCacheAsync(string cacheFilePath)
    {
        try
        {
            if (!File.Exists(cacheFilePath))
            {
                _logger.LogDebug("No dependency cache found: {CacheFile}", cacheFilePath);
                return;
            }

            var json = await File.ReadAllTextAsync(cacheFilePath);
            var cache = JsonSerializer.Deserialize(json, SnowJsonContext.Default.DependencyCache);

            if (cache == null)
            {
                _logger.LogWarning("Invalid dependency cache file: {CacheFile}", cacheFilePath);
                return;
            }

            // Restore nodes
            foreach (var node in cache.Nodes)
            {
                _nodes[node.Path] = node;
            }

            // Restore dependencies
            foreach (var kvp in cache.Dependencies)
            {
                _dependencies[kvp.Key] = new HashSet<string>(kvp.Value);
                
                // Rebuild reverse dependencies
                foreach (var dependency in kvp.Value)
                {
                    var dependents = _dependents.GetOrAdd(dependency, _ => new HashSet<string>());
                    dependents.Add(kvp.Key);
                }
            }

            _logger.LogInformation("Loaded dependency graph from cache: {NodeCount} nodes, {DependencyCount} dependencies",
                _nodes.Count, _dependencies.Values.Sum(d => d.Count));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load dependency graph cache: {CacheFile}", cacheFilePath);
            Clear(); // Start fresh if cache is corrupted
        }
    }
}

/// <summary>
/// Represents a node in the build dependency graph.
/// </summary>
public record BuildNode(
    string Path,
    BuildNodeType Type,
    string ContentHash = "",
    DateTime LastModified = default);

/// <summary>
/// Types of build nodes in the dependency graph.
/// </summary>
public enum BuildNodeType
{
    /// <summary>
    /// Source content file (markdown, etc.)
    /// </summary>
    SourceContent,

    /// <summary>
    /// Template file (layouts, partials)
    /// </summary>
    Template,

    /// <summary>
    /// Generated page output
    /// </summary>
    GeneratedPage,

    /// <summary>
    /// Asset file (CSS, JS, images)
    /// </summary>
    Asset,

    /// <summary>
    /// Configuration file
    /// </summary>
    Configuration
}

/// <summary>
/// Cache structure for persisting dependency graph.
/// </summary>
public class DependencyCache
{
    public List<BuildNode> Nodes { get; set; } = new();
    public Dictionary<string, List<string>> Dependencies { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
