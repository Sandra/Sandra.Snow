using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snow.Cli.Migration;
using Spectre.Console;

namespace Snow.Cli.Commands;

/// <summary>
/// Handles migration from Sandra.Snow v1 to v2.
/// </summary>
public static class MigrateCommand
{
    /// <summary>
    /// Executes the migrate command.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Exit code.</returns>
    [RequiresUnreferencedCode("Migration uses reflection-based JSON serialization")]
    public static async Task<int> ExecuteAsync(string[] args)
    {
        try
        {
            var options = ParseMigrateOptions(args);
            
            if (options.ShowHelp)
            {
                ShowMigrateHelp();
                return 0;
            }

            if (string.IsNullOrEmpty(options.SourcePath))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Source path is required");
                AnsiConsole.MarkupLine("Use: [cyan]sandra migrate <v1-site-path>[/]");
                return 1;
            }

            if (!Directory.Exists(options.SourcePath))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Directory not found: {options.SourcePath}");
                return 1;
            }

            AnsiConsole.MarkupLine("[green]Sandra.Snow Migration Tool[/]");
            AnsiConsole.MarkupLine($"[dim]Migrating v1 site:[/] {options.SourcePath}");
            AnsiConsole.WriteLine();

            // Setup services
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            using var serviceProvider = services.BuildServiceProvider();

            var migrator = new ConfigMigrator(serviceProvider.GetRequiredService<ILogger<ConfigMigrator>>());

            // Step 1: Validate v1 site
            AnsiConsole.MarkupLine("[yellow]Step 1:[/] Validating v1 site...");
            var validationResult = await migrator.ValidateSiteAsync(options.SourcePath);
            
            DisplayValidationResults(validationResult);
            
            if (validationResult.HasErrors && !options.Force)
            {
                AnsiConsole.MarkupLine("[red]Validation failed.[/] Use --force to continue anyway.");
                return 1;
            }

            // Step 2: Migrate configuration
            AnsiConsole.MarkupLine("[yellow]Step 2:[/] Migrating configuration...");
            
            var v1ConfigPath = FindV1ConfigFile(options.SourcePath);
            if (v1ConfigPath == null)
            {
                AnsiConsole.MarkupLine("[red]Error:[/] No v1 configuration file found");
                AnsiConsole.MarkupLine("Expected: snow.config, snow.config.json");
                return 1;
            }

            var outputPath = options.OutputPath ?? Path.Combine(options.SourcePath, "sandra.json");
            var migrationResult = await migrator.MigrateConfigAsync(
                v1ConfigPath, 
                outputPath, 
                options.Interactive);

            DisplayMigrationResults(migrationResult);

            if (!migrationResult.Success)
            {
                return 1;
            }

            // Step 3: Summary and next steps
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]✓ Migration completed successfully![/]");
            AnsiConsole.MarkupLine($"[dim]New configuration:[/] {migrationResult.OutputPath}");
            
            if (migrationResult.HasIssues)
            {
                AnsiConsole.MarkupLine("[yellow]⚠ Review the warnings above before building[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[cyan]Next steps:[/]");
            AnsiConsole.WriteLine("1. Review the new sandra.json configuration");
            AnsiConsole.WriteLine("2. Test the migration: [cyan]sandra build[/]");
            AnsiConsole.WriteLine("3. Start development: [cyan]sandra serve --watch[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Migration failed:[/]");
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    /// <summary>
    /// Parses migrate command options.
    /// </summary>
    private static MigrateOptions ParseMigrateOptions(string[] args)
    {
        var options = new MigrateOptions();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "migrate":
                    // Skip command name
                    break;
                case "--help":
                case "-h":
                    options.ShowHelp = true;
                    break;
                case "--output":
                case "-o":
                    if (i + 1 < args.Length)
                        options.OutputPath = args[++i];
                    break;
                case "--interactive":
                case "-i":
                    options.Interactive = true;
                    break;
                case "--force":
                case "-f":
                    options.Force = true;
                    break;
                default:
                    if (string.IsNullOrEmpty(options.SourcePath) && !args[i].StartsWith("-"))
                    {
                        options.SourcePath = args[i];
                    }
                    break;
            }
        }

        return options;
    }

    /// <summary>
    /// Displays validation results.
    /// </summary>
    private static void DisplayValidationResults(Snow.Cli.Migration.ValidationResult result)
    {
        if (!result.Issues.Any())
        {
            AnsiConsole.MarkupLine("[green]✓ No validation issues found[/]");
            return;
        }

        var errorCount = result.Issues.Count(i => i.Severity == ValidationSeverity.Error);
        var warningCount = result.Issues.Count(i => i.Severity == ValidationSeverity.Warning);

        AnsiConsole.MarkupLine($"[yellow]Found {result.Issues.Count} issues:[/] {errorCount} errors, {warningCount} warnings");
        AnsiConsole.WriteLine();

        foreach (var issue in result.Issues)
        {
            var color = issue.Severity switch
            {
                ValidationSeverity.Error => "red",
                ValidationSeverity.Warning => "yellow",
                _ => "blue"
            };

            AnsiConsole.MarkupLine($"[{color}]{issue.Severity}:[/] {issue.Title}");
            AnsiConsole.MarkupLine($"[dim]{issue.Description}[/]");
            AnsiConsole.WriteLine();
        }
    }

    /// <summary>
    /// Displays migration results.
    /// </summary>
    private static void DisplayMigrationResults(MigrationResult result)
    {
        if (result.Migrations.Any())
        {
            AnsiConsole.MarkupLine($"[green]Applied {result.Migrations.Count} migrations:[/]");
            foreach (var migration in result.Migrations)
            {
                AnsiConsole.MarkupLine($"[dim]  ✓[/] {migration}");
            }
            AnsiConsole.WriteLine();
        }

        if (result.Warnings.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]{result.Warnings.Count} warnings:[/]");
            foreach (var warning in result.Warnings)
            {
                AnsiConsole.MarkupLine($"[yellow]  ⚠[/] {warning}");
            }
            AnsiConsole.WriteLine();
        }

        if (result.Errors.Any())
        {
            AnsiConsole.MarkupLine($"[red]{result.Errors.Count} errors:[/]");
            foreach (var error in result.Errors)
            {
                AnsiConsole.MarkupLine($"[red]  ✗[/] {error}");
            }
            AnsiConsole.WriteLine();
        }
    }

    /// <summary>
    /// Finds the v1 configuration file in the site directory.
    /// </summary>
    private static string? FindV1ConfigFile(string sitePath)
    {
        var possibleFiles = new[] { "snow.config", "snow.config.json" };
        
        foreach (var file in possibleFiles)
        {
            var fullPath = Path.Combine(sitePath, file);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    /// <summary>
    /// Shows help for the migrate command.
    /// </summary>
    private static void ShowMigrateHelp()
    {
        Console.WriteLine("USAGE:");
        Console.WriteLine("  sandra migrate <v1-site-path> [options]");
        Console.WriteLine();
        Console.WriteLine("ARGUMENTS:");
        Console.WriteLine("  <v1-site-path>          Path to Sandra.Snow v1 site directory");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  -o, --output <path>     Output path for migrated configuration");
        Console.WriteLine("  -i, --interactive       Interactive mode for ambiguous migrations");
        Console.WriteLine("  -f, --force             Continue migration despite validation errors");
        Console.WriteLine("  -h, --help              Show this help");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  sandra migrate ./old-blog                    # Migrate to sandra.json");
        Console.WriteLine("  sandra migrate ./old-blog --interactive      # Interactive migration");
        Console.WriteLine("  sandra migrate ./old-blog -o ./new-blog.json # Custom output path");
    }
}

/// <summary>
/// Options for the migrate command.
/// </summary>
public class MigrateOptions
{
    public string? SourcePath { get; set; }
    public string? OutputPath { get; set; }
    public bool Interactive { get; set; }
    public bool Force { get; set; }
    public bool ShowHelp { get; set; }
}
