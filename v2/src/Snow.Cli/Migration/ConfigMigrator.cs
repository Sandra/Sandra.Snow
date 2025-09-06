using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snow.Engine.Configuration;
using Spectre.Console;

namespace Snow.Cli.Migration;

/// <summary>
/// Migrates Sandra.Snow v1 configuration to v2 format.
/// </summary>
public class ConfigMigrator
{
    private readonly ILogger<ConfigMigrator> _logger;
    private readonly Dictionary<string, ConfigMappingRule> _mappingTable;

    public ConfigMigrator(ILogger<ConfigMigrator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mappingTable = CreateMappingTable();
    }

    /// <summary>
    /// Migrates a v1 configuration file to v2 format.
    /// </summary>
    /// <param name="v1ConfigPath">Path to the v1 configuration file.</param>
    /// <param name="outputPath">Path for the migrated v2 configuration.</param>
    /// <param name="interactive">Whether to prompt for ambiguous migrations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Migration result with warnings and errors.</returns>
    public async Task<MigrationResult> MigrateConfigAsync(
        string v1ConfigPath,
        string outputPath,
        bool interactive = false,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(v1ConfigPath))
        {
            throw new FileNotFoundException($"v1 configuration file not found: {v1ConfigPath}");
        }

        _logger.LogInformation("Migrating configuration: {V1Config} -> {V2Config}", v1ConfigPath, outputPath);

        var result = new MigrationResult();

        try
        {
            // Read v1 configuration
            var v1Content = await File.ReadAllTextAsync(v1ConfigPath, cancellationToken);
            var v1Config = JsonSerializer.Deserialize<Dictionary<string, object>>(v1Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            if (v1Config == null)
            {
                throw new InvalidOperationException("Failed to parse v1 configuration");
            }

            // Create v2 configuration
            var v2Config = new Dictionary<string, object>();
            
            // Apply migrations
            foreach (var kvp in v1Config)
            {
                await MigrateConfigKey(kvp.Key, kvp.Value, v2Config, result, interactive);
            }

            // Add v2 defaults for new features
            AddV2Defaults(v2Config, result);

            // Create backup of original
            await CreateBackup(v1ConfigPath, cancellationToken);

            // Write migrated configuration
            await WriteV2Config(v2Config, outputPath, cancellationToken);

            result.Success = true;
            result.OutputPath = outputPath;

            _logger.LogInformation("Configuration migration completed: {Warnings} warnings, {Errors} errors", 
                result.Warnings.Count, result.Errors.Count);

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Migration failed: {ex.Message}");
            _logger.LogError(ex, "Configuration migration failed");
            return result;
        }
    }

    /// <summary>
    /// Validates a v1 site for compatibility with v2.
    /// </summary>
    /// <param name="sitePath">Path to the v1 site directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with issues found.</returns>
    public async Task<ValidationResult> ValidateSiteAsync(string sitePath, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult();

        _logger.LogInformation("Validating v1 site for v2 compatibility: {SitePath}", sitePath);

        try
        {
            // Validate directory structure
            await ValidateDirectoryStructure(sitePath, result);

            // Validate content files
            await ValidateContentFiles(sitePath, result, cancellationToken);

            // Validate templates
            await ValidateTemplates(sitePath, result);

            _logger.LogInformation("Site validation completed: {Issues} issues found", result.Issues.Count);
        }
        catch (Exception ex)
        {
            result.Issues.Add(new ValidationIssue(ValidationSeverity.Error, "Validation failed", ex.Message));
            _logger.LogError(ex, "Site validation failed");
        }

        return result;
    }

    /// <summary>
    /// Migrates a single configuration key using the mapping table.
    /// </summary>
    private async Task MigrateConfigKey(
        string key, 
        object value, 
        Dictionary<string, object> v2Config, 
        MigrationResult result, 
        bool interactive)
    {
        var normalizedKey = key.ToLowerInvariant();

        if (_mappingTable.TryGetValue(normalizedKey, out var rule))
        {
            switch (rule.Action)
            {
                case MigrationAction.Copy:
                    v2Config[rule.NewKey ?? key] = value;
                    result.Migrations.Add($"Copied: {key} -> {rule.NewKey ?? key}");
                    break;

                case MigrationAction.Rename:
                    v2Config[rule.NewKey!] = value;
                    result.Migrations.Add($"Renamed: {key} -> {rule.NewKey}");
                    break;

                case MigrationAction.Transform:
                    var transformedValue = await TransformValue(key, value, rule);
                    v2Config[rule.NewKey ?? key] = transformedValue;
                    result.Migrations.Add($"Transformed: {key} -> {rule.NewKey ?? key}");
                    break;

                case MigrationAction.Remove:
                    result.Warnings.Add($"Removed obsolete setting: {key} - {rule.Message}");
                    break;

                case MigrationAction.Breaking:
                    if (interactive)
                    {
                        var userChoice = AnsiConsole.Confirm($"Breaking change for '{key}': {rule.Message}. Apply suggested fix?");
                        if (userChoice && !string.IsNullOrEmpty(rule.NewKey))
                        {
                            v2Config[rule.NewKey] = rule.DefaultValue ?? value;
                            result.Migrations.Add($"Applied fix: {key} -> {rule.NewKey}");
                        }
                        else
                        {
                            result.Errors.Add($"Manual fix required for '{key}': {rule.Message}");
                        }
                    }
                    else
                    {
                        result.Errors.Add($"Breaking change: {key} - {rule.Message}");
                    }
                    break;

                default:
                    result.Warnings.Add($"Unknown migration action for key: {key}");
                    break;
            }
        }
        else
        {
            // Unknown key - copy as-is but warn
            v2Config[key] = value;
            result.Warnings.Add($"Unknown key copied as-is: {key}");
        }
    }

    /// <summary>
    /// Transforms a value according to migration rules.
    /// </summary>
    private Task<object> TransformValue(string key, object value, ConfigMappingRule rule)
    {
        // Simple transformations - can be extended
        switch (rule.TransformType)
        {
            case "url-format":
                // Convert old URL format to new format
                if (value is string urlFormat)
                {
                    return Task.FromResult<object>(urlFormat.Replace("/{year}/{month}/{day}/", "/yyyy/MM/dd/"));
                }
                break;

            case "boolean-invert":
                if (value is bool boolValue)
                {
                    return Task.FromResult<object>(!boolValue);
                }
                break;

            case "array-to-object":
                if (value is JsonElement arrayElement && arrayElement.ValueKind == JsonValueKind.Array)
                {
                    // Transform array to object format
                    return Task.FromResult<object>(arrayElement.EnumerateArray().Select((item, index) => new { file = item.ToString() }).ToArray());
                }
                break;
        }

        return Task.FromResult(value);
    }

    /// <summary>
    /// Adds v2-specific defaults that didn't exist in v1.
    /// </summary>
    private void AddV2Defaults(Dictionary<string, object> v2Config, MigrationResult result)
    {
        var defaults = new Dictionary<string, object>
        {
            ["markdown"] = new
            {
                extensions = new[] { "emoji", "smartypants", "tables", "tasklists", "autolinks" },
                enableAdvancedExtensions = true,
                sanitizeHtml = true,
                headingIds = "github"
            }
        };

        foreach (var kvp in defaults)
        {
            if (!v2Config.ContainsKey(kvp.Key))
            {
                v2Config[kvp.Key] = kvp.Value;
                result.Migrations.Add($"Added v2 default: {kvp.Key}");
            }
        }
    }

    /// <summary>
    /// Creates the mapping table for v1 to v2 configuration migration.
    /// </summary>
    private static Dictionary<string, ConfigMappingRule> CreateMappingTable()
    {
        return new Dictionary<string, ConfigMappingRule>(StringComparer.OrdinalIgnoreCase)
        {
            // Direct copies (no change)
            ["blogtitle"] = new(MigrationAction.Copy),
            ["siteurl"] = new(MigrationAction.Copy),
            ["author"] = new(MigrationAction.Copy),
            ["email"] = new(MigrationAction.Copy),
            ["theme"] = new(MigrationAction.Copy),

            // Renames
            ["posts"] = new(MigrationAction.Rename, "postsDirectory"),
            ["pages"] = new(MigrationAction.Rename, "pagesDirectory"),
            ["layouts"] = new(MigrationAction.Rename, "layoutsDirectory"),

            // URL format transformation
            ["urlformat"] = new(MigrationAction.Transform, "postUrlFormat") { TransformType = "url-format" },

            // Process files need restructuring
            ["processfiles"] = new(MigrationAction.Transform, "processFiles") { TransformType = "array-to-object" },

            // Breaking changes
            ["server"] = new(MigrationAction.Breaking) { Message = "Server functionality replaced with 'sandra serve' command. Remove this setting." },
            
            ["debug"] = new(MigrationAction.Breaking) { Message = "Debug flag replaced with 'sandra --verbose' command line option. Remove this setting." },

            // Removed features
            ["rss"] = new(MigrationAction.Remove) { Message = "RSS generation now configured through processFiles. See migration guide for examples." },
            
            ["copyDirectories"] = new(MigrationAction.Copy),
            ["copyFiles"] = new(MigrationAction.Copy),
            ["defaultPostLayout"] = new(MigrationAction.Copy),
            ["defaultPageLayout"] = new(MigrationAction.Copy),
            ["pageSize"] = new(MigrationAction.Copy),
            ["feedSize"] = new(MigrationAction.Copy)
        };
    }

    /// <summary>
    /// Creates a backup of the original configuration file.
    /// </summary>
    private Task CreateBackup(string originalPath, CancellationToken cancellationToken)
    {
        var backupPath = originalPath + ".v1-backup-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        File.Copy(originalPath, backupPath);
        _logger.LogInformation("Created backup: {BackupPath}", backupPath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Writes the migrated v2 configuration.
    /// </summary>
    private async Task WriteV2Config(Dictionary<string, object> config, string outputPath, CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(config, options);
        
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        await File.WriteAllTextAsync(outputPath, json, cancellationToken);
    }

    /// <summary>
    /// Validates the directory structure of a v1 site.
    /// </summary>
    private async Task ValidateDirectoryStructure(string sitePath, ValidationResult result)
    {
        var requiredDirs = new[] { "_posts" };
        var recommendedDirs = new[] { "_pages", "_layouts", "themes" };

        foreach (var dir in requiredDirs)
        {
            var dirPath = Path.Combine(sitePath, dir);
            if (!Directory.Exists(dirPath))
            {
                result.Issues.Add(new ValidationIssue(ValidationSeverity.Error, 
                    $"Required directory missing: {dir}",
                    $"Create the {dir} directory for your content"));
            }
        }

        foreach (var dir in recommendedDirs)
        {
            var dirPath = Path.Combine(sitePath, dir);
            if (!Directory.Exists(dirPath))
            {
                result.Issues.Add(new ValidationIssue(ValidationSeverity.Warning,
                    $"Recommended directory missing: {dir}",
                    $"Consider creating {dir} directory for better organization"));
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Validates content files for v2 compatibility.
    /// </summary>
    private async Task ValidateContentFiles(string sitePath, ValidationResult result, CancellationToken cancellationToken)
    {
        var postsDir = Path.Combine(sitePath, "_posts");
        if (Directory.Exists(postsDir))
        {
            var files = Directory.GetFiles(postsDir, "*.md", SearchOption.AllDirectories)
                               .Concat(Directory.GetFiles(postsDir, "*.markdown", SearchOption.AllDirectories));

            foreach (var file in files)
            {
                await ValidateContentFile(file, result, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Validates a single content file.
    /// </summary>
    private async Task ValidateContentFile(string filePath, ValidationResult result, CancellationToken cancellationToken)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            var fileName = Path.GetFileName(filePath);

            // Check file naming convention
            if (!System.Text.RegularExpressions.Regex.IsMatch(fileName, @"^\d{4}-\d{2}-\d{2}-.+\.md$"))
            {
                result.Issues.Add(new ValidationIssue(ValidationSeverity.Warning,
                    $"File naming: {fileName}",
                    "Posts should follow YYYY-MM-DD-slug.md convention"));
            }

            // Check for required front matter
            if (!content.StartsWith("---"))
            {
                result.Issues.Add(new ValidationIssue(ValidationSeverity.Error,
                    $"Missing front matter: {fileName}",
                    "Posts require YAML front matter at the beginning"));
            }

            // Check for Nancy-specific syntax that won't work in v2
            if (content.Contains("@Html.") || content.Contains("@Url."))
            {
                result.Issues.Add(new ValidationIssue(ValidationSeverity.Warning,
                    $"Nancy syntax detected: {fileName}",
                    "Nancy Razor helpers (@Html.*, @Url.*) may need manual conversion"));
            }
        }
        catch (Exception ex)
        {
            result.Issues.Add(new ValidationIssue(ValidationSeverity.Error,
                $"Failed to validate {Path.GetFileName(filePath)}",
                ex.Message));
        }
    }

    /// <summary>
    /// Validates template files for v2 compatibility.
    /// </summary>
    private async Task ValidateTemplates(string sitePath, ValidationResult result)
    {
        var themesDir = Path.Combine(sitePath, "themes");
        if (Directory.Exists(themesDir))
        {
            var templateFiles = Directory.GetFiles(themesDir, "*.cshtml", SearchOption.AllDirectories);
            
            foreach (var file in templateFiles)
            {
                await ValidateTemplateFile(file, result);
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Validates a single template file.
    /// </summary>
    private async Task ValidateTemplateFile(string filePath, ValidationResult result)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            // Check for Nancy-specific features
            var nancyFeatures = new[]
            {
                ("@Html.AntiForgeryToken", "Anti-forgery tokens not supported in v2"),
                ("@Url.Content", "URL helpers simplified in v2"),
                ("Html.Raw", "Use @Html.Raw() instead"),
                ("ViewBag.", "ViewBag not directly supported, use model properties"),
                ("TempData.", "TempData not supported in static generation")
            };

            foreach (var (pattern, message) in nancyFeatures)
            {
                if (content.Contains(pattern))
                {
                    result.Issues.Add(new ValidationIssue(ValidationSeverity.Warning,
                        $"Nancy feature in {fileName}",
                        $"{pattern}: {message}"));
                }
            }
        }
        catch (Exception ex)
        {
            result.Issues.Add(new ValidationIssue(ValidationSeverity.Error,
                $"Failed to validate template {Path.GetFileName(filePath)}",
                ex.Message));
        }
    }
}

/// <summary>
/// Configuration mapping rule for migration.
/// </summary>
public record ConfigMappingRule(
    MigrationAction Action,
    string? NewKey = null)
{
    public string? Message { get; init; }
    public object? DefaultValue { get; init; }
    public string? TransformType { get; init; }
}

/// <summary>
/// Migration actions for configuration keys.
/// </summary>
public enum MigrationAction
{
    Copy,
    Rename,
    Transform,
    Remove,
    Breaking
}

/// <summary>
/// Result of a configuration migration operation.
/// </summary>
public class MigrationResult
{
    public bool Success { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public List<string> Migrations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets whether the migration has any issues.
    /// </summary>
    public bool HasIssues => Warnings.Any() || Errors.Any();
}

/// <summary>
/// Result of a site validation operation.
/// </summary>
public class ValidationResult
{
    public List<ValidationIssue> Issues { get; set; } = new();

    /// <summary>
    /// Gets whether validation found any errors.
    /// </summary>
    public bool HasErrors => Issues.Any(i => i.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Gets whether validation found any warnings.
    /// </summary>
    public bool HasWarnings => Issues.Any(i => i.Severity == ValidationSeverity.Warning);
}

/// <summary>
/// Represents a validation issue found during site analysis.
/// </summary>
public record ValidationIssue(
    ValidationSeverity Severity,
    string Title,
    string Description,
    string? FilePath = null);

/// <summary>
/// Severity levels for validation issues.
/// </summary>
public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}
