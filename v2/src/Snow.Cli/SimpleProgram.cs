using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snow.Abstractions;
using Snow.Cli.DevServer;
using Snow.Cli.Infrastructure;
using Snow.Engine.Extensions;
using Snow.Engine.Incremental;
using Snow.Engine.Pipeline;
using Spectre.Console;

namespace Snow.Cli;

/// <summary>
/// Simplified program entry point for Stage 5.
/// </summary>
public static class SimpleProgram
{
    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            // Parse basic command line arguments
            var options = ParseArguments(args);
            
            AnsiConsole.MarkupLine("[green]Sandra.Snow v2.0.0-alpha[/]");
            AnsiConsole.MarkupLine("[dim]Modern static site generator built with .NET 10[/]");
            AnsiConsole.WriteLine();

            if (options.ShowHelp)
            {
                ShowHelp();
                return 0;
            }

            if (options.ShowVersion)
            {
                return 0; // Version already shown above
            }

            // Check for migrate command
            if (args.Contains("migrate"))
            {
                return await Commands.MigrateCommand.ExecuteAsync(args);
            }

            // Check for serve command
            if (args.Contains("serve") || args.Contains("--serve"))
            {
                return await ExecuteServe(options);
            }

            // Execute build
            return await ExecuteBuild(options);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] " + ex.Message);
            if (args.Contains("--verbose"))
            {
                AnsiConsole.WriteException(ex);
            }
            return 1;
        }
    }

    private static CliOptions ParseArguments(string[] args)
    {
        var options = new CliOptions();
        
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var lowerArg = arg.ToLowerInvariant();
            
            // Handle v1-style key=value arguments
            if (arg.Contains('='))
            {
                var parts = arg.Split('=', 2);
                var key = parts[0].ToLowerInvariant();
                var value = parts[1];
                
                switch (key)
                {
                    case "config":
                        options.ConfigPath = value;
                        continue;
                    case "debug":
                        options.Verbose = true;
                        continue;
                    case "server":
                        options.ServeMode = true;
                        continue;
                }
            }
            
            switch (lowerArg)
            {
                case "--help":
                case "-h":
                    options.ShowHelp = true;
                    break;
                case "--version":
                    options.ShowVersion = true;
                    break;
                case "--verbose":
                case "-v":
                case "--debug":
                    options.Verbose = true;
                    break;
                case "--config":
                case "-c":
                    if (i + 1 < args.Length)
                        options.ConfigPath = args[++i];
                    break;
                case "--output":
                case "-o":
                    if (i + 1 < args.Length)
                        options.OutputPath = args[++i];
                    break;
                case "--theme":
                case "-t":
                    if (i + 1 < args.Length)
                        options.Theme = args[++i];
                    break;
                case "--port":
                case "-p":
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var port))
                    {
                        options.Port = port;
                        i++;
                    }
                    break;
                case "--serve":
                case "--server":
                case "serve":
                    options.ServeMode = true;
                    break;
                case "--watch":
                case "-w":
                    options.WatchMode = true;
                    break;
                default:
                    // Treat first positional argument as config path
                    if (!arg.StartsWith("-") && string.IsNullOrEmpty(options.ConfigPath))
                    {
                        options.ConfigPath = arg;
                    }
                    break;
            }
        }

        return options;
    }

    private static async Task<int> ExecuteBuild(CliOptions options)
    {
        try
        {
            AnsiConsole.MarkupLine("[green]Building site...[/]");

            // Load configuration
            var settings = await ConfigurationLoader.LoadSettingsAsync(options.ConfigPath);

            // Apply overrides
            if (!string.IsNullOrEmpty(options.OutputPath))
            {
                settings.PostsOutputRaw = options.OutputPath;
                settings.PagesOutputRaw = options.OutputPath;
                settings.RefreshPaths();
            }

            if (!string.IsNullOrEmpty(options.Theme))
            {
                settings.Theme = options.Theme;
            }

            AnsiConsole.MarkupLine($"[dim]Blog:[/] {settings.BlogTitle}");
            AnsiConsole.MarkupLine($"[dim]Theme:[/] {settings.Theme}");
            AnsiConsole.MarkupLine($"[dim]Output:[/] {settings.PostsOutput}");
            AnsiConsole.WriteLine();

            // Setup DI and build
            var services = new ServiceCollection();
            services.AddSnow(settings, options.Verbose);

            using var serviceProvider = services.BuildServiceProvider();
            var generator = serviceProvider.GetRequiredService<SiteGenerator>();

            var progress = new SimpleProgressReporter();
            var generatorWithProgress = new SiteGenerator(
                serviceProvider.GetRequiredService<IPostParser>(),
                serviceProvider.GetRequiredService<IPageParser>(),
                serviceProvider.GetRequiredService<ITemplateRenderer>(),
                serviceProvider.GetRequiredService<IFileProvider>(),
                settings,
                serviceProvider.GetRequiredService<ILogger<SiteGenerator>>(),
                progress);

            await generatorWithProgress.GenerateAsync();

            AnsiConsole.MarkupLine("[green]✓ Build completed successfully![/]");
            return 0;
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine("[red]Build failed:[/]");
            AnsiConsole.WriteLine(ex.Message);
            return 1;
        }
        catch (DirectoryNotFoundException ex)
        {
            AnsiConsole.MarkupLine("[red]Build failed:[/]");
            AnsiConsole.WriteLine(ex.Message);
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine("[red]Build failed:[/]");
            AnsiConsole.WriteLine(ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Build failed:[/]");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            return 1;
        }
    }

    private static async Task<int> ExecuteServe(CliOptions options)
    {
        try
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => 
            {
                e.Cancel = true;
                cts.Cancel();
            };

            AnsiConsole.MarkupLine("[green]Building site for development server...[/]");

            // First, build the site (reuse existing build logic)
            var buildResult = await ExecuteBuild(options);
            if (buildResult != 0)
            {
                return buildResult;
            }

            // Load settings to get output path
            var settings = await ConfigurationLoader.LoadSettingsAsync(options.ConfigPath, cts.Token);
            
            if (!string.IsNullOrEmpty(options.OutputPath))
            {
                settings.PostsOutputRaw = options.OutputPath;
                settings.PagesOutputRaw = options.OutputPath;
                settings.RefreshPaths();
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]Starting development server...[/]");

            // Start development server with live reload if watch mode enabled
            var enableLiveReload = options.WatchMode;
            using var devServer = await Snow.Cli.DevServer.DevServer.StartAsync(
            settings.PostsOutput,
            options.Port,
            enableDirectoryBrowsing: true,
            enableLiveReload: enableLiveReload,
            autoOpen: true,
                cts.Token);

        // Setup file watching if enabled
            FileWatcher? fileWatcher = null;
            IncrementalSiteGenerator? incrementalGenerator = null;
        
        if (options.WatchMode)
        {
            AnsiConsole.MarkupLine("[yellow]👁[/] Setting up file watching...");
            
            // Setup incremental generator with dependency graph
            var services = new ServiceCollection();
            services.AddSnow(settings, options.Verbose);
            using var serviceProvider = services.BuildServiceProvider();
            
            var dependencyGraph = new DependencyGraph(serviceProvider.GetRequiredService<ILogger<DependencyGraph>>());
            incrementalGenerator = new IncrementalSiteGenerator(
                serviceProvider.GetRequiredService<SiteGenerator>(),
                dependencyGraph,
                serviceProvider.GetRequiredService<IPostParser>(),
                serviceProvider.GetRequiredService<IPageParser>(),
                serviceProvider.GetRequiredService<ITemplateRenderer>(),
                serviceProvider.GetRequiredService<IFileProvider>(),
                settings,
                serviceProvider.GetRequiredService<ILogger<IncrementalSiteGenerator>>());

            // Initialize from cache
            await incrementalGenerator.InitializeFromCacheAsync(cts.Token);

            // Setup file watcher
            fileWatcher = new FileWatcher(
                settings.CurrentDirectory, 
                serviceProvider.GetRequiredService<ILogger<FileWatcher>>());

            fileWatcher.FilesChanged += async (changedFiles) =>
            {
                try
                {
                    AnsiConsole.MarkupLine($"[dim]Files changed:[/] {string.Join(", ", changedFiles.Select(Path.GetFileName))}");
                    
                    var hasChanges = await incrementalGenerator.IncrementalBuildAsync(changedFiles, cts.Token);
                    if (hasChanges)
                    {
                        AnsiConsole.MarkupLine("[green]✓[/] Site rebuilt");
                        await devServer.NotifyReloadAsync(changedFiles);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Build error:[/] {ex.Message}");
                }
            };

            fileWatcher.Start();
            AnsiConsole.MarkupLine("[green]👁[/] File watching started");
        }

        AnsiConsole.WriteLine();
        
        if (enableLiveReload)
        {
            AnsiConsole.MarkupLine("[green]⚡[/] Live reload enabled");
        }
        
        AnsiConsole.MarkupLine("[dim]Press Ctrl+C to stop the server[/]");

        try
        {
            // Wait for shutdown
            await devServer.WaitForShutdownAsync(cts.Token);
        }
        finally
        {
            fileWatcher?.Dispose();
        }

            AnsiConsole.MarkupLine("[yellow]Development server stopped[/]");
            return 0;
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[yellow]Server cancelled[/]");
            return 130;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Server failed:[/]");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            return 1;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("USAGE:");
        Console.WriteLine("  sandra [command] [options]");
        Console.WriteLine();
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  migrate <path>          Migrate v1 site to v2 format");
        Console.WriteLine("  serve                   Build and start development server");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  -c, --config <path>     Configuration file path");
        Console.WriteLine("  -o, --output <path>     Output directory");
        Console.WriteLine("  -t, --theme <name>      Theme name");
        Console.WriteLine("  -p, --port <number>     Port for development server (default: auto)");
        Console.WriteLine("  -w, --watch             Enable file watching with live reload");
        Console.WriteLine("  -v, --verbose           Enable verbose output");
        Console.WriteLine("      --version           Show version");
        Console.WriteLine("  -h, --help              Show this help");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  sandra                               # Build with default settings");
        Console.WriteLine("  sandra migrate ./old-blog            # Migrate v1 site to v2");
        Console.WriteLine("  sandra serve                         # Build and start development server");
        Console.WriteLine("  sandra serve --watch                 # Start server with live reload");
        Console.WriteLine("  sandra serve --port 8080 --watch     # Custom port with file watching");
        Console.WriteLine("  sandra --config ./custom/sandra.json # Build with custom config");
        Console.WriteLine("  sandra --output ./dist --verbose     # Build to custom output with verbose logging");
    }
}

/// <summary>
/// Command line options for the simplified CLI.
/// </summary>
public class CliOptions
{
    public bool ShowHelp { get; set; }
    public bool ShowVersion { get; set; }
    public bool Verbose { get; set; }
    public string? ConfigPath { get; set; }
    public string? OutputPath { get; set; }
    public string? Theme { get; set; }
    public int Port { get; set; } = 0; // 0 = auto-discover
    public bool ServeMode { get; set; }
    public bool WatchMode { get; set; }
}

/// <summary>
/// Simple progress reporter for the basic CLI.
/// </summary>
public class SimpleProgressReporter : IProgress<GeneratorProgress>
{
    public void Report(GeneratorProgress value)
    {
        if (!string.IsNullOrEmpty(value.CurrentFile))
        {
            AnsiConsole.MarkupLine($"[dim]Processing:[/] {value.CurrentFile}");
        }
        else if (value.IsComplete)
        {
            AnsiConsole.MarkupLine($"[green]Completed:[/] {value.ProcessedFiles} files processed");
            if (value.ErrorCount > 0)
            {
                AnsiConsole.MarkupLine($"[yellow]Warnings:[/] {value.ErrorCount} errors");
            }
        }
    }
}
