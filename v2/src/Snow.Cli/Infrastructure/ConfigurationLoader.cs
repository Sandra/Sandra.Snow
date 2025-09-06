using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Snow.Engine.Configuration;
using Spectre.Console;

namespace Snow.Cli.Infrastructure;

/// <summary>
/// Loads Sandra.Snow configuration from various sources.
/// </summary>
public static class ConfigurationLoader
{
    /// <summary>
    /// Loads site settings from configuration files and CLI options.
    /// </summary>
    /// <param name="configPath">Optional path to configuration file or directory.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The loaded site settings.</returns>
    public static async Task<SiteSettings> LoadSettingsAsync(string? configPath, CancellationToken cancellationToken = default)
    {
        var currentDirectory = DetermineWorkingDirectory(configPath);
        var configFile = FindConfigurationFile(currentDirectory);

        if (configFile == null)
        {
            throw new FileNotFoundException(
                $"No configuration file found in {currentDirectory}. " +
                "Expected: sandra.json, sandra.config.json, or snow.config.json");
        }

        try
        {
            AnsiConsole.MarkupLine($"[dim]Loading configuration from:[/] {Path.GetFileName(configFile)}");

            var configContent = await File.ReadAllTextAsync(configFile, cancellationToken);
            var settings = JsonSerializer.Deserialize<SiteSettings>(configContent, CreateJsonOptions());

            if (settings == null)
            {
                throw new InvalidOperationException("Configuration file deserialized to null");
            }

            // Update with actual directory
            var directoryBasedSettings = new SiteSettings(currentDirectory);
            CopySettings(settings, directoryBasedSettings);
            
            // Validate configuration
            ValidateSettings(directoryBasedSettings);

            return directoryBasedSettings;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON in configuration file {configFile}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Determines the working directory from the config path.
    /// </summary>
    private static string DetermineWorkingDirectory(string? configPath)
    {
        if (string.IsNullOrEmpty(configPath))
        {
            return Directory.GetCurrentDirectory();
        }

        if (File.Exists(configPath))
        {
            return Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory();
        }

        if (Directory.Exists(configPath))
        {
            return configPath;
        }

        throw new DirectoryNotFoundException($"Configuration path not found: {configPath}");
    }

    /// <summary>
    /// Finds the configuration file in the specified directory.
    /// </summary>
    private static string? FindConfigurationFile(string directory)
    {
        var possibleFiles = new[]
        {
            "sandra.json",
            "sandra.config.json", 
            "snow.config.json",
            "snow.config"
        };

        foreach (var fileName in possibleFiles)
        {
            var filePath = Path.Combine(directory, fileName);
            if (File.Exists(filePath))
            {
                return filePath;
            }
        }

        return null;
    }

    /// <summary>
    /// Creates JSON serializer options for configuration.
    /// </summary>
    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    /// <summary>
    /// Copies settings from source to target.
    /// </summary>
    private static void CopySettings(SiteSettings source, SiteSettings target)
    {
        // Copy all public properties
        target.Author = source.Author;
        target.Email = source.Email;
        target.BlogTitle = source.BlogTitle;
        target.SiteUrl = source.SiteUrl;
        target.PostsDirectoryRaw = source.PostsDirectoryRaw;
        target.PagesDirectoryRaw = source.PagesDirectoryRaw;
        target.LayoutsDirectoryRaw = source.LayoutsDirectoryRaw;
        target.Theme = source.Theme;
        target.ThemesDirectory = source.ThemesDirectory;
        target.PostsOutputRaw = source.PostsOutputRaw;
        target.PagesOutputRaw = source.PagesOutputRaw;
        target.PostUrlFormat = source.PostUrlFormat;
        target.PageUrlFormat = source.PageUrlFormat;
        target.DefaultPostLayout = source.DefaultPostLayout;
        target.DefaultPageLayout = source.DefaultPageLayout;
        target.CopyDirectories = source.CopyDirectories;
        target.CopyFiles = source.CopyFiles;
        target.ProcessFiles = source.ProcessFiles;
        target.PageSize = source.PageSize;
        target.FeedSize = source.FeedSize;
        target.Markdown = source.Markdown;

        // Refresh computed paths
        target.RefreshPaths();
    }

    /// <summary>
    /// Validates the loaded settings.
    /// </summary>
    private static void ValidateSettings(SiteSettings settings)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.BlogTitle))
            errors.Add("BlogTitle is required");

        if (string.IsNullOrWhiteSpace(settings.Theme))
            errors.Add("Theme is required");

        if (string.IsNullOrWhiteSpace(settings.PostsDirectoryRaw))
            errors.Add("Posts directory is required");

        if (!Directory.Exists(settings.PostsDirectory))
            errors.Add($"Posts directory not found: {settings.PostsDirectory}");

        var themePath = Path.Combine(settings.CurrentDirectory, settings.ThemesDirectory, settings.Theme);
        if (!Directory.Exists(themePath))
            errors.Add($"Theme directory not found: {themePath}");

        if (errors.Any())
        {
            throw new InvalidOperationException($"Configuration validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
