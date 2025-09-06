using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;
using Spectre.Console;

namespace Snow.Cli.DevServer;

/// <summary>
/// Development web server for serving static sites with live reload capabilities.
/// </summary>
public class DevServer : IDisposable, IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly string _contentPath;
    private readonly int _port;
    private readonly ILogger<DevServer> _logger;

    private DevServer(WebApplication app, string contentPath, int port, ILogger<DevServer> logger)
    {
        _app = app;
        _contentPath = contentPath;
        _port = port;
        _logger = logger;
    }

    /// <summary>
    /// Creates and starts a development server.
    /// </summary>
    /// <param name="contentPath">Path to the generated static site content.</param>
    /// <param name="port">Port to listen on (0 for auto-assign).</param>
    /// <param name="enableDirectoryBrowsing">Whether to enable directory browsing.</param>
    /// <param name="autoOpen">Whether to automatically open browser.</param>
    /// <param name="cancellationToken">Cancellation token for startup.</param>
    /// <returns>A running development server instance.</returns>
    public static async Task<DevServer> StartAsync(
        string contentPath, 
        int port = 0, 
        bool enableDirectoryBrowsing = true,
        bool autoOpen = true,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(contentPath))
        {
            throw new DirectoryNotFoundException($"Content directory not found: {contentPath}");
        }

        // Find available port if not specified (0 = auto-assign)
        var actualPort = port > 0 ? port : FindAvailablePort(5498);
        
        var builder = WebApplication.CreateBuilder();
        
        // Configure services
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning); // Quiet by default
        });

        // Configure Kestrel
        builder.WebHost.UseKestrel(options =>
        {
            options.ListenLocalhost(actualPort);
        });

        var app = builder.Build();
        var logger = app.Services.GetRequiredService<ILogger<DevServer>>();

        // Configure middleware
        ConfigureMiddleware(app, contentPath, enableDirectoryBrowsing, logger);

        // Start the server
        await app.StartAsync(cancellationToken);
        
        var serverUrl = $"http://localhost:{actualPort}";
        
        AnsiConsole.MarkupLine($"[green]Development server started[/]");
        AnsiConsole.MarkupLine($"[cyan]Local:[/] {serverUrl}");
        AnsiConsole.MarkupLine($"[dim]Serving:[/] {Path.GetFullPath(contentPath)}");
        
        if (enableDirectoryBrowsing)
        {
            AnsiConsole.MarkupLine("[dim]Directory browsing:[/] enabled");
        }

        // Auto-open browser
        if (autoOpen)
        {
            try
            {
                OpenBrowser(serverUrl);
                AnsiConsole.MarkupLine("[dim]Browser opened automatically[/]");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to open browser automatically");
            }
        }

        return new DevServer(app, contentPath, actualPort, logger);
    }

    /// <summary>
    /// Configures the middleware pipeline for the development server.
    /// </summary>
    private static void ConfigureMiddleware(WebApplication app, string contentPath, bool enableDirectoryBrowsing, ILogger logger)
    {
        // Development-friendly caching
        app.Use(async (context, next) =>
        {
            // Disable caching for development
            context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache"; 
            context.Response.Headers["Expires"] = "0";
            
            await next();
        });

        var fileProvider = new PhysicalFileProvider(Path.GetFullPath(contentPath));

        // 1. Default files (changes path before static file handling)
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = fileProvider,
            DefaultFileNames = ["index.html", "index.htm", "default.html"]
        });

        // 2. Static file serving
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider,
            ContentTypeProvider = CreateContentTypeProvider(),
            ServeUnknownFileTypes = true, // Allow unknown file types for development
            DefaultContentType = "application/octet-stream",
            OnPrepareResponse = context =>
            {
                // Additional no-cache headers for development
                context.Context.Response.Headers["Cache-Control"] = "no-store";
            }
        });

        // 3. Directory browsing (development convenience)
        if (enableDirectoryBrowsing)
        {
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = fileProvider
            });
        }

        // 4. Fallback for SPA-style routing (serve index.html for unmatched routes)
        app.MapFallbackToFile("index.html", new StaticFileOptions
        {
            FileProvider = fileProvider
        });

        logger.LogDebug("Configured development server middleware for: {ContentPath}", contentPath);
    }

    /// <summary>
    /// Creates a content type provider with web development-friendly MIME types.
    /// </summary>
    private static FileExtensionContentTypeProvider CreateContentTypeProvider()
    {
        var provider = new FileExtensionContentTypeProvider();
        
        // Add additional MIME types for modern web development
        provider.Mappings[".webmanifest"] = "application/manifest+json";
        provider.Mappings[".json"] = "application/json";
        provider.Mappings[".wasm"] = "application/wasm";
        provider.Mappings[".br"] = "application/brotli";
        
        return provider;
    }

    /// <summary>
    /// Finds an available port starting from the specified port.
    /// </summary>
    private static int FindAvailablePort(int startingPort)
    {
        for (int port = startingPort; port < startingPort + 100; port++)
        {
            try
            {
                using var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch (SocketException)
            {
                // Port is in use, try next one
            }
        }

        throw new InvalidOperationException($"No available ports found in range {startingPort}-{startingPort + 99}");
    }

    /// <summary>
    /// Opens the default browser to the specified URL.
    /// </summary>
    private static void OpenBrowser(string url)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                System.Diagnostics.Process.Start("open", url);
            }
            else if (OperatingSystem.IsLinux())
            {
                System.Diagnostics.Process.Start("xdg-open", url);
            }
        }
        catch
        {
            // Ignore browser opening failures
        }
    }

    /// <summary>
    /// Gets the server URL.
    /// </summary>
    public string ServerUrl => $"http://localhost:{_port}";

    /// <summary>
    /// Waits for the server to shut down.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for shutdown.</param>
    public async Task WaitForShutdownAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _app.WaitForShutdownAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Development server shutdown requested");
        }
    }

    /// <summary>
    /// Stops the development server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for shutdown.</param>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _app.StopAsync(cancellationToken);
        _logger.LogInformation("Development server stopped");
    }

    /// <summary>
    /// Disposes the development server asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_app != null)
        {
            await _app.DisposeAsync();
        }
        
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the development server synchronously.
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
        GC.SuppressFinalize(this);
    }
}
